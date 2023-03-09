using BExIS.App.Bootstrap.Attributes;
using BExIS.Dim.Entities.Publication;
using BExIS.Dim.Helpers;
using BExIS.Dim.Helpers.Export;
using BExIS.Dim.Helpers.GFBIO;
using BExIS.Dim.Services;
using BExIS.Dlm.Entities.Data;
using BExIS.Dlm.Services.Data;
using BExIS.IO;
using BExIS.IO.Transform.Output;
using BExIS.Modules.Dim.UI.Models;
using BExIS.Security.Entities.Authorization;
using BExIS.Security.Services.Authorization;
using BExIS.Xml.Helpers;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using Vaiona.Persistence.Api;
using Vaiona.Utils.Cfg;
using Vaiona.Web.Mvc;

namespace BExIS.Modules.Dim.UI.Controllers
{
    public class SubmissionController : BaseController
    {
        private XmlDatasetHelper xmlDatasetHelper = new XmlDatasetHelper();

        // GET: Submission

        #region submission

        /// <summary>
        /// ToDo Refactor submission Commented by Javad due to modularity issues.
        /// Thes functions should call the APIs of the DIM module and get json objects back.
        /// If Publication or any other entity is not part of the DLM, it is visible only to its own module.
        /// Other mosules who consume the API results of a module, should only expect .NET types, DLM types, json, xml, CSV, or Html.
        /// </summary>

        [BExISEntityAuthorize(typeof(Dataset), "datasetId", RightType.Write)]
        public ActionResult publishData(long datasetId, long datasetVersionId = -1)
        {
            ShowPublishDataModel model = getShowPublishDataModel(datasetId, datasetVersionId);

            return View("_showPublishDataView", model);
        }

        [BExISEntityAuthorize(typeof(Dataset), "datasetId", RightType.Read)]
        public ActionResult getPublishDataPartialView(long datasetId, long datasetVersionId = -1)
        {
            ShowPublishDataModel model = getShowPublishDataModel(datasetId, datasetVersionId);

            return PartialView("_showPublishDataView", model);
        }

        private ShowPublishDataModel getShowPublishDataModel(long datasetId, long datasetVersionId = -1)
        {
            PublicationManager publicationManager = new PublicationManager();
            DatasetManager datasetManager = new DatasetManager();
            EntityPermissionManager entityPermissionManager = new EntityPermissionManager();
            ShowPublishDataModel model = new ShowPublishDataModel();

            int versionNr = 1;

            try
            {
                Dataset dataset = datasetManager.GetDataset(datasetId);

                List<Broker> Brokers = GetBrokers(dataset.MetadataStructure.Id, publicationManager);

                model.Brokers = Brokers.Select(b => b.Name).ToList();
                model.DatasetId = datasetId;

                //Todo Download Rigths -> currently set read rigths for this case
                model.DownloadRights = entityPermissionManager.HasEffectiveRight(HttpContext.User.Identity.Name,
                    typeof(Dataset), datasetId, RightType.Read);
                model.EditRights = entityPermissionManager.HasEffectiveRight(HttpContext.User.Identity.Name,
                    typeof(Dataset), datasetId, RightType.Write);

                List<long> versions = new List<long>();
                if (datasetVersionId == -1)
                {
                    DatasetVersion datasetVersion = datasetManager.GetDatasetLatestVersion(datasetId);
                    datasetVersionId = datasetVersion.Id;
                    versions = datasetManager.GetDatasetVersions(datasetId).Select(d => d.Id).ToList();

                    if (datasetVersion.StateInfo != null)
                        model.MetadataIsValid = DatasetStateInfo.Valid.ToString().Equals(datasetVersion.StateInfo.State) ? true : false;
                }

                //todo check if datasetversion id is correct
                List<Publication> publications =
                    publicationManager.PublicationRepo.Query().Where(p => versions.Contains(p.DatasetVersion.Id)).ToList();

                //get versionNr
                versionNr = datasetManager.GetDatasetVersionNr(datasetVersionId);

                foreach (var pub in publications)
                {
                    Broker broker = publicationManager.BrokerRepo.Get(pub.Broker.Id);
                    Repository repo = null;

                    if (pub.Repository != null)
                    {
                        repo = publicationManager.RepositoryRepo.Get(pub.Repository.Id);
                    }
                    string dataRepoName = repo == null ? "" : repo.Name;

                    List<string> repos =
                        GetRepos(dataset.MetadataStructure.Id, broker.Id, publicationManager).Select(r => r.Name).ToList();

                    model.Publications.Add(new PublicationModel()
                    {
                        Broker = new BrokerModel(broker.Name, repos, broker.Link),
                        DataRepo = dataRepoName,
                        DatasetVersionId = pub.DatasetVersion.Id,
                        CreationDate = pub.Timestamp,
                        ExternalLink = pub.ExternalLink,
                        FilePath = pub.FilePath,
                        Status = pub.Status,
                        DatasetVersionNr = versionNr
                    });
                }

                return model;
            }
            finally
            {
                publicationManager.Dispose();
                datasetManager.Dispose();
                entityPermissionManager.Dispose();
            }
        }

