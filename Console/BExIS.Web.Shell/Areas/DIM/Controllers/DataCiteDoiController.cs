using BExIS.Dim.Entities.Mappings;
using BExIS.Dim.Entities.Publications;
using BExIS.Dim.Helpers.Mappings;
using BExIS.Dim.Services.Publications;
using BExIS.Dim.Services.Mappings;
using BExIS.Dlm.Entities.Data;
using BExIS.Dlm.Services.Data;
using BExIS.Modules.Dim.UI.Helpers;
using BExIS.Modules.Dim.UI.Models;
using BExIS.Security.Services.Authorization;
using BExIS.Security.Services.Objects;
using BExIS.Security.Services.Utilities;
using BExIS.Xml.Helpers;
using Newtonsoft.Json;
using RestSharp;
using RestSharp.Authenticators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Web.UI.WebControls;
using System.Xml;
using System.Xml.Serialization;
using Vaelastrasz.Library.Models;
using Vaelastrasz.Library.Services;
using Vaiona.Web.Mvc;

namespace BExIS.Modules.Dim.UI.Controllers
{
    public class DataCiteDOIController : BaseController
    {
        public DataCiteDOIController()
        {
        }

        public ActionResult _denyDoi(long datasetVersionId)
        {
            using (DatasetManager datasetManager = new DatasetManager())
            using (PublicationManager publicationManager = new PublicationManager())
            {
                DatasetVersion datasetVersion = datasetManager.GetDatasetVersion(datasetVersionId);

                Publication publication = publicationManager.Publications.Where(p => p.DatasetVersion.Id.Equals(datasetVersion.Id)).FirstOrDefault();

                publication.Status = "DOI Denied";

                var updated = publicationManager.Update(publication);

                if(updated)
                {
                    EmailService es = new EmailService();
                    List<string> tmp = null;
                    string title = new XmlDatasetHelper().GetInformationFromVersion(datasetVersion.Id, NameAttributeValues.title);
                    string subject = "DOI Request for Dataset " + title + "(" + datasetVersion.Dataset.Id + ")";
                    string body = "<p>DOI reqested for dataset " + title + "(" + datasetVersion.Dataset.Id + "), was denied by the Datamanager.</p>";

                    tmp = new List<string>();
                    tmp = MappingUtils.GetValuesFromMetadata((int)Key.Email, LinkElementType.Key, datasetVersion.Dataset.MetadataStructure.Id, XmlUtility.ToXDocument(datasetVersion.Metadata));

                    foreach (string s in tmp)
                    {
                        string e = s.Trim();
                        es.Send(subject, body, e);
                    }
                }

                return PartialView("_requestRow", new PublicationModel()
                {
                    Broker = new BrokerModel(publication.Broker.Id, publication.Broker.Name, new List<string>() { publication.Repository.Name }, publication.Broker.Link),
                    DataRepo = publication.Repository.Name,
                    DatasetVersionId = publication.DatasetVersion.Id,
                    CreationDate = publication.Timestamp,
                    ExternalLink = publication.ExternalLink,
                    FilePath = publication.FilePath,
                    Status = publication.Status,
                    DatasetId = publication.DatasetVersion.Dataset.Id,
                    DatasetVersionNr = datasetManager.GetDatasetVersionNr(publication.DatasetVersion.Id)
                });
            }
        }

