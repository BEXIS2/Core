using BExIS.Dim.Entities.Mapping;
using BExIS.Dim.Entities.Publication;
using BExIS.Dim.Helpers;
using BExIS.Dim.Helpers.Mapping;
using BExIS.Dim.Services;
using BExIS.Dlm.Entities.Data;
using BExIS.Dlm.Services.Data;
using BExIS.Modules.Dim.UI.Helpers;
using BExIS.Modules.Dim.UI.Models;
using BExIS.Security.Entities.Authorization;
using BExIS.Security.Services.Authorization;
using BExIS.Security.Services.Objects;
using BExIS.Security.Services.Utilities;
using BExIS.Xml.Helpers;
using Newtonsoft.Json;
using RestSharp;
using RestSharp.Authenticators;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web.Mvc;
using System.Web.Routing;
using Vaelastrasz.Library.Models;
using Vaiona.Web.Mvc;
using Vaiona.Web.Mvc.Modularity;

namespace BExIS.Modules.Dim.UI.Controllers
{
    public class DataCiteDoiController : BaseController
    {
        public ActionResult index()
        {
            List<PublicationModel> model = new List<PublicationModel>();

            using (DatasetManager datasetManager = new DatasetManager())
            using (PublicationManager publicationManager = new PublicationManager())
            {

                Broker broker = publicationManager.RepositoryRepo.Get().Where(b => b.Name.ToLower() == "datacite").FirstOrDefault().Broker;
                List<Publication> publications = publicationManager.GetPublication().Where(p => p.Broker.Name.ToLower().Equals(broker.Name.ToLower())).ToList();

                foreach (Publication p in publications)
                {
                    model.Add(new PublicationModel()
                    {
                        Broker = new BrokerModel(broker.Name, new List<string>() { p.Repository.Name }, broker.Link),
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

        public ActionResult _grantDoi(long datasetVersionId)
        {
            using (DatasetManager datasetManager = new DatasetManager())
            using (PublicationManager publicationManager = new PublicationManager())
            using (EntityPermissionManager entityPermissionManager = new EntityPermissionManager())
            using (EntityManager entityManager = new EntityManager())
            {
                // dataset - version
                DatasetVersion datasetVersion = datasetManager.GetDatasetVersion(datasetVersionId);

                long versionNo = datasetManager.GetDatasetVersions(datasetVersion.Dataset.Id).OrderBy(d => d.Timestamp).Count();

                Publication publication = publicationManager.GetPublication().Where(p => p.DatasetVersion.Id.Equals(datasetVersion.Id)).FirstOrDefault();

                DatasetVersion latestDatasetVersion = datasetManager.GetDatasetLatestVersion(datasetVersion.Dataset.Id);

                string datasetUrl = new Uri(new Uri(Request.Url.GetLeftPart(UriPartial.Authority)), Url.Content("~/ddm/Data/ShowData/" + datasetVersion.Dataset.Id).ToString()).ToString();


                SettingsHelper settingsHelper = new SettingsHelper();
                if (settingsHelper.KeyExist("proxy") && settingsHelper.KeyExist("token"))
                {
                    // helper
                    var datacitedoihelper = new DataCiteDoiHelper();

                    var client = new RestClient(settingsHelper.GetValue("proxy"));
                    client.Authenticator = new JwtAuthenticator(settingsHelper.GetValue("token"));

                    // doi
                    var placeholders = datacitedoihelper.CreatePlaceholders(datasetVersion, settingsHelper.GetDataCiteSettings("placeholders"));

                    var doi_request = new RestRequest($"api/dois", Method.POST).AddJsonBody(placeholders);
                    //CreateDOIModel doi = JsonConvert.DeserializeObject<CreateDOIModel>(client.Execute(doi_request).Content);

                    // Model
                    var mappings = settingsHelper.GetDataCiteSettings("mappings");

                    var model = datacitedoihelper.CreateDataCiteModel(datasetVersion, mappings);
                    model.Data.Attributes.Doi = "";

                    var datacite_request = new RestRequest($"api/datacite", Method.POST).AddJsonBody(JsonConvert.SerializeObject(model));
                    var response = client.Execute(datacite_request);

                    if (response.StatusCode != System.Net.HttpStatusCode.Created)
                    {
                        return PartialView("_requestRow", new PublicationModel()
                        {
                            Broker = new BrokerModel(publication.Broker.Name, new List<string>() { publication.Repository.Name }, publication.Broker.Link),
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

                    //var response_content = JsonConvert.DeserializeObject<ReadDataCiteModel>(response.Content);
                    var response_content = JsonConvert.DeserializeObject(response.Content);


                    publication.DatasetVersion = latestDatasetVersion;
                    //  publication.Doi = doi;
                    publication.ExternalLink = "response_content.Id";
                    publication.Status = "DOI Registered";

                    publication = publicationManager.UpdatePublication(publication);

                    EmailService es = new EmailService();
                    List<string> tmp = null;
                    string title = new XmlDatasetHelper().GetInformationFromVersion(latestDatasetVersion.Id, NameAttributeValues.title);
                    string subject = "DOI Request for Dataset " + title + "(" + latestDatasetVersion.Dataset.Id + ")";
                    string body = "<p>DOI reqested for dataset <a href=\"" + datasetUrl + "\">" + title + "(" + latestDatasetVersion.Dataset.Id + ")</a>, was granted by the Datamanager.</p>" +
                        "<p>The doi is<a href=\"https://doi.org/" + "doi.DOI" + "\">" + "doi.DOI" + "</a></p>";

                    tmp = new List<string>();
                    List<string> emails = new List<string>();
                    tmp = MappingUtils.GetValuesFromMetadata((int)Key.Email, LinkElementType.Key, latestDatasetVersion.Dataset.MetadataStructure.Id, XmlUtility.ToXDocument(latestDatasetVersion.Metadata));

                    foreach (string s in tmp)
                    {
                        var email = s.Trim();
                        if (!string.IsNullOrEmpty(email) && !emails.Contains(email))
                        {
                            emails.Add(email);
                        }

                    }

                    es.Send(subject, body, emails);
                    es.Send(subject, body, ConfigurationManager.AppSettings["SystemEmail"]);

                    long entityId = entityManager.Entities.Where(e => e.Name.ToLower().Equals("dataset")).FirstOrDefault().Id;

                    EntityPermission entityPermission = entityPermissionManager.Find(null, entityId, datasetVersion.Dataset.Id);

                    if (entityPermission == null)
                    {
                        entityPermissionManager.Create(null, entityId, datasetVersion.Dataset.Id, (int)RightType.Read);
                    }
                    else
                    {
                        entityPermission.Rights = (int)RightType.Read;
                        entityPermissionManager.Update(entityPermission);
                    }

                    if (this.IsAccessible("DDM", "SearchIndex", "ReIndexSingle"))
                    {
                        var x = this.Run("DDM", "SearchIndex", "ReIndexSingle", new RouteValueDictionary() { { "id", datasetVersion.Dataset.Id } });
                    }
                }

                return PartialView("_requestRow", new PublicationModel()
                {
                    Broker = new BrokerModel(publication.Broker.Name, new List<string>() { publication.Repository.Name }, publication.Broker.Link),
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

        public ActionResult _denyDoi(long datasetVersionId)
        {
            using (DatasetManager datasetManager = new DatasetManager())
            using (PublicationManager publicationManager = new PublicationManager())
            {
                DatasetVersion datasetVersion = datasetManager.GetDatasetVersion(datasetVersionId);

                Publication publication = publicationManager.GetPublication().Where(p => p.DatasetVersion.Id.Equals(datasetVersion.Id)).FirstOrDefault();

                publication.Status = "DOI Denied";

                publication = publicationManager.UpdatePublication(publication);

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

                return PartialView("_requestRow", new PublicationModel()
                {
                    Broker = new BrokerModel(publication.Broker.Name, new List<string>() { publication.Repository.Name }, publication.Broker.Link),
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

        public ActionResult Create(long datasetVersionId)
        {
            using (var datasetManager = new DatasetManager())
            using (var conceptManager = new ConceptManager())
            {
                var datasetVersion = datasetManager.GetDatasetVersion(datasetVersionId);
                //var concept = conceptManager.MappingConceptRepo.Query(c => c.Name == "DataCiteDoi").FirstOrDefault();

                //if(concept == null)
                //    return PartialView("_Create", new CreateDataCiteDoiModel(datasetVersion.Dataset.Id, datasetVersion.Id));

                //var output = MappingUtils.GetConceptOutput(datasetVersion.Dataset.MetadataStructure.Id, concept.Id, datasetVersion.Metadata);

                CreateDataCiteModel response = new CreateDataCiteModel();
                //XmlSerializer serializer = new XmlSerializer(typeof(CreateDataCiteModel));
                //using (XmlReader reader = new XmlNodeReader(output))
                //{
                //    response = (CreateDataCiteModel)serializer.Deserialize(reader);
                //}

                var model = new CreateDataCiteDoiModel(datasetVersion.Dataset.Id, datasetVersion.Id, response);

                return View("_Create", model);
            }
        }

        [HttpPost]
        public ActionResult Create(CreateDataCiteDoiModel model)
        {
            SettingsHelper settingsHelper = new SettingsHelper();


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

        [HttpPost]
        public ActionResult Delete(string s)
        {
            return View();
        }

        [HttpPost]
        public ActionResult Accept(string s)
        {
            return View();
        }

        [HttpPost]
        public ActionResult Reject(string s)
        {
            return View();
        }
    }
}