        public ActionResult LoadDataRepoRequirementView(string datarepo, long datasetid)
        {
            DataRepoRequirentModel model = new DataRepoRequirentModel();
            model.DatasetId = datasetid;

            //get broker
            PublicationManager publicationManager = new PublicationManager();
            // datasetversion
            DatasetManager dm = new DatasetManager();

            try
            {
                long version = dm.GetDatasetLatestVersionId(datasetid);
                model.DatasetVersionId = version;
                if (publicationManager.BrokerRepo.Get().Any(d => d.Name.ToLower().Equals(datarepo.ToLower())))
                {
                    Broker broker =
                        publicationManager.BrokerRepo.Get()
                            .Where(d => d.Name.ToLower().Equals(datarepo.ToLower()))
                            .FirstOrDefault();

                    Publication publication =
                        publicationManager.PublicationRepo.Get()
                            .Where(p => p.Broker.Id.Equals(broker.Id) && p.DatasetVersion.Id.Equals(version))
                            .FirstOrDefault();

                    if (publication != null && !String.IsNullOrEmpty(publication.FilePath)
                        && FileHelper.FileExist(Path.Combine(AppConfiguration.DataPath, publication.FilePath)))
                    {
                        model.Exist = true;
                    }
                    else
                    {
                        //if convertion check ist needed
                        //get all export attr from metadata structure
                        List<string> exportNames =
                            xmlDatasetHelper.GetAllTransmissionInformation(datasetid,
                                TransmissionType.mappingFileExport, AttributeNames.name).ToList();
                        if (string.IsNullOrEmpty(broker.MetadataFormat) || exportNames.Contains(broker.MetadataFormat)) model.IsMetadataConvertable = true;

                        // Validate
                        model.metadataValidMessage = OutputMetadataManager.IsValideAgainstSchema(datasetid,
                            TransmissionType.mappingFileExport, datarepo);

                        #region primary Data

                        if (broker.PrimaryDataFormat.ToLower().Contains("text/plain") ||
                            broker.PrimaryDataFormat.ToLower().Contains("text/csv") ||
                            broker.PrimaryDataFormat.ToLower().Contains("application/excel") ||
                            String.IsNullOrEmpty(broker.PrimaryDataFormat))
                        {
                            model.IsDataConvertable = true;
                        }

                        #endregion primary Data
                    }
                }

                return PartialView("_dataRepositoryRequirementsView", model);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                publicationManager.Dispose();
                dm.Dispose();
            }
        }