        public ActionResult _grantDoi(long datasetVersionId)
        {
            try
            {
                using (var datasetManager = new DatasetManager())
                using (var conceptManager = new ConceptManager())
                {

                }
            }
            catch(Exception ex)
            {
                throw;
            }
            
            
            using (DatasetManager datasetManager = new DatasetManager())
            using (PublicationManager publicationManager = new PublicationManager())
            using (EntityPermissionManager entityPermissionManager = new EntityPermissionManager())
            using (EntityManager entityManager = new EntityManager())
            {
                // dataset - version
                DatasetVersion datasetVersion = datasetManager.GetDatasetVersion(datasetVersionId);

                long versionNo = datasetManager.GetDatasetVersions(datasetVersion.Dataset.Id).OrderBy(d => d.Timestamp).Count();

                Publication publication = publicationManager.Publications.Where(p => p.DatasetVersion.Id.Equals(datasetVersion.Id)).FirstOrDefault();

                DatasetVersion latestDatasetVersion = datasetManager.GetDatasetLatestVersion(datasetVersion.Dataset.Id);

                string datasetUrl = new Uri(new Uri(Request.Url.GetLeftPart(UriPartial.Authority)), Url.Content("~/ddm/Data/ShowData/" + datasetVersion.Dataset.Id).ToString()).ToString();

                //SettingsHelper settingsHelper = new SettingsHelper();
                //if (settingsHelper.KeyExist("proxy") && settingsHelper.KeyExist("token"))
                //{
                //    // helper
                //    var datacitedoihelper = new DataCiteDOIHelper();

                //    var client = new RestClient(settingsHelper.GetValue("proxy"));
                //    client.Authenticator = new JwtAuthenticator(settingsHelper.GetValue("token"));

                //    // doi
                //    var placeholders = datacitedoihelper.CreatePlaceholders(datasetVersion, settingsHelper.GetDataCiteSettings("placeholders"));

                //    var doi_request = new RestRequest($"api/dois", Method.POST).AddJsonBody(placeholders);
                //    //CreateDOIModel doi = JsonConvert.DeserializeObject<CreateDOIModel>(client.Execute(doi_request).Content);

                //    // Model
                //    var mappings = settingsHelper.GetDataCiteDOISettings("mappings");

                //    var model = datacitedoihelper.CreateDataCiteModel(datasetVersion, mappings);
                //    model.Data.Attributes.Doi = "";

                //    var datacite_request = new RestRequest($"api/datacite", Method.POST).AddJsonBody(JsonConvert.SerializeObject(model));
                //    var response = client.Execute(datacite_request);

                //    if (response.StatusCode != System.Net.HttpStatusCode.Created)
                //    {
                //        return PartialView("_requestRow", new PublicationModel()
                //        {
                //            Broker = new BrokerModel(publication.Broker.Name, new List<string>() { publication.Repository.Name }, publication.Broker.Link),
                //            DataRepo = publication.Repository.Name,
                //            DatasetVersionId = publication.DatasetVersion.Id,
                //            CreationDate = publication.Timestamp,
                //            ExternalLink = publication.ExternalLink,
                //            FilePath = publication.FilePath,
                //            Status = publication.Status,
                //            DatasetId = publication.DatasetVersion.Dataset.Id,
                //            DatasetVersionNr = datasetManager.GetDatasetVersionNr(publication.DatasetVersion.Id)
                //        });
                //    }

                //    //var response_content = JsonConvert.DeserializeObject<ReadDataCiteModel>(response.Content);
                //    var response_content = JsonConvert.DeserializeObject(response.Content);

                //    publication.DatasetVersion = latestDatasetVersion;
                //    //  publication.Doi = doi;
                //    publication.ExternalLink = "response_content.Id";
                //    publication.Status = "DOI Registered";

                //    publication = publicationManager.UpdatePublication(publication);

                //    EmailService es = new EmailService();
                //    List<string> tmp = null;
                //    string title = new XmlDatasetHelper().GetInformationFromVersion(latestDatasetVersion.Id, NameAttributeValues.title);
                //    string subject = "DOI Request for Dataset " + title + "(" + latestDatasetVersion.Dataset.Id + ")";
                //    string body = "<p>DOI reqested for dataset <a href=\"" + datasetUrl + "\">" + title + "(" + latestDatasetVersion.Dataset.Id + ")</a>, was granted by the Datamanager.</p>" +
                //        "<p>The doi is<a href=\"https://doi.org/" + "doi.DOI" + "\">" + "doi.DOI" + "</a></p>";

                //    tmp = new List<string>();
                //    List<string> emails = new List<string>();
                //    tmp = MappingUtils.GetValuesFromMetadata((int)Key.Email, LinkElementType.Key, latestDatasetVersion.Dataset.MetadataStructure.Id, XmlUtility.ToXDocument(latestDatasetVersion.Metadata));

                //    foreach (string s in tmp)
                //    {
                //        var email = s.Trim();
                //        if (!string.IsNullOrEmpty(email) && !emails.Contains(email))
                //        {
                //            emails.Add(email);
                //        }

                //    }

                //    es.Send(subject, body, emails);
                //    es.Send(subject, body, ConfigurationManager.AppSettings["SystemEmail"]);

                //    long entityId = entityManager.Entities.Where(e => e.Name.ToLower().Equals("dataset")).FirstOrDefault().Id;

                //    EntityPermission entityPermission = entityPermissionManager.Find(null, entityId, datasetVersion.Dataset.Id);

                //    if (entityPermission == null)
                //    {
                //        entityPermissionManager.Create(null, entityId, datasetVersion.Dataset.Id, (int)RightType.Read);
                //    }
                //    else
                //    {
                //        entityPermission.Rights = (int)RightType.Read;
                //        entityPermissionManager.Update(entityPermission);
                //    }

                //    if (this.IsAccessible("DDM", "SearchIndex", "ReIndexSingle"))
                //    {
                //        var x = this.Run("DDM", "SearchIndex", "ReIndexSingle", new RouteValueDictionary() { { "id", datasetVersion.Dataset.Id } });
                //    }
                //}

                return PartialView("_requestRow", new PublicationModel()
                {
                    Broker = new BrokerModel(publication.Broker.Id, publication.Broker.Name, new List<string>() { publication.Repository.Name }, publication.Broker.Link),
                    DataRepo = publication.Repository.Name,
                    DatasetVersionId = publication.DatasetVersion.Id,
                    CreationDate = publication.Timestamp,
                    ExternalLink = publication.ExternalLink,
                    FilePath = publication.FilePath,
                    Status = publication.Status,
                    DatasetId = publication.DatasetVersion.Dataset.Id,
                    DatasetVersionNr = datasetManager.GetDatasetVersionNr(publication.DatasetVersion.Id)
                });
            }
        }

