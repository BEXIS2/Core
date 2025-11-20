using BExIS.App.Bootstrap.Attributes;
using BExIS.Dim.Entities.Export.GBIF;
using BExIS.Dim.Entities.Mappings;
using BExIS.Dim.Entities.Publications;
using BExIS.Dim.Helpers.GBIF;
using BExIS.Dim.Helpers.Mappings;
using BExIS.Dim.Services;
using BExIS.Dlm.Entities.Data;
using BExIS.Dlm.Entities.MetadataStructure;
using BExIS.Dlm.Services.Data;
using BExIS.Modules.Dim.UI.Models.Export;
using BExIS.Security.Entities.Requests;
using BExIS.UI.Helpers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Vaiona.Web.Mvc.Modularity;
using static System.Net.WebRequestMethods;

namespace BExIS.Modules.Dim.UI.Controllers
{
    public class GbifController : Controller
    {
        // GET: Gbif
        public ActionResult Index()
        {
            string module = "DIM";

            ViewData["app"] = SvelteHelper.GetApp(module);
            ViewData["start"] = SvelteHelper.GetStart(module);

            return View();
        }

        [JsonNetFilter]
        [HttpGet]
        public JsonResult Load()
        {
            List<GbifPublicationModel> pubs = new List<GbifPublicationModel>();
            using (var publicationManager = new PublicationManager())
            using (var datasetManager = new DatasetManager())
            {
                // get all publications belong to gbif
                var repository = publicationManager.GetRepository().Where(r => r.Name.Equals("GBIF")).FirstOrDefault();
                var publications = new List<Publication>();

                if(repository!=null)
                    publications = publicationManager.GetPublication().Where(p => p.Repository.Id.Equals(repository.Id)).ToList();

                foreach (var p in publications)
                {
                    int vNr = datasetManager.GetDatasetVersionNr(p.DatasetVersion.Id);

                    pubs.Add(new GbifPublicationModel()
                    {
                        PublicationId = p.Id,
                        DatasetVersionId = p.DatasetVersion.Id,
                        DatasetId = p.DatasetVersion.Dataset.Id,
                        BrokerRef = p.Broker.Id,
                        RepositoryRef = p.Repository.Id,
                        Title = p.DatasetVersion.Title,
                        Status = p.Status,
                        Response = p.Response,
                        Link = p.FilePath,
                        Type = p.Broker.Type,
                        DatasetVersionNr = vNr
                    });
                }

                return Json(pubs, JsonRequestBehavior.AllowGet);
            }
        }

        [JsonNetFilter]
        [HttpPost]
        public async Task<JsonResult> RegisterDataset(long publicationId)
        {
            using (var publicationManager = new PublicationManager())
            using (var datasetManager = new DatasetManager())
            {
                var publication = publicationManager.GetPublication(publicationId);
                var dataset = datasetManager.GetDataset(publication.DatasetVersion.Dataset.Id);
                var datasetVersion = datasetManager.GetDatasetVersion(publication.DatasetVersion.Id);
                var type = (GbifDataType)Enum.Parse(typeof(GbifDataType), publication.Broker.Type, true);
                var brokerId = publication.Broker.Id;
                var datasetVersionId = publication.DatasetVersion.Id;
                var url = Request.Url.Host;
                var port = Request.Url.Port;
                var protocol = Request.Url.Scheme;
                string doi = "";

                // load settings
                GBFICrendentials credentials = ModuleManager.GetModuleSettings("DIM").GetValueByKey<GBFICrendentials>("gbifapicredentials");

                // create dataset in gbif
                GbifServiceManager gbifServiceManager = new GbifServiceManager(credentials);
                
                // register dataset
                HttpResponseMessage res = await gbifServiceManager.CreateDataset(type, datasetVersion.Title);
                if (res.IsSuccessStatusCode) // success
                {
                    string result = res.Content.ReadAsStringAsync().Result.Replace("\"", "");
                    string downloadPath = Url.Action("DownloadZip", "Submission", new { brokerId = brokerId, datasetversionid = datasetVersionId });

                    downloadPath = getDownloadUrl(Request.Url, brokerId, datasetVersionId);

                    // get dataset
                    // grab doi and store it in externel link + type
                    HttpResponseMessage resGetDataset = await gbifServiceManager.GetDataset(result);
                    if (res.IsSuccessStatusCode) // success
                    {
                        var gbfioGetDatasetResult = JsonConvert.DeserializeObject<GbifGetDatasetResponce>(resGetDataset.Content.ReadAsStringAsync().Result);
                        doi = gbfioGetDatasetResult.DOI;
                    }

                    // add endpoint
                    HttpResponseMessage resEndpoint = await gbifServiceManager.AddEndpoint(result, downloadPath);
                    if (res.IsSuccessStatusCode) // success
                    {
                        publication.Status = PublicationStatus.Registered.ToString();
                        publication.FilePath = downloadPath;
                        publication.Response = result.Replace('\"', ' ').Trim();
                        publication.ExternalLink = "https://doi.org/"+doi;
                        publication.ExternalLinkType = "DOI";
                        publicationManager.UpdatePublication(publication);

                        // check if doi mapping to metadata exist, and add it also there
                        setDoiInMetadataIfExist(datasetVersion, doi, datasetManager);

                    }
                    else // fail
                    { 
                        throw new Exception("Error: " + resEndpoint.Content.ReadAsStringAsync().Result);
                    }


                    return Json(true, JsonRequestBehavior.AllowGet);
                }
                else // fail
                { 
                    throw new Exception("Error: " + res.Content.ReadAsStringAsync().Result);
                }
            }
        }

