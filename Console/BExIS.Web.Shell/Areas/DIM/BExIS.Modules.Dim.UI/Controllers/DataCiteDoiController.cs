using BExIS.Dim.Entities.Mappings;
using BExIS.Dim.Entities.Publications;
using BExIS.Dim.Helpers.Mappings;
using BExIS.Dim.Services.Mappings;
using BExIS.Dim.Services.Publications;
using BExIS.Dlm.Entities.Data;
using BExIS.Dlm.Services.Data;
using BExIS.Modules.Dim.UI.Helpers;
using BExIS.Modules.Dim.UI.Models;
using BExIS.Security.Services.Authorization;
using BExIS.Security.Services.Objects;
using BExIS.Security.Services.Utilities;
using BExIS.Xml.Helpers;
using BEXIS.JSON.Helpers.Extensions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Web.UI.WebControls;
using Vaelastrasz.Library.Entities;
using Vaelastrasz.Library.Extensions;
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

                if (updated)
                {
                    List<string> tmp = null;
                    string title = new XmlDatasetHelper().GetInformationFromVersion(datasetVersion.Id, NameAttributeValues.title);
                    string subject = "DOI Request for Dataset " + title + "(" + datasetVersion.Dataset.Id + ")";
                    string body = "<p>DOI reqested for dataset " + title + "(" + datasetVersion.Dataset.Id + "), was denied by the Datamanager.</p>";

                    tmp = new List<string>();
                    tmp = MappingUtils.GetValuesFromMetadata((int)Key.Email, LinkElementType.Key, datasetVersion.Dataset.MetadataStructure.Id, XmlUtility.ToXDocument(datasetVersion.Metadata));

                    using(var emailService = new EmailService())
                    {
                        foreach (string s in tmp)
                        {
                            string e = s.Trim();
                            emailService.Send(subject, body, e);
                        }
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
                    DatasetVersionNr = publication.DatasetVersion.VersionNo
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
            catch (Exception ex)
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

                return PartialView("_requestRow", new PublicationModel()
                {
                    Id = publication.Id,
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
                    var datasetVersionNr = datasetManager.GetDatasetVersionNr(publication.DatasetVersion.Id);

                    if (publication == null)
                        throw new ArgumentException("Publication does not exist", nameof(publicationId));

                    if (publication.Status == "accepted")
                        return PartialView("_requestRow", new PublicationModel()
                        {
                            Id = publication.Id,
                            Broker = new BrokerModel(publication.Broker.Id, publication.Broker.Name, new List<string>() { publication.Repository.Name }, publication.Broker.Link),
                            DataRepo = publication.Repository.Name,
                            DatasetVersionId = publication.DatasetVersion.Id,
                            CreationDate = publication.Timestamp,
                            ExternalLink = publication.ExternalLink,
                            FilePath = publication.FilePath,
                            Status = publication.Status,
                            DatasetId = publication.DatasetVersion.Dataset.Id,
                            DatasetVersionNr = datasetVersionNr,
                            Response = publication.Response
                        });

                    // Preparation
                    var settingsHelper = new SettingsHelper();
                    var placeholders = settingsHelper.GetDataCitePlaceholders();
                    var mappings = settingsHelper.GetDataCiteMappings();

                    // Placeholders for DOI
                    for (int i = 0; i < placeholders.Count; i++)
                    {
                        var placeholder = placeholders.ElementAt(i);


                        switch (placeholder.Value)
                        {

                            case "{DatasetId}":
                                placeholders[placeholder.Key] = publication.DatasetVersion.Dataset.Id.ToString();
                                break;

                            case "{VersionId}":
                                placeholders[placeholder.Key] = publication.DatasetVersion.Id.ToString();
                                break;

                            case "{VersionNumber}":
                                placeholders[placeholder.Key] = datasetVersionNr.ToString();
                                break;

                            case "{VersionName}":
                                placeholders[placeholder.Key] = publication.DatasetVersion.VersionName?.ToString();
                                break;

                            case "{Tag}":
                                placeholders[placeholder.Key] = publication.DatasetVersion.Tag?.ToString();
                                break;

                            default:
                                break;
                        }
                    }

                    var model = new CreateDataCiteModel();

                    var configuration = new Vaelastrasz.Library.Configurations.Configuration(publication.Broker.UserName, publication.Broker.Password, publication.Broker.Host, true);
                    var doiService = new DOIService(configuration);
                    var createSuffixModel = new CreateSuffixModel()
                    {
                        Placeholders = placeholders
                    };
                    var doi = await doiService.GenerateAsync(createSuffixModel);

                    if (!doi.IsSuccessful)
                        return PartialView("_requestRow", new PublicationModel()
                        {
                            Id = publication.Id,
                            Broker = new BrokerModel(publication.Broker.Id, publication.Broker.Name, new List<string>() { publication.Repository.Name }, publication.Broker.Link),
                            DataRepo = publication.Repository.Name,
                            DatasetVersionId = publication.DatasetVersion.Id,
                            CreationDate = publication.Timestamp,
                            ExternalLink = publication.ExternalLink,
                            FilePath = publication.FilePath,
                            Status = publication.Status,
                            DatasetId = publication.DatasetVersion.Dataset.Id,
                            DatasetVersionNr = datasetVersionNr,
                            Response = $"status: {doi.Status}. error message: {doi.ErrorMessage}"
                        });

                    var json_model = JsonConvert.SerializeObject(model);

                    var concept = conceptManager.MappingConceptRepository.Query(c => c.Name.ToLower() == "datacite").FirstOrDefault();
                    //var metadataStructureId = publication.DatasetVersion.Dataset.MetadataStructure.Id;

                    if (concept == null)
                        return View("Create", model);

                    var xml = MappingUtils.GetConceptOutput(publication.DatasetVersion.Dataset.MetadataStructure.Id, concept.Id, publication.DatasetVersion.Metadata);
                    var json = JsonConvert.SerializeObject(xml);

                    var jObject = JObject.Parse(json);

                    JsonExtensions.TransformToMatchClassTypes(jObject, typeof(CreateDataCiteModel));

                    json = JsonConvert.SerializeObject(jObject, Formatting.Indented);

                    try
                    {
                        model = JsonConvert.DeserializeObject<CreateDataCiteModel>(json);
                    }
                    catch (Exception ex)
                    {
                        return PartialView("_requestRow", new PublicationModel()
                        {
                            Id = publication.Id,
                            Broker = new BrokerModel(publication.Broker.Id, publication.Broker.Name, new List<string>() { publication.Repository.Name }, publication.Broker.Link),
                            DataRepo = publication.Repository.Name,
                            DatasetVersionId = publication.DatasetVersion.Id,
                            CreationDate = publication.Timestamp,
                            ExternalLink = publication.ExternalLink,
                            FilePath = publication.FilePath,
                            Status = publication.Status,
                            DatasetId = publication.DatasetVersion.Dataset.Id,
                            DatasetVersionNr = datasetVersionNr,
                            Response = $"error message: {ex.Message}"
                        });
                    }

                    model.SetDoi(doi.Data);

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
                    if (dataCiteResponse.IsSuccessful)
                    {
                        publication.Status = "accepted";
                        publication.Response = JsonConvert.SerializeObject(dataCiteResponse.Data);
                        publication.ExternalLink = dataCiteResponse.Data.Data.Attributes.Doi;
                        publication.ExternalLinkType = "DOI";

                        publicationManager.Update(publication);

                        // kritisch, dass hier der Manager weiter geleitet wird?!
                        setDoiInMetadataIfExist(publication.DatasetVersion, dataCiteResponse.Data.Data.Attributes.Doi, datasetManager);
                    }
                    else
                    {
                        publication.Status = "pending";
                        publication.Response = $"status: {dataCiteResponse.Status}. error message: {dataCiteResponse.ErrorMessage}";

                        publicationManager.Update(publication);
                    }

                    // E-Mail
                    string datasetUrl = new Uri(new Uri(Request.Url.GetLeftPart(UriPartial.Authority)), Url.Content("~/ddm/Data/ShowData/" + publication.DatasetVersion.Dataset.Id).ToString()).ToString();
                    List<string> tmp = null;
                    string title = new XmlDatasetHelper().GetInformationFromVersion(publication.DatasetVersion.Id, NameAttributeValues.title);
                    string subject = "DOI Request for Dataset " + title + "(" + publication.DatasetVersion.Dataset.Id + ")";
                    string body = "<p>DOI reqested for dataset <a href=\"" + datasetUrl + "\">" + title + "(" + publication.DatasetVersion.Dataset.Id + ")</a>, was granted by the Datamanager.</p>" +
                        "<p>The doi is<a href=\"https://doi.org/" + "doi.DOI" + "\">" + "doi.DOI" + "</a></p>";

                    tmp = new List<string>();
                    List<string> emails = new List<string>();
                    tmp = MappingUtils.GetValuesFromMetadata((int)Key.Email, LinkElementType.Key, publication.DatasetVersion.Dataset.MetadataStructure.Id, XmlUtility.ToXDocument(publication.DatasetVersion.Metadata));

                    foreach (string s in tmp)
                    {
                        var email = s.Trim();
                        if (!string.IsNullOrEmpty(email) && !emails.Contains(email))
                        {
                            emails.Add(email);
                        }

                    }

                    using(var emailService = new EmailService())
                    {
                        emailService.Send(subject, body, emails);
                        emailService.Send(subject, body, ConfigurationManager.AppSettings["SystemEmail"]);
                    }

                    return PartialView("_requestRow", new PublicationModel()
                    {
                        Id = publication.Id,
                        Broker = new BrokerModel(publication.Broker.Id, publication.Broker.Name, new List<string>() { publication.Repository.Name }, publication.Broker.Link),
                        DataRepo = publication.Repository.Name,
                        DatasetVersionId = publication.DatasetVersion.Id,
                        CreationDate = publication.Timestamp,
                        ExternalLink = publication.ExternalLink,
                        FilePath = publication.FilePath,
                        Status = publication.Status,
                        DatasetId = publication.DatasetVersion.Dataset.Id,
                        DatasetVersionNr = datasetVersionNr,
                        Response = publication.Response
                    });
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public ActionResult Reject(long publicationId)
        {
            try
            {
                using (var datasetManager = new DatasetManager())
                using (var publicationManager = new PublicationManager())
                using (var conceptManager = new ConceptManager())
                {
                    var publication = publicationManager.FindById(publicationId);
                    var datasetVersionNr = datasetManager.GetDatasetVersionNr(publication.DatasetVersion.Id);

                    if (publication == null)
                        throw new ArgumentException("Publication does not exist", nameof(publicationId));

                    publication.Status = "rejected";
                    publicationManager.Update(publication);

                    List<string> tmp = null;
                    string title = new XmlDatasetHelper().GetInformationFromVersion(publication.DatasetVersion.Id, NameAttributeValues.title);
                    string subject = "DOI Request for Dataset " + title + "(" + publication.DatasetVersion.Dataset.Id + ")";
                    string body = "<p>DOI reqested for dataset " + title + "(" + publication.DatasetVersion.Dataset.Id + "), was denied by the Datamanager.</p>";

                    tmp = new List<string>();
                    tmp = MappingUtils.GetValuesFromMetadata((int)Key.Email, LinkElementType.Key, publication.DatasetVersion.Dataset.MetadataStructure.Id, XmlUtility.ToXDocument(publication.DatasetVersion.Metadata));

                    using (var emailService = new EmailService())
                    {
                        foreach (string s in tmp)
                        {
                            string e = s.Trim();
                            emailService.Send(subject, body, e);
                        }
                    } 

                    return PartialView("_requestRow", new PublicationModel()
                    {
                        Id = publication.Id,
                        Broker = new BrokerModel(publication.Broker.Id, publication.Broker.Name, new List<string>() { publication.Repository.Name }, publication.Broker.Link),
                        DataRepo = publication.Repository.Name,
                        DatasetVersionId = publication.DatasetVersion.Id,
                        CreationDate = publication.Timestamp,
                        ExternalLink = publication.ExternalLink,
                        FilePath = publication.FilePath,
                        Status = publication.Status,
                        DatasetId = publication.DatasetVersion.Dataset.Id,
                        DatasetVersionNr = datasetVersionNr
                    });
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public ActionResult Create(long datasetVersionId)
        {
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

        private bool setDoiInMetadataIfExist(DatasetVersion version, string doi, DatasetManager datasetManager)
        {
            var sourceId = (int)Key.DOI;
            var sourceType = LinkElementType.Key;
            var metadataStructureId = version.Dataset.MetadataStructure.Id;

            LinkElement target = null;
            MappingUtils.HasTarget(sourceId, metadataStructureId, out target);

            if (target != null)
            {
                datasetManager.UpdateSingleValueInMetadata(version.Id, target.XPath, doi);

                return true;
            }

            return false;
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
                List<long> brokerIds = brokerManager.FindByName("datacite").Select(b => b.Id).ToList();
                List<Publication> publications = publicationManager.Publications.Where(p => brokerIds.Contains(p.Broker.Id)).ToList();

                foreach (Publication p in publications)
                {
                    model.Add(new PublicationModel()
                    {
                        Id = p.Id,
                        Broker = new BrokerModel(p.Broker.Id, p.Broker.Name, new List<string>() { p.Repository.Name }, p.Broker.Link),
                        DataRepo = p.Repository.Name,
                        DatasetVersionId = p.DatasetVersion.Id,
                        CreationDate = p.Timestamp,
                        ExternalLink = p.ExternalLink,
                        FilePath = p.FilePath,
                        Status = p.Status,
                        DatasetId = p.DatasetVersion.Dataset.Id,
                        DatasetVersionNr = datasetManager.GetDatasetVersionNr(p.DatasetVersion.Id),
                        Response = p.Response
                    });
                }
            }

            return View(model);
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