        public async Task<ActionResult> Accept(long publicationId)
        {
            try
            {
                using (var datasetManager = new DatasetManager())
                using (var publicationManager = new PublicationManager())
                using (var conceptManager = new ConceptManager())
                {
                    var publication = publicationManager.FindById(publicationId);

                    if (publication == null)
                        throw new ArgumentException("Publication does not exist", nameof(publicationId));

                    // Preparation
                    var settingsHelper = new SettingsHelper();
                    var placeholders = settingsHelper.GetVaelastraszPlaceholders();
                    var mappings = settingsHelper.GetVaelastraszMappings();

                    // Placeholders for DOI
                    foreach (var placeholder in placeholders)
                    {
                        switch(placeholder.Value)
                        {
                            case "{DatasetId}":
                                placeholders[placeholder.Key] = publication.DatasetVersion.Dataset.Id.ToString();
                                break;

                            case "{VersionId}":
                                placeholders[placeholder.Key] = publication.DatasetVersion.Id.ToString();
                                break;

                            case "{VersionNumber}":
                                placeholders[placeholder.Key] = publication.DatasetVersion.VersionNo.ToString();
                                break;

                            case "{VersionName}":
                                placeholders[placeholder.Key] = publication.DatasetVersion.VersionName.ToString();
                                break;

                            default:
                                break;
                        }
                    }

                    var configuration = new Configuration(publication.Broker.UserName, publication.Broker.Password, publication.Broker.Host);

                    var doiService = new DOIService(configuration);

                    var createSuffixModel = new CreateSuffixModel()
                    {
                        Placeholders = placeholders
                    };
                    var doi = await doiService.GenerateAsync(createSuffixModel);

                    // Mappings for Metadata
                    var concept = conceptManager.MappingConceptRepo.Query(c => c.Name.ToLower() == "datacite").FirstOrDefault();

                    var model = new CreateDataCiteModel();

                    if (concept == null)
                        return View("Create", model);

                    var xml = MappingUtils.GetConceptOutput(publication.DatasetVersion.Dataset.MetadataStructure.Id, concept.Id, publication.DatasetVersion.Metadata);

                    CreateDataCiteModel response = new CreateDataCiteModel();

                    XmlSerializer serializer = new XmlSerializer(typeof(CreateDataCiteDataModel));
                    using (XmlReader reader = new XmlNodeReader(xml))
                    {
                        model = (CreateDataCiteModel)serializer.Deserialize(reader);
                    }

                    // DOI
                    model.SetDoi(doi);

                    // Specific Mappings
                    foreach (var mapping in mappings)
                    {
                        switch (mapping.Key)
                        {
                            case "URL":
                                var scheme = Request.Url.Scheme;
                                var host = Request.Url.Host;
                                var port = Request.Url.IsDefaultPort ? "" : ":" + Request.Url.Port;
                                var url = mapping.Value.Replace(placeholders);
                                model.SetUrl($"{scheme}://{host}{port}/ddm/Data/ShowData/{url}");
                                break;

                            case "Version":
                                model.SetVersion(mapping.Value.Replace(placeholders));
                                break;

                            default:
                                break;
                        }
                    }

                    // Vaelastrasz
                    var dataCiteService = new DataCiteService(configuration);
                    var dataCiteResponse = await dataCiteService.CreateAsync(model);

                    // Publication Status


                    // E-Mail

                    return null;
                }
            }
            catch(Exception ex)
            {
                throw;
            }
        }

        public ActionResult Reject(long publicationId)
        {
            return null;
        }

        [HttpPost]
        public ActionResult Accept(string s)
        {
            return View();
        }

