using BExIS.Dim.Entities.Publication;
using BExIS.Dim.Helpers;
using BExIS.Dim.Helpers.GFBIO;
using BExIS.Dim.Services;
using BExIS.Dlm.Entities.Data;
using BExIS.Dlm.Entities.DataStructure;
using BExIS.Dlm.Services.Data;
using BExIS.Dlm.Services.DataStructure;
using BExIS.IO;
using BExIS.IO.Transform.Output;
using BExIS.Modules.Dim.UI.Models;
using BExIS.Security.Entities.Authorization;
using BExIS.Security.Entities.Subjects;
using BExIS.Security.Services.Authorization;
using BExIS.Security.Services.Subjects;
using BExIS.Xml.Helpers;
using Ionic.Zip;
using Microsoft.AspNet.Identity;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using System.Xml;
using Vaiona.Logging;
using Vaiona.Utils.Cfg;

namespace BExIS.Modules.Dim.UI.Controllers
{
    public class SubmissionController : Controller
    {
        // GET: Submission

        #region submission

        /// <summary>
        /// ToDo Refactor submission Commented by Javad due to modularity issues.
        /// Thes functions should call the APIs of the DIM module and get json objects back.
        /// If Publication or any other entity is not part of the DLM, it is visible only to its own module.
        /// Other mosules who consume the API results of a module, should only expect .NET types, DLM types, json, xml, CSV, or Html.
        /// </summary>

        public ActionResult publishData(long datasetId, long datasetVersionId = -1)
        {
            PublicationManager publicationManager = new PublicationManager();
            SubmissionManager publishingManager = new SubmissionManager();
            publishingManager.Load();

            ShowPublishDataModel model = new ShowPublishDataModel();

            List<Broker> Brokers = publicationManager.BrokerRepo.Get().ToList();

            model.Brokers = Brokers.Select(b => b.Name).ToList();
            model.DatasetId = datasetId;

            EntityPermissionManager entityPermissionManager = new EntityPermissionManager();


            //Todo Download Rigths -> currently set read rigths for this case
            model.DownloadRights = entityPermissionManager.HasRight<User>(HttpContext.User.Identity.Name, "Dataset", typeof(Dataset), datasetId, RightType.Read);
            model.EditRights = entityPermissionManager.HasRight<User>(HttpContext.User.Identity.Name, "Dataset", typeof(Dataset), datasetId, RightType.Write);


            List<long> versions = new List<long>();
            if (datasetVersionId == -1)
            {
                DatasetManager datasetManager = new DatasetManager();
                datasetVersionId = datasetManager.GetDatasetLatestVersion(datasetId).Id;
                versions = datasetManager.GetDatasetVersions(datasetId).Select(d => d.Id).ToList();
            }

            //todo check if datasetversion id is correct
            List<Publication> publications = publicationManager.PublicationRepo.Get().Where(p => versions.Contains(p.DatasetVersion.Id)).ToList();

            foreach (var pub in publications)
            {
                Broker broker = publicationManager.BrokerRepo.Get(pub.Broker.Id);

                model.Publications.Add(new PublicationModel()
                {
                    Broker = broker.Name,
                    DatasetVersionId = datasetVersionId,
                    CreationDate = pub.Timestamp,
                    ExternalLink = pub.ExternalLink,
                    FilePath = pub.FilePath,
                    Status = pub.Status
                });
            }

            return View("_showPublishDataView", model);
        }

        public ActionResult getPublishDataPartialView(long datasetId, long datasetVersionId = -1)
        {
            PublicationManager publicationManager = new PublicationManager();
            SubmissionManager publishingManager = new SubmissionManager();
            publishingManager.Load();

            ShowPublishDataModel model = new ShowPublishDataModel();

            List<Broker> Brokers = publicationManager.BrokerRepo.Get().ToList();

            model.Brokers = Brokers.Select(b => b.Name).ToList();
            model.DatasetId = datasetId;

            EntityPermissionManager entityPermissionManager = new EntityPermissionManager();


            //Todo Download Rigths -> currently set read rigths for this case
            model.DownloadRights = entityPermissionManager.HasRight<User>(HttpContext.User.Identity.Name, "Dataset", typeof(Dataset), datasetId, RightType.Read);
            model.EditRights = entityPermissionManager.HasRight<User>(HttpContext.User.Identity.Name, "Dataset", typeof(Dataset), datasetId, RightType.Write);


            List<long> versions = new List<long>();
            if (datasetVersionId == -1)
            {
                DatasetManager datasetManager = new DatasetManager();
                datasetVersionId = datasetManager.GetDatasetLatestVersion(datasetId).Id;
                versions = datasetManager.GetDatasetVersions(datasetId).Select(d => d.Id).ToList();
            }

            //todo check if datasetversion id is correct
            List<Publication> publications = publicationManager.PublicationRepo.Get().Where(p => versions.Contains(p.DatasetVersion.Id)).ToList();

            foreach (var pub in publications)
            {
                Broker broker = publicationManager.BrokerRepo.Get(pub.Broker.Id);

                model.Publications.Add(new PublicationModel()
                {
                    Broker = broker.Name,
                    DatasetVersionId = datasetVersionId,
                    CreationDate = pub.Timestamp,
                    ExternalLink = pub.ExternalLink,
                    FilePath = pub.FilePath,
                    Status = pub.Status
                });
            }

            return PartialView("_showPublishDataView", model);
        }

