using BExIS.App.Bootstrap.Attributes;
using BExIS.Dim.Entities.Export.GBIF;
using BExIS.Dim.Entities.Publications;
using BExIS.Dim.Helpers.GBIF;
using BExIS.Dim.Services;
using BExIS.Dlm.Services.Data;
using BExIS.Modules.Dim.UI.Models.Export;
using BExIS.UI.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Vaiona.Web.Mvc.Modularity;

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

                // load settings
                GBFICrendentials credentials = ModuleManager.GetModuleSettings("DIM").GetValueByKey<GBFICrendentials>("gbifapicredentials");

                // create dataset in gbif
                GbifServiceManager gbifServiceManager = new GbifServiceManager(credentials);
                
                // register dataset
                HttpResponseMessage res = await gbifServiceManager.CreateDataset(type, datasetVersion.Title);
                if (res.IsSuccessStatusCode) // success
                {
                    string result = res.Content.ReadAsStringAsync().Result;
                    string downloadPath = string.Format("{0}:{1}/DIM/Submission/DownloadZip?brokerId={2}&datasetversionid={3}", url,port, brokerId, datasetVersionId);


                    // add endpoint
                    HttpResponseMessage resEndpoint = await gbifServiceManager.AddEndpoint(result, downloadPath);
                    if (res.IsSuccessStatusCode) // success
                    {
                        publication.Status = PublicationStatus.Registered.ToString();
                        publication.FilePath = downloadPath;
                        publication.Response = result.Replace('\"', ' ').Trim();
                        publicationManager.UpdatePublication(publication);
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
    }
}