        public async Task<ActionResult> Create(long datasetVersionId)
        {
            //try
            //{
            //    using (var datasetManager = new DatasetManager())
            //    using (var conceptManager = new ConceptManager())
            //    {
            //        var datasetVersion = datasetManager.GetDatasetVersion(datasetVersionId);

            //        //var dataCiteDOIHelper = new DataCiteDOIHelper();
            //        var settingsHelper = new SettingsHelper();
            //        var placeholders = new Dictionary<string, string>(); //<dataCiteDOIHelper.CreatePlaceholders(datasetVersion, settingsHelper.GetDataCiteDOISettings("placeholders"));

            //        // Creation of DOI

            //        var doiService = new DOIService(_configuration);

            //        var createSuffixModel = new CreateSuffixModel()
            //        {
            //            Placeholders = placeholders
            //        };
            //        var doi = await doiService.GenerateAsync(createSuffixModel);

            //        var concept = conceptManager.MappingConceptRepo.Query(c => c.Name.ToLower() == "datacitedoi").FirstOrDefault();

            //        var model = new CreateDataCiteDOIModel();

            //        if (concept == null)
            //            return View("Create", model);

            //        var xml = MappingUtils.GetConceptOutput(datasetVersion.Dataset.MetadataStructure.Id, concept.Id, datasetVersion.Metadata);

            //        CreateDataCiteModel response = new CreateDataCiteModel();

            //        XmlSerializer serializer = new XmlSerializer(typeof(CreateDataCiteDataModel));
            //        using (XmlReader reader = new XmlNodeReader(xml))
            //        {
            //            model.DataCiteModel = (CreateDataCiteModel)serializer.Deserialize(reader);
            //        }

            //        //var mappings = settingsHelper.GetDataCiteSettings("mappings");

            //        // mappings
            //        // update values from settings afterwards
            //        //model.DataCiteModel.UpdateCreateDataCiteModel(mappings, placeholders);
            //        //model.DataCiteModel.Data.Attributes.Doi = $"{doi}";

            //        return View("Create", model);
            //    }
            //}
            //catch (Exception ex)
            //{
            //    return null;
            //}

            return View();
        }

        [HttpPost]
        public ActionResult Create(CreateDataCiteDOIModel model)
        {
            using (var datasetManager = new DatasetManager())
            {
                var datasetVersion = datasetManager.GetDatasetVersion(model.DatasetVersionId);

                var settingsHelper = new SettingsHelper();
                //var datacitedoihelper = new DataCiteDOIHelper();

                var placeholders = new List<string>();// datacitedoihelper.CreatePlaceholders(datasetVersion, settingsHelper.GetDataCiteDOIPlaceholders());

                //var client = new RestClient(_credentials.Host);
                //client.Authenticator = new HttpBasicAuthenticator(_credentials.Username, _credentials.Password);

                //var doi_request = new RestRequest($"api/dois", Method.POST).AddJsonBody(placeholders);
                //var doi = JsonConvert.DeserializeObject<ReadDOIModel>(client.Execute(doi_request).Content);
            }

            return View();
        }

        [HttpPost]
        public ActionResult Delete(string s)
        {
            return View();
        }

        public ActionResult index()
        {
            List<PublicationModel> model = new List<PublicationModel>();

            using (DatasetManager datasetManager = new DatasetManager())
            using (PublicationManager publicationManager = new PublicationManager())
            using (var brokerManager = new BrokerManager())
            {
                Broker broker = brokerManager.FindByName("vaelastrasz").FirstOrDefault();
                List<Publication> publications = publicationManager.Publications.Where(p => p.Broker.Name.ToLower().Equals(broker.Name.ToLower())).ToList();

                foreach (Publication p in publications)
                {
                    model.Add(new PublicationModel()
                    {
                        Broker = new BrokerModel(broker.Id, broker.Name, new List<string>() { p.Repository.Name }, broker.Link),
                        DataRepo = p.Repository.Name,
                        DatasetVersionId = p.DatasetVersion.Id,
                        CreationDate = p.Timestamp,
                        ExternalLink = p.ExternalLink,
                        FilePath = p.FilePath,
                        Status = p.Status,
                        DatasetId = p.DatasetVersion.Dataset.Id,
                        DatasetVersionNr = datasetManager.GetDatasetVersionNr(p.DatasetVersion.Id)
                    });
                }
            }

            return View(model);
        }

        [HttpPost]
        public ActionResult Reject(string s)
        {
            return View();
        }

        public ActionResult Update()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Update(string s)
        {
            return View();
        }
    }
}