        public JsonResult CheckExportPossibility(string datarepo, long datasetid)
        {
            bool isDataConvertable = false;
            bool isMetadataConvertable = false;
            string metadataValidMessage = "";
            bool exist = false;

            using (PublicationManager publicationManager = new PublicationManager())
            using (DatasetManager dm = new DatasetManager())
            {
                // datasetversion
                long version = dm.GetDatasetLatestVersion(datasetid).Id;

                if (publicationManager.BrokerRepo.Get().Any(d => d.Name.ToLower().Equals(datarepo.ToLower())))
                {
                    Broker broker =
                       publicationManager.BrokerRepo.Get().Where(d => d.Name.ToLower().Equals(datarepo.ToLower())).FirstOrDefault();

                    Publication publication =
                        publicationManager.PublicationRepo.Get()
                            .Where(p => p.Broker != null && p.Broker.Id.Equals(broker.Id) && p.DatasetVersion != null && p.DatasetVersion.Id.Equals(version))
                            .FirstOrDefault();

                    if (publication != null && !String.IsNullOrEmpty(publication.FilePath)
                        && FileHelper.FileExist(Path.Combine(AppConfiguration.DataPath, publication.FilePath)))
                    {
                        //model.Exist = true;
                        exist = true;
                    }
                    else
                    {
                        #region metadata

                        // if no conversion is needed
                        if (String.IsNullOrEmpty(broker.MetadataFormat))
                        {
                            //model.IsMetadataConvertable = true;
                            isMetadataConvertable = true;

                            // Validate
                            metadataValidMessage = OutputMetadataManager.IsValideAgainstSchema(datasetid,
                                TransmissionType.mappingFileExport, datarepo);
                        }
                        else
                        {
                            //if convertion check ist needed
                            //get all export attr from metadata structure
                            List<string> exportNames =
                                xmlDatasetHelper.GetAllTransmissionInformation(datasetid,
                                    TransmissionType.mappingFileExport, AttributeNames.name).ToList();
                            if (exportNames.Contains(broker.MetadataFormat))
                                isMetadataConvertable = true;

                            metadataValidMessage = OutputMetadataManager.IsValideAgainstSchema(datasetid,
                                TransmissionType.mappingFileExport, datarepo);
                        }

                        #endregion metadata

                        #region primary Data

                        //todo need a check if the primary data is structured or not, if its unstructured also export should be possible

                        if (broker.PrimaryDataFormat.ToLower().Contains("text/plain") ||
                            broker.PrimaryDataFormat.ToLower().Contains("text/csv") ||
                            broker.PrimaryDataFormat.ToLower().Contains("application/excel") ||
                            String.IsNullOrEmpty(broker.PrimaryDataFormat))
                        {
                            isDataConvertable = true;
                        }

                        #endregion primary Data
                    }

                    //check if reporequirements are fit
                    //e.g. GFBIO
                }
            }

            return Json(new { isMetadataConvertable = isMetadataConvertable, isDataConvertable = isDataConvertable, metadataValidMessage = metadataValidMessage, Exist = exist });
        }

        public ActionResult DownloadZip(string broker, string datarepo, long datasetversionid)
        {
            DatasetVersion datasetVersion = this.GetUnitOfWork().GetReadOnlyRepository<DatasetVersion>().Get(datasetversionid);
            long datasetId = datasetVersion.Dataset.Id;

            Tuple<string, string> tmp = PrepareData(datasetversionid, datasetId, datarepo, broker);

            string filepath = tmp.Item1;
            string mimetype = tmp.Item2;

            return File(filepath, mimetype, Path.GetFileName(filepath));
        }