        public ActionResult LoadDataRepoRequirementView(string datarepo, long datasetid)
        {
            DataRepoRequirentModel model = new DataRepoRequirentModel();
            model.DatasetId = datasetid;

            //get broker
            PublicationManager publicationManager = new PublicationManager();


            // datasetversion
            DatasetManager dm = new DatasetManager();
            long version = dm.GetDatasetLatestVersion(datasetid).Id;
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
                        XmlDatasetHelper.GetAllTransmissionInformation(datasetid,
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

                    #endregion
                }

            }

            return PartialView("_dataRepositoryRequirementsView", model);
        }

        public JsonResult CheckExportPossibility(string datarepo, long datasetid)
        {

            bool isDataConvertable = false;
            bool isMetadataConvertable = false;
            string metadataValidMessage = "";
            bool exist = false;

            //get broker
            PublicationManager publicationManager = new PublicationManager();


            // datasetversion
            DatasetManager dm = new DatasetManager();
            long version = dm.GetDatasetLatestVersion(datasetid).Id;

            if (publicationManager.BrokerRepo.Get().Any(d => d.Name.ToLower().Equals(datarepo.ToLower())))
            {

                Broker broker =
                   publicationManager.BrokerRepo.Get().Where(d => d.Name.ToLower().Equals(datarepo.ToLower())).FirstOrDefault();

                Publication publication =
                    publicationManager.PublicationRepo.Get()
                        .Where(p => p.Broker.Id.Equals(broker.Id) && p.DatasetVersion.Id.Equals(version))
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
                            XmlDatasetHelper.GetAllTransmissionInformation(datasetid,
                                TransmissionType.mappingFileExport, AttributeNames.name).ToList();
                        if (exportNames.Contains(broker.MetadataFormat))
                            isMetadataConvertable = true;

                        metadataValidMessage = OutputMetadataManager.IsValideAgainstSchema(datasetid,
                            TransmissionType.mappingFileExport, datarepo);

                    }

                    #endregion

                    #region primary Data

                    //todo need a check if the primary data is structured or not, if its unstructured also export should be possible

                    if (broker.PrimaryDataFormat.ToLower().Contains("text/plain") ||
                        broker.PrimaryDataFormat.ToLower().Contains("text/csv") ||
                        broker.PrimaryDataFormat.ToLower().Contains("application/excel") ||
                        String.IsNullOrEmpty(broker.PrimaryDataFormat))
                    {
                        isDataConvertable = true;
                    }

                    #endregion
                }


