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
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web.Mvc;
using System.Web.Routing;
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

                Broker broker = publicationManager.RepositoryRepo.Get().Where(b => b.Name.ToLower().Equals(ConfigurationManager.AppSettings["doiProvider"].ToLower())).FirstOrDefault().Broker;
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

                DatasetVersion datasetVersion = datasetManager.GetDatasetVersion(datasetVersionId);

                long versionNo = datasetManager.GetDatasetVersions(datasetVersion.Dataset.Id).OrderBy(d => d.Timestamp).Count();

                Publication publication = publicationManager.GetPublication().Where(p => p.DatasetVersion.Id.Equals(datasetVersion.Id)).FirstOrDefault();

                DatasetVersion latestDatasetVersion = datasetManager.GetDatasetLatestVersion(datasetVersion.Dataset.Id);

                string datasetUrl = new Uri(new Uri(Request.Url.GetLeftPart(UriPartial.Authority)), Url.Content("~/ddm/Data/ShowData/" + datasetVersion.Dataset.Id).ToString()).ToString();


                SettingsHelper settingsHelper = new SettingsHelper();
                if (settingsHelper.KeyExist("doi_proxy") && settingsHelper.KeyExist("doi_token"))
                {
                    string doi = new DataCiteDoiHelper(settingsHelper.GetValue("doi_proxy"), settingsHelper.GetValue("doi_token")).issueDoi(latestDatasetVersion, datasetUrl, versionNo);

                    publication.DatasetVersion = latestDatasetVersion;
                    //  publication.Doi = doi;
                    publication.ExternalLink = doi;
                    publication.Status = "DOI Registered";

                    publication = publicationManager.UpdatePublication(publication);

                    EmailService es = new EmailService();
                    List<string> tmp = null;
                    string title = new XmlDatasetHelper().GetInformationFromVersion(latestDatasetVersion.Id, NameAttributeValues.title);
                    string subject = "DOI Request for Dataset " + title + "(" + latestDatasetVersion.Dataset.Id + ")";
                    string body = "<p>DOI reqested for dataset <a href=\"" + datasetUrl + "\">" + title + "(" + latestDatasetVersion.Dataset.Id + ")</a>, was granted by the Datamanager.</p>" +
                        "<p>The doi is<a href=\"https://doi.org/" + doi + "\">" + doi + "</a></p>";

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
    }
}