        [JsonNetFilter]
        [HttpPost]
        public async Task<JsonResult> DeleteDataset(string key, long publicationId)
        {
            using (var publicationManager = new PublicationManager())
            using (var datasetManager = new DatasetManager())
            {
                var publication = publicationManager.GetPublication(publicationId);

                // load settings
                GBFICrendentials credentials = ModuleManager.GetModuleSettings("DIM").GetValueByKey<GBFICrendentials>("gbifapicredentials");

                // create dataset in gbif
                GbifServiceManager gbifServiceManager = new GbifServiceManager(credentials);

                // register dataset
                HttpResponseMessage res = await gbifServiceManager.DeleteDataset(key);
                if (res.IsSuccessStatusCode) // success
                {
                    string result = res.Content.ReadAsStringAsync().Result;
                    publication.Status = PublicationStatus.Open.ToString();
                    publication.FilePath = "";
                    publication.Response = result.Replace('\"', ' ').Trim();
                    publicationManager.UpdatePublication(publication);


                    return Json(true, JsonRequestBehavior.AllowGet);
                }
                else // fail
                {
                    throw new Exception("Error: " + res.Content.ReadAsStringAsync().Result);
                }
            }
        }

        private string getDownloadUrl(Uri url, long brokerId, long datasetVersionId)
        {
            string protocolAndHost = url.GetLeftPart(UriPartial.Authority);

            // If the port is not the default port, it will be included.
            // If you need to ensure the port is always included, you can manually append it.
            if (url.Port != 80 && url.Scheme == "http" || url.Port != 443 && url.Scheme == "https")
            {
                protocolAndHost = $"https://{url.Host}:{url.Port}";
            }
            else
            {
                protocolAndHost = $"https://{url.Host}";
            }

          

            return string.Format("{0}/DIM/Submission/DownloadZip?brokerId={1}&datasetversionid={2}", protocolAndHost, brokerId, datasetVersionId); ;
        }

        private bool setDoiInMetadataIfExist(DatasetVersion version, string doi, DatasetManager datasetManager)
        {
            var sourceId = (int)Key.DOI;
            var sourceType = LinkElementType.Key;
            var metadataStructureId = version.Dataset.MetadataStructure.Id;

            LinkElement target = null;

            //// check if cardinality of target usage > 1

            MappingUtils.HasTarget(sourceId, metadataStructureId, out target);

            if (target != null)
            {
                datasetManager.UpdateSingleValueInMetadata(version.Id, target.XPath, doi);

                return true;
            }

            return false;
        }
    }
}