                //check if reporequirements are fit
                //e.g. GFBIO

            }

            return Json(new { isMetadataConvertable = isMetadataConvertable, isDataConvertable = isDataConvertable, metadataValidMessage = metadataValidMessage, Exist = exist });
        }

        public ActionResult DownloadZip(string datarepo, long datasetversionid)
        {
            string path = "";

            PublicationManager publicationManager = new PublicationManager();
            SubmissionManager publishingManager = new SubmissionManager();

            Publication publication = publicationManager.PublicationRepo.Get().Where(p => p.DatasetVersion.Id.Equals(datasetversionid)).LastOrDefault();

            if (publication != null)
            {
                Broker broker = publicationManager.BrokerRepo.Get(publication.Broker.Id);
                if (broker.Name.ToLower().Equals(datarepo.ToLower()))
                {
                    DatasetManager datasetManager = new DatasetManager();
                    DatasetVersion dsv = datasetManager.GetDatasetVersion(datasetversionid);
                    long datasetid = dsv.Dataset.Id;


                    string zipName = publishingManager.GetZipFileName(datasetid, datasetversionid);
                    path = Path.Combine(AppConfiguration.DataPath, publication.FilePath);

                    return File(path, "application/zip", zipName);
                }
            }

            return null;
        }

        public async Task<ActionResult> PrepareData(long datasetId, string datarepo)
        {
            DatasetManager datasetManager = new DatasetManager();
            DatasetVersion datasetVersion = datasetManager.GetDatasetLatestVersion(datasetId);
            PublicationManager publicationManager = new PublicationManager();
            SubmissionManager publishingManager = new SubmissionManager();


            Publication publication =
                publicationManager.GetPublication()
                    .Where(
                        p =>
                            p.DatasetVersion.Id.Equals(datasetVersion.Id) &&
                            p.Broker.Name.ToLower().Equals(datarepo.ToLower()))
                    .FirstOrDefault();
            // if(broker exist)
            if (publication == null && publicationManager.GetBroker().Any(b => b.Name.ToLower().Equals(datarepo.ToLower())))
            {
                //SubmissionManager publishingManager = new SubmissionManager();
                //publishingManager.Load();
                //DataRepository dataRepository = publishingManager.DataRepositories.Where(d => d.Name.Equals(datarepo)).FirstOrDefault();

                Broker broker = publicationManager.GetBroker().Where(b => b.Name.ToLower().Equals(datarepo.ToLower())).FirstOrDefault();

                if (broker != null)
                {

                    OutputMetadataManager.GetConvertedMetadata(datasetId, TransmissionType.mappingFileExport,
                        broker.MetadataFormat);

                    // get primary data
                    // check the data sturcture type ...
                    if (datasetVersion.Dataset.DataStructure.Self is StructuredDataStructure)
                    {
                        OutputDataManager odm = new OutputDataManager();
                        // apply selection and projection

                        string title = XmlDatasetHelper.GetInformation(datasetVersion, NameAttributeValues.title);

                        odm.GenerateAsciiFile(datasetId, title, broker.PrimaryDataFormat);
                    }

                    string zipName = publishingManager.GetZipFileName(datasetId, datasetVersion.Id);
                    string zipPath = publishingManager.GetDirectoryPath(datasetId, broker.Name);
                    string dynamicZipPath = publishingManager.GetDynamicDirectoryPath(datasetId, broker.Name);
                    string zipFilePath = Path.Combine(zipPath, zipName);
                    string dynamicFilePath = Path.Combine(dynamicZipPath, zipName);

                    FileHelper.CreateDicrectoriesIfNotExist(Path.GetDirectoryName(zipFilePath));

                    if (FileHelper.FileExist(zipFilePath))
                    {
                        if (FileHelper.WaitForFile(zipFilePath))
                        {
                            FileHelper.Delete(zipFilePath);
                        }
                    }



                    // add datastructure
                    //ToDo put that functiom to the outputDatatructureManager
                    #region datatructure

                    DataStructureManager dataStructureManager = new DataStructureManager();

                    long dataStructureId = datasetVersion.Dataset.DataStructure.Id;
                    DataStructure dataStructure = dataStructureManager.StructuredDataStructureRepo.Get(dataStructureId);

                    if (dataStructure != null)
                    {

                        try
                        {
                            string dynamicPathOfDS = "";
                            dynamicPathOfDS = storeGeneratedFilePathToContentDiscriptor(datasetId, datasetVersion, "datastructure", ".txt");
                            string datastructureFilePath = AsciiWriter.CreateFile(dynamicPathOfDS);

                            string json = OutputDataStructureManager.GetDataStructureAsJson(datasetId);

                            AsciiWriter.AllTextToFile(datastructureFilePath, json);


                        }
                        catch (Exception ex)
                        {
                            throw ex;
                        }
                    }

                    #endregion

                    ZipFile zip = new ZipFile();

                    foreach (ContentDescriptor cd in datasetVersion.ContentDescriptors)
                    {
                        string path = Path.Combine(AppConfiguration.DataPath, cd.URI);
                        string name = cd.URI.Split('\\').Last();

                        if (FileHelper.FileExist(path))
                        {
                            zip.AddFile(path, "");
                        }
                    }


                    // add xsd of the metadata schema
                    string xsdDirectoryPath = OutputMetadataManager.GetSchemaDirectoryPath(datasetId);
                    if (Directory.Exists(xsdDirectoryPath))
                        zip.AddDirectory(xsdDirectoryPath, "Schema");

                    XmlDocument manifest = OutputDatasetManager.GenerateManifest(datasetId, datasetVersion.Id);

                    if (manifest != null)
                    {
                        string dynamicManifestFilePath = OutputDatasetManager.GetDynamicDatasetStorePath(datasetId,
                            datasetVersion.Id, "manifest", ".xml");
                        string fullFilePath = Path.Combine(AppConfiguration.DataPath, dynamicManifestFilePath);

                        manifest.Save(fullFilePath);
                        zip.AddFile(fullFilePath, "");

                    }

                    string message = string.Format("dataset {0} version {1} was published for repository {2}", datasetId,
                        datasetVersion.Id, broker.Name);
                    LoggerFactory.LogCustom(message);


                    Session["ZipFilePath"] = dynamicFilePath;

                    zip.Save(zipFilePath);
                }
            }


            return RedirectToAction("getPublishDataPartialView", new { datasetId });
        }

        public async Task<ActionResult> SendDataToDataRepo(long datasetId, string datarepo)
        {
            try
            {
                string zipfilepath = "";
                if (Session["ZipFilePath"] != null)
                    zipfilepath = Session["ZipFilePath"].ToString();

                DatasetManager datasetManager = new DatasetManager();
                DatasetVersion datasetVersion = datasetManager.GetDatasetLatestVersion(datasetId);
                PublicationManager publicationManager = new PublicationManager();

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
                            //create a gfbio api webservice manager
                            GFBIOWebserviceManager gfbioWebserviceManager = new GFBIOWebserviceManager(broker);
                            GFBIOException gfbioException = null;
                            //get user from system
                            string username = HttpContext.User.Identity.Name;
                            UserManager userManager = new UserManager(new UserStore());

                            User user = userManager.FindByName(username);

                            //check if user exist and api user has access
                            string jsonresult = await gfbioWebserviceManager.GetUserByEmail(user.Email);
                            GFBIOUser gfbioUser = new JavaScriptSerializer().Deserialize<GFBIOUser>(jsonresult);

                            //if user not exist, api call was failed
                            if (gfbioUser.userid == 0)
                            {
                                //get the exception
                                gfbioException = new JavaScriptSerializer().Deserialize<GFBIOException>(jsonresult);

                                if (!String.IsNullOrEmpty(gfbioException.exception))
                                    return Json(jsonresult);
                            }
                            //user exist and api user has access to the api´s
                            else
                            {

                                //ToDo [SUBMISSION] -> add infos from metadata via system mapping
                                string projectName = "Bexis 2 Instance Project";
                                string projectDescription = "Bexis 2 Instance Project Description";


                                // ToDo submission test users, set project and more parameters
                                if (user.Name.ToLower().Equals("drwho"))
                                {
                                    projectName = "Time Traveler";
                                    projectDescription = "Project to find places that are awesome!!";
                                }


                                if (user.Name.ToLower().Equals("mcfly"))
                                {
                                    projectName = "Back to the Future";
                                    projectDescription = "Meet your parents in the past";

                                }

                                if (user.Name.ToLower().Equals("arthurdent"))
                                {
                                    projectName = "Per Anhalter durch die Galaxie";
                                    projectDescription = "Find the answer of life and so.";
                                }

                                //create or get project
                                string projectJsonResult =
                                    await gfbioWebserviceManager.GetProjectsByUser(gfbioUser.userid);

                                var projects =
                                    new JavaScriptSerializer().Deserialize<List<GFBIOProject>>(projectJsonResult);

                                GFBIOProject gbfioProject = new GFBIOProject();

                                if (!projects.Any(p => p.name.Equals(projectName)))
                                {
                                    string createProjectJsonResult = await gfbioWebserviceManager.CreateProject(
                                        gfbioUser.userid, projectName, projectDescription);

                                    gbfioProject =
                                        new JavaScriptSerializer().Deserialize<GFBIOProject>(createProjectJsonResult);

                                    //if (!String.IsNullOrEmpty(gfbioException.exception))
                                    //return Json(createProjectJsonResult);
                                }
                                else
                                {
                                    gbfioProject = projects.Where(p => p.name.Equals(projectName)).FirstOrDefault();

                                }



                                string name = XmlDatasetHelper.GetInformation(datasetId, NameAttributeValues.title);
                                string description = XmlDatasetHelper.GetInformation(datasetId,
                                    NameAttributeValues.description);


                                //TODO based on the data policy there must be a decision what should be in the extended data as a example of the dataset. at first metadata is added            
                                //create extended Data
                                XmlDocument metadataExportFormat = OutputMetadataManager.GetConvertedMetadata(datasetId,
                                    TransmissionType.mappingFileExport,
                                    broker.MetadataFormat);

                                string extendedDataAsJSON = JsonConvert.SerializeXmlNode(metadataExportFormat);

                                string roJsonResult = await gfbioWebserviceManager.CreateResearchObject(
                                    gfbioUser.userid,
                                    gbfioProject.projectid,
                                    name,
                                    description,
                                    "Dataset",
                                    extendedDataAsJSON,
                                    null
                                    );

                                List<GFBIOResearchObjectResult> gfbioResearchObjectList =
                                    new JavaScriptSerializer().Deserialize<List<GFBIOResearchObjectResult>>(roJsonResult);
                                GFBIOResearchObjectResult gfbioResearchObject = gfbioResearchObjectList.FirstOrDefault();

                                if (gfbioResearchObject != null && gfbioResearchObject.researchobjectid > 0)
                                {
                                    // reseachhobject exist

                                    string roStatusJsonResult =
                                        await
                                            gfbioWebserviceManager.GetStatusByResearchObjectById(
                                                gfbioResearchObject.researchobjectid);

                                    //get status and store ro
                                    List<GFBIOResearchObjectStatus> gfbioRoStatusList =
                                        new JavaScriptSerializer().Deserialize<List<GFBIOResearchObjectStatus>>(
                                            roStatusJsonResult);
                                    GFBIOResearchObjectStatus gfbioRoStatus = gfbioRoStatusList.LastOrDefault();

                                    //Store ro in db
                                    string title = XmlDatasetHelper.GetInformation(datasetVersion,
                                        NameAttributeValues.title);
                                    publicationManager.CreatePublication(datasetVersion, broker, title,
                                        gfbioRoStatus.researchobjectid, zipfilepath, "",
                                        gfbioRoStatus.status);

                                }
                                else
                                {
                                    gfbioException = new JavaScriptSerializer().Deserialize<GFBIOException>(roJsonResult);

                                    //if (!String.IsNullOrEmpty(gfbioException.exception))
                                    return Json(roJsonResult);
                                }

                            }

                        }

                        #endregion
                    }


                    if (datarepo.ToLower().Equals("generic"))
                    {
                        #region GENERIC

                        Broker broker =
                            publicationManager.BrokerRepo.Get()
                                .Where(b => b.Name.ToLower().Equals(datarepo.ToLower()))
                                .FirstOrDefault();
                        string title = XmlDatasetHelper.GetInformation(datasetVersion, NameAttributeValues.title);
                        publicationManager.CreatePublication(datasetVersion, broker, title, 0, zipfilepath, "",
                            "created");

                        #endregion
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


            return Json(true);
        }


        #region helper


        private static string storeGeneratedFilePathToContentDiscriptor(long datasetId, DatasetVersion datasetVersion, string title, string ext)
        {

            string name = "";
            string mimeType = "";

            if (ext.Contains("csv"))
            {
                name = "datastructure";
                mimeType = "text/comma-separated-values";
            }


            // create the generated FileStream and determine its location
            string dynamicPath = OutputDatasetManager.GetDynamicDatasetStorePath(datasetId, datasetVersion.Id, title, ext);
            //Register the generated data FileStream as a resource of the current dataset version
            //ContentDescriptor generatedDescriptor = new ContentDescriptor()
            //{
            //    OrderNo = 1,
            //    Name = name,
            //    MimeType = mimeType,
            //    URI = dynamicPath,
            //    DatasetVersion = datasetVersion,
            //};

            DatasetManager dm = new DatasetManager();
            if (datasetVersion.ContentDescriptors.Count(p => p.Name.Equals(name)) > 0)
            {   // remove the one contentdesciptor 
                foreach (ContentDescriptor cd in datasetVersion.ContentDescriptors)
                {
                    if (cd.Name == name)
                    {
                        cd.URI = dynamicPath;
                        dm.UpdateContentDescriptor(cd);
                    }
                }
            }
            else
            {
                // add current contentdesciptor to list
                //datasetVersion.ContentDescriptors.Add(generatedDescriptor);
                dm.CreateContentDescriptor(name, mimeType, dynamicPath, 1, datasetVersion);
            }

            //dm.EditDatasetVersion(datasetVersion, null, null, null);
            return dynamicPath;
        }

    }

    #endregion
    #endregion
}