        /// <summary>
        /// prepare data for the broker and repo
        /// and return a tuple
        /// tuple.item1 = filepath
        /// tuple.item2 = mimetype
        /// </summary>
        /// <param name="datasetVersionId"></param>
        /// <param name="datasetId"></param>
        /// <param name="datarepo"></param>
        /// <param name="broker"></param>
        /// <returns></returns>
        private Tuple<string, string> PrepareData(long datasetVersionId, long datasetId, string datarepo, string broker)
        {
            Tuple<string, string> tmp;
            try
            {
                using (PublicationManager publicPublicationManager = new PublicationManager())
                {
                    Repository repository =
                        publicPublicationManager.RepositoryRepo
                            .Query().FirstOrDefault(p => p.Name.ToLower().Equals(datarepo.ToLower()) &&
                                        p.Broker.Name.ToLower().Equals(broker.ToLower()));

                    switch (datarepo.ToLower())
                    {
                        case "pangaea":
                            {
                                PangaeaDataRepoConverter dataRepoConverter = new PangaeaDataRepoConverter(repository);

                                tmp = new Tuple<string, string>(dataRepoConverter.Convert(datasetVersionId), "text/txt");
                                return tmp;
                            }
                        case "collections":
                            {
                                GenericDataRepoConverter dataRepoConverter = new GenericDataRepoConverter(repository);
                                tmp = new Tuple<string, string>(dataRepoConverter.Convert(datasetVersionId), "application/zip");
                                return tmp;
                            }
                        case "pensoft":
                            {
                                PensoftDataRepoConverter dataRepoConverter = new PensoftDataRepoConverter(repository);
                                tmp = new Tuple<string, string>(dataRepoConverter.Convert(datasetVersionId), "text/xml");
                                return tmp;
                            }
                        case "gbif":
                            {
                                GBIFDataRepoConverter dataRepoConverter = new GBIFDataRepoConverter(repository);
                                tmp = new Tuple<string, string>(dataRepoConverter.Convert(datasetVersionId), "application/zip");
                                return tmp;
                            }
                        default:
                            {
                                //default
                                GenericDataRepoConverter dataRepoConverter = new GenericDataRepoConverter(repository);
                                tmp = new Tuple<string, string>(dataRepoConverter.Convert(datasetVersionId), "application/zip");
                                return tmp;
                            }
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return null;
        }

        [BExISEntityAuthorize(typeof(Dataset), "datasetId", RightType.Write)]
        public async Task<ActionResult> SendDataToDataRepo(long datasetId, string datarepo)
        {
            PublicationManager publicationManager = new PublicationManager();
            DatasetManager datasetManager = new DatasetManager();

            try
            {
                string zipfilepath = "";
                if (Session["ZipFilePath"] != null)
                    zipfilepath = Session["ZipFilePath"].ToString();

                DatasetVersion datasetVersion = datasetManager.GetDatasetLatestVersion(datasetId);

                Publication publication =
                    publicationManager.GetPublication()
                        .Where(
                            p =>
                                p.DatasetVersion.Id.Equals(datasetVersion.Id) &&
                                p.Broker.Name.ToLower().Equals(datarepo.ToLower()))
                        .FirstOrDefault();

                if (publication == null)
                {
                    //ToDo [SUBMISSION] -> create broker specfic function
                    // check case for gfbio
                    if (datarepo.ToLower().Contains("gfbio"))
                    {
                        #region GFBIO

                        //SubmissionManager publishingManager = new SubmissionManager();
                        //publishingManager.Load();
                        //DataRepository dataRepository = publishingManager.DataRepositories.Where(d => d.Name.Equals(datarepo)).FirstOrDefault();

                        Broker broker =
                            publicationManager.GetBroker()
                                .Where(b => b.Name.ToLower().Equals(datarepo.ToLower()))
                                .FirstOrDefault();

                        if (broker != null)
                        {
                            //Store ro in db
                            publicationManager.CreatePublication(datasetVersion, broker, datasetVersion.Title, 0, zipfilepath, "", "no status available");

                            //sendToGFBIO(broker, datasetId, datasetVersion, zipfilepath);
                        }

                        #endregion GFBIO
                    }

                    if (datarepo.ToLower().Contains("pensoft"))
                    {
                        #region pensoft

                        Broker broker =
                            publicationManager.BrokerRepo.Get()
                                .Where(b => b.Name.ToLower().Equals(datarepo.ToLower()))
                                .FirstOrDefault();

                        Repository repository =
                            publicationManager.RepositoryRepo.Get()
                                .Where(b => b.Name.ToLower().Equals(datarepo.ToLower()))
                                .FirstOrDefault();

                        publicationManager.CreatePublication(datasetVersion, broker, repository, datasetVersion.Title, 0, zipfilepath, "",
                            "no status available");

                        #endregion pensoft
                    }

                    if (datarepo.ToLower().Equals("generic"))
                    {
                        #region GENERIC

                        Broker broker =
                            publicationManager.BrokerRepo.Get()
                                .Where(b => b.Name.ToLower().Equals(datarepo.ToLower()))
                                .FirstOrDefault();
                        publicationManager.CreatePublication(datasetVersion, broker, datasetVersion.Title, 0, zipfilepath, "","created");

                        #endregion GENERIC
                    }

                    if (datarepo.ToLower().Equals("gbif"))
                    {
                        #region gbif

                        Broker broker =
                            publicationManager.BrokerRepo.Get()
                                .Where(b => b.Name.ToLower().Equals(datarepo.ToLower()))
                                .FirstOrDefault();
                        publicationManager.CreatePublication(datasetVersion, broker, datasetVersion.Title, 0, zipfilepath, "", "created");

                        #endregion GENERIC
                    }

                    if (datarepo.ToLower().Equals("doi"))
                    {
                        #region datacite

                        Broker broker =
                            publicationManager.BrokerRepo.Get()
                                .Where(b => b.Name.ToLower().Equals(datarepo.ToLower()))
                                .FirstOrDefault();

                        Repository repository =
                            publicationManager.RepositoryRepo.Get()
                                .Where(b => b.Broker.Name.ToLower().Equals(datarepo.ToLower()) &&
                                            b.Name.ToLower() == "datacite")
                                .FirstOrDefault();

                        if (repository != null && repository.Name.ToLower() == "datacite")
                        {
                            string datasetUrl = new Uri(new Uri(Request.Url.GetLeftPart(UriPartial.Authority)), Url.Content("~/ddm/Data/ShowData/" + datasetVersion.Dataset.Id).ToString()).ToString();
                            new DataCiteDoiHelper().sendRequest(datasetVersion, datasetUrl);

                            string title = xmlDatasetHelper.GetInformationFromVersion(datasetVersion.Id, NameAttributeValues.title);
                            publicationManager.CreatePublication(datasetVersion, broker, repository, title, 0, zipfilepath, datasetUrl, "under review");
                        }

                        #endregion
                    }

                    if (datarepo.ToLower().Equals("externallink"))
                    {
                        #region datacite

                        Broker broker =
                            publicationManager.BrokerRepo.Get()
                                .Where(b => b.Name.ToLower().Equals(datarepo.ToLower()))
                                .FirstOrDefault();

                        Repository repository =
                            publicationManager.RepositoryRepo.Get()
                                .Where(b => b.Broker.Name.ToLower().Equals(datarepo.ToLower()) &&
                                            b.Name.ToLower().Equals(ConfigurationManager.AppSettings["doiProvider"].ToLower()))
                                .FirstOrDefault();

                        if (repository != null && repository.Name.ToLower() == "datacite")
                        {
                            string datasetUrl = new Uri(new Uri(Request.Url.GetLeftPart(UriPartial.Authority)), Url.Content("~/ddm/Data/ShowData/" + datasetVersion.Dataset.Id).ToString()).ToString();
                            new DataCiteDoiHelper().sendRequest(datasetVersion, datasetUrl);

                            string title = xmlDatasetHelper.GetInformationFromVersion(datasetVersion.Id, NameAttributeValues.title);
                            publicationManager.CreatePublication(datasetVersion, broker, repository, title, 0, zipfilepath, datasetUrl, "under review");
                        }
                        #endregion datacite
                    }

                }
                else
                {
                    Json("Publication exist.");
                }
            }
            catch (Exception ex)
            {
                return Json(ex.Message);
            }
            finally
            {
                publicationManager.Dispose();
                datasetManager.Dispose();
            }

            return Json(true);
        }

        private List<Broker> GetBrokers(long metadataStrutcureId, PublicationManager publicationManager)
        {
            IEnumerable<Repository> repos = publicationManager.GetRepository();
            List<Broker> Brokers = new List<Broker>();

            foreach (var repo in repos)
            {
                // if repo is in table, means, that there is a restriction
                // only when dataset has a specific metada structure, the repo should be available in th ui
                if (publicationManager.MetadataStructureToRepositoryRepo.Get().Any(m => m.RepositoryId.Equals(repo.Id)))
                {
                    // exist in table
                    //check if metadataStructureId is existing
                    if (publicationManager.MetadataStructureToRepositoryRepo.Get().Any(m =>
                        m.RepositoryId.Equals(repo.Id) && m.MetadataStructureId.Equals(metadataStrutcureId)))
                    {
                        //add broker
                        Brokers.Add(repo.Broker);
                    }
                }
                else
                {
                    //add broker
                    Brokers.Add(repo.Broker);
                }
            }

            return Brokers.Distinct().ToList();
        }

        private List<Repository> GetRepos(long metadataStrutcureId, long brokerId, PublicationManager publicationManager)
        {
            IEnumerable<Repository> repos = publicationManager.GetRepository().Where(r => r.Broker.Id.Equals(brokerId));
            List<Repository> tmp = new List<Repository>();

            foreach (var repo in repos)
            {
                // if repo is in table, means, that there is a restriction
                // only when dataset has a specific metada structure, the repo should be available in th ui
                if (publicationManager.MetadataStructureToRepositoryRepo.Get().Any(m => m.RepositoryId.Equals(repo.Id)))
                {
                    // exist in table
                    // check if metadataStructureId is existing
                    if (publicationManager.MetadataStructureToRepositoryRepo.Get().Any(m =>
                        m.RepositoryId.Equals(repo.Id) && m.MetadataStructureId.Equals(metadataStrutcureId)))
                    {
                        //add repo
                        tmp.Add(repo);
                    }
                }
                else
                {
                    //add repo
                    tmp.Add(repo);
                }
            }

            return tmp.Distinct().ToList();
        }

        #region webservices calls STATUS

        /// <summary>
        /// Get Status from publiction
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<string> GetStatus(Publication publication)
        {
            if (publication.Broker != null)
            {
                //create a gfbio api webservice manager
                GFBIOWebserviceManager gfbioWebserviceManager = new GFBIOWebserviceManager(publication.Broker);

                string roStatusJsonResult = await gfbioWebserviceManager.GetStatusByResearchObjectById(publication.Id);

                //get status and store ro
                List<GFBIOResearchObjectStatus> gfbioRoStatusList =
                    new JavaScriptSerializer().Deserialize<List<GFBIOResearchObjectStatus>>(
                        roStatusJsonResult);
                GFBIOResearchObjectStatus gfbioRoStatus = gfbioRoStatusList.LastOrDefault();
                return gfbioRoStatus.status;
            }

            return "no status";
        }

        #endregion webservices calls STATUS

        #region GFBIO

        private async Task<ActionResult> sendToGFBIO(Broker broker, long datasetId, DatasetVersion datasetVersion, string zipfilepath)
        {
            //PublicationManager publicationManager = new PublicationManager();
            //DatasetManager datasetManager = new DatasetManager();

            ////create a gfbio api webservice manager
            //GFBIOWebserviceManager gfbioWebserviceManager = new GFBIOWebserviceManager(broker);
            //GFBIOException gfbioException = null;
            ////get user from system
            //string username = HttpContext.User.Identity.Name;
            //UserManager userManager = new UserManager();

            //// EXPERIMENT: See if the method call waits for the result or not!
            //var user = userManager.FindByNameAsync(username).Result;

            ////check if user exist and api user has access
            //string jsonresult = await gfbioWebserviceManager.GetUserByEmail(user.Email);
            //GFBIOUser gfbioUser = new JavaScriptSerializer().Deserialize<GFBIOUser>(jsonresult);

            ////if user not exist, api call was failed
            //if (gfbioUser.userid == 0)
            //{
            //    //get the exception
            //    gfbioException = new JavaScriptSerializer().Deserialize<GFBIOException>(jsonresult);

            //    if (!String.IsNullOrEmpty(gfbioException.exception))
            //        return Json(gfbioException.message);

            //    return Json(jsonresult);
            //}
            ////user exist and api user has access to the api´s
            //else
            //{
            //    //load metadata from version
            //    Dataset dataset = datasetManager.GetDataset(datasetId);
            //    XmlDocument metadata = datasetManager.GetDatasetLatestMetadataVersion(datasetId);

            //    //ToDo [SUBMISSION] -> add infos from metadata via system mapping
            //    string projectName = "Bexis 2 Instance Project";
            //    string projectDescription = "Bexis 2 Instance Project Description";

            //    //grab project from metadata
            //    projectName = MappingUtils.GetValuesFromMetadata(Convert.ToInt64(Key.ProjectTitle), LinkElementType.Key, dataset.MetadataStructure.Id,
            //        XmlUtility.ToXDocument(metadata)).FirstOrDefault();

            //    //create or get project
            //    string projectJsonResult =
            //        await gfbioWebserviceManager.GetProjectsByUser(gfbioUser.userid);

            //    var projects =
            //        new JavaScriptSerializer().Deserialize<List<GFBIOProject>>(projectJsonResult);

            //    GFBIOProject gbfioProject = new GFBIOProject();

            //    if (!projects.Any(p => p.name.Equals(projectName)))
            //    {
            //        string createProjectJsonResult = await gfbioWebserviceManager.CreateProject(
            //            gfbioUser.userid, projectName, projectDescription);

            //        gbfioProject =
            //            new JavaScriptSerializer().Deserialize<GFBIOProject>(createProjectJsonResult);

            //        //if (!String.IsNullOrEmpty(gfbioException.exception))
            //        //return Json(createProjectJsonResult);
            //    }
            //    else
            //    {
            //        gbfioProject = projects.Where(p => p.name.Equals(projectName)).FirstOrDefault();

            //    }

            //    //grab title from metadata
            //    string name = MappingUtils.GetValuesFromMetadata(Convert.ToInt64(Key.Title), LinkElementType.Key, dataset.MetadataStructure.Id,
            //        XmlUtility.ToXDocument(metadata)).FirstOrDefault();

            //    //grab description from metadata
            //    string description = MappingUtils.GetValuesFromMetadata(Convert.ToInt64(Key.Description), LinkElementType.Key, dataset.MetadataStructure.Id,
            //        XmlUtility.ToXDocument(metadata)).FirstOrDefault();

            //    //grab authores from metadata
            //    List<string> authorList = new List<string>();
            //    authorList = MappingUtils.GetValuesFromMetadata(Convert.ToInt64(Key.Author), LinkElementType.Key, dataset.MetadataStructure.Id,
            //        XmlUtility.ToXDocument(metadata));

            //    string metadataLabel = dataset.MetadataStructure.Name;

            //    //TODO based on the data policy there must be a decision what should be in the extended data as a example of the dataset. at first metadata is added
            //    //create extended Data
            //    XmlDocument metadataExportFormat = OutputMetadataManager.GetConvertedMetadata(datasetId,
            //        TransmissionType.mappingFileExport,
            //        broker.MetadataFormat);

            //    string roJsonResult = "";

            //    //if a publication from a previus version exist, the researchobject need to get
            //    long researchobjectId = 0;

            //    var tmp = publicationManager.GetPublication().FirstOrDefault(
            //    p => p.DatasetVersion.Dataset.Id.Equals(datasetId) && p.Broker.Name.ToLower().Equals(datarepo.ToLower()));

            //    if (tmp != null) researchobjectId = tmp.ResearchObjectId;

            //    if (researchobjectId == 0)
            //    {
            //        roJsonResult = await gfbioWebserviceManager.CreateResearchObject(
            //            gfbioUser.userid,
            //            gbfioProject.projectid,
            //            name,
            //            description,
            //            "Dataset",
            //            metadataExportFormat,
            //            authorList,
            //            metadataLabel
            //            );
            //    }
            //    else
            //    {
            //        //todo update
            //        roJsonResult = await gfbioWebserviceManager.UpdateResearchObject(
            //            researchobjectId,
            //            name,
            //            description,
            //            metadataExportFormat,
            //            authorList
            //            );
            //    }

            //    List<GFBIOResearchObjectResult> gfbioResearchObjectList =
            //        new JavaScriptSerializer().Deserialize<List<GFBIOResearchObjectResult>>(roJsonResult);
            //    GFBIOResearchObjectResult gfbioResearchObject = gfbioResearchObjectList.FirstOrDefault();

            //    if (gfbioResearchObject != null && gfbioResearchObject.researchobjectid > 0)
            //    {
            //        // reseachhobject exist

            //        string roStatusJsonResult =
            //            await
            //                gfbioWebserviceManager.GetStatusByResearchObjectById(
            //                    gfbioResearchObject.researchobjectid);

            //        //get status and store ro
            //        List<GFBIOResearchObjectStatus> gfbioRoStatusList =
            //            new JavaScriptSerializer().Deserialize<List<GFBIOResearchObjectStatus>>(
            //                roStatusJsonResult);
            //        GFBIOResearchObjectStatus gfbioRoStatus = gfbioRoStatusList.LastOrDefault();

            //        //Store ro in db
            //        string title = xmlDatasetHelper.GetInformationFromVersion(datasetVersion.Id,
            //            NameAttributeValues.title);
            //        publicationManager.CreatePublication(datasetVersion, broker, title,
            //            gfbioRoStatus.researchobjectid, zipfilepath, "",
            //            gfbioRoStatus.status);

            //    }
            //    else
            //    {
            //        gfbioException = new JavaScriptSerializer().Deserialize<GFBIOException>(roJsonResult);

            //        //if (!String.IsNullOrEmpty(gfbioException.exception))
            //        return Json(roJsonResult);
            //    }

            return Json("");
        }

        #endregion GFBIO
    }

    #endregion submission
}