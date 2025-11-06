using BExIS.App.Bootstrap;
using BExIS.Dim.Entities.Publications;
using BExIS.Dim.Helpers;
using BExIS.Dim.Helpers.Export;
using BExIS.Dim.Services;
using BExIS.Dlm.Entities.Data;
using BExIS.Dlm.Entities.DataStructure;
using BExIS.Dlm.Entities.MetadataStructure;
using BExIS.Dlm.Entities.Party;
using BExIS.Dlm.Services.Data;
using BExIS.Dlm.Services.DataStructure;
using BExIS.Dlm.Services.Party;
using BExIS.IO;
using BExIS.IO.Transform.Output;
using BExIS.Modules.Dim.UI.Helpers;
using BExIS.Modules.Dim.UI.Models.Api;
using BExIS.Modules.Dim.UI.Models.Download;
using BExIS.Modules.Dim.UI.Models.Export;
using BExIS.Security.Entities.Subjects;
using BExIS.Security.Entities.Versions;
using BExIS.Security.Services.Utilities;
using BExIS.UI.Helpers;
using BExIS.Utils.Config;
using BExIS.Utils.Extensions;
using BExIS.Utils.Models;
using BExIS.Utils.NH.Querying;
using BExIS.Xml.Helpers;
using Newtonsoft.Json;
using System;
using System.Data;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web.Mvc;
using System.Web.Routing;
using System.Xml;
using Telerik.Web.Mvc;
using Vaelastrasz.Library.Models;
using Vaiona.Logging;
using Vaiona.Persistence.Api;
using Vaiona.Utils.Cfg;
using Vaiona.Web.Extensions;
using Vaiona.Web.Mvc.Modularity;
using static System.Net.Mime.MediaTypeNames;

namespace BExIS.Modules.Dim.UI.Controllers
{
    public class ExportController : Controller
    {
        // GET: Export
        /// <summary>
        ///
        /// </summary>
        /// <param name="datasetVersionId"></param>
        /// <param name="primaryDataFormat">mimetype like text/csv, text/plain</param>
        /// <returns></returns>
        public ActionResult GetZipOfDatasetVersion(long datasetVersionId, string primaryDataFormat = "")
        {
            using (var uow = this.GetUnitOfWork())
            {
                DatasetVersion dsv = uow.GetReadOnlyRepository<DatasetVersion>().Get(datasetVersionId);
                MetadataStructure metadataStructure = uow.GetReadOnlyRepository<MetadataStructure>().Get(dsv.Dataset.MetadataStructure.Id);
                Broker broker = new Broker();
                broker.Name = "generic";
                broker.MetadataFormat = metadataStructure.Name;
                broker.MetadataFormat = primaryDataFormat;

                Repository dataRepo = new Repository();
                dataRepo.Name = "generic";
                broker.Repository = dataRepo;

                GenericDataRepoConverter dataRepoConverter = new GenericDataRepoConverter(broker);
                Tuple<string, string> tmp = new Tuple<string, string>(dataRepoConverter.Convert(datasetVersionId), "application/zip");

                return File(tmp.Item1, tmp.Item2, Path.GetFileName(tmp.Item1));
            }
        }

        /// <summary>
        /// Return a metadata as html file from a  datasetversion
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult GetMetadataAsHtmlFile(long id)
        {
            DatasetManager dm = new DatasetManager();

            try
            {
                using (var uow = this.GetUnitOfWork())
                {
                    long dsId = dm.GetDatasetLatestVersion(id).Id;
                    DatasetVersion ds = uow.GetUnitOfWork().GetReadOnlyRepository<DatasetVersion>().Get(dsId);

                    XmlDocument document = OutputMetadataManager.GetConvertedMetadata(id, TransmissionType.mappingFileExport,
                             ds.Dataset.MetadataStructure.Name);

                    string htmlPage = PartialView("SimpleMetadata", document).RenderToString();
                    byte[] content = Encoding.ASCII.GetBytes(htmlPage);

                    return File(content, "text/html", "metadata.html");
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                dm.Dispose();
            }
        }

        /// <summary>
        /// Get a html file of the data structure from a dataset
        /// </summary>
        /// <param name="id">dataset id</param>
        /// <returns>html file</returns>
        public ActionResult GetDataStructureAsHtmlFile(long id)
        {
            DatasetManager dm = new DatasetManager();
            DataStructureManager dsm = new DataStructureManager();

            try
            {
                using (var uow = this.GetUnitOfWork())
                {
                    long dsId = dm.GetDatasetLatestVersion(id).Id;
                    DatasetVersion ds = uow.GetUnitOfWork().GetReadOnlyRepository<DatasetVersion>().Get(dsId);
                    DataStructure dataStructure = null;
                    if (ds != null) dataStructure = uow.GetReadOnlyRepository<DataStructure>().Get(ds.Dataset.DataStructure.Id);

                    if (dataStructure != null && dataStructure.Self is StructuredDataStructure)
                    {
                        SimpleDataStructureModel model = new SimpleDataStructureModel((StructuredDataStructure)dataStructure.Self);

                        string htmlPage = PartialView("SimpleDataStructure", model).RenderToString();
                        byte[] content = Encoding.ASCII.GetBytes(htmlPage);

                        return File(content, "text/html", "dataStructure.html");
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                dm.Dispose();
                dsm.Dispose();
            }

            return null;
        }

        public ActionResult SimpleDataStructure(long id)
        {
            DatasetManager dm = new DatasetManager();
            DataStructureManager dsm = new DataStructureManager();

            try
            {
                using (var uow = this.GetUnitOfWork())
                {
                    long dsId = dm.GetDatasetLatestVersion(id).Id;
                    DatasetVersion ds = uow.GetUnitOfWork().GetReadOnlyRepository<DatasetVersion>().Get(dsId);
                    DataStructure dataStructure = null;
                    if (ds != null) dataStructure = uow.GetReadOnlyRepository<DataStructure>().Get(ds.Dataset.DataStructure.Id);

                    if (dataStructure != null && dataStructure.Self is StructuredDataStructure)
                    {
                        SimpleDataStructureModel model = new SimpleDataStructureModel((StructuredDataStructure)dataStructure.Self);

                        return PartialView("SimpleDataStructure", model);
                    }
                }

                return null;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                dm.Dispose();
                dsm.Dispose();
            }
        }

        public ActionResult GenerateZip(long id, long versionid, string format,bool withFilter = false, bool withUnits = false)
        {
            XmlDatasetHelper xmlDatasetHelper = new XmlDatasetHelper();
            DatasetManager dm = new DatasetManager();
            DataStructureManager dataStructureManager = new DataStructureManager();
            PublicationManager publicationManager = new PublicationManager();
            SubmissionManager publishingManager = new SubmissionManager();

            bool isFilterInUse = filterInUse();
            string filteredFilePath = "";

            string brokerName = "generic";

            try
            {
                using (var datasetManager = new DatasetManager())
                {
                    long datasetVersionId = versionid;
                    long dataStructureId = 0;
                    int datasetVersionNumber = dm.GetDatasetVersionNr(datasetVersionId);
                    DatasetVersion datasetVersion = datasetManager.GetDatasetVersion(datasetVersionId);
                    string title = "";    

                    #region Metadata

                    //metadata as XML
                    XmlDocument document = OutputMetadataManager.GetConvertedMetadata(id, TransmissionType.mappingFileExport, datasetVersion.Dataset.MetadataStructure.Name);

                    //generate data structure as html 
                    generateMetadataAsHtml(datasetVersion);

                    #endregion

                    #region primary data



                    // check the data sturcture type ...
                    if (format != null && datasetVersion.Dataset.DataStructure.Self is StructuredDataStructure && dm.GetDataTuplesCount(datasetVersion.Id) > 0)
                    {
                        OutputDataManager odm = new OutputDataManager();

                        //check wheter title is empty or not
                        title = String.IsNullOrEmpty(datasetVersion.Title) ? "no title available" : datasetVersion.Title;

                        if (isFilterInUse && withFilter)
                        {
                            DataTable filteredData = getFilteredData(id);
                            filteredFilePath = odm.GenerateAsciiFile("temp", filteredData, title, format, datasetVersion.Dataset.DataStructure.Id, withUnits);
                        }
                        else
                        {

                            switch (format)
                            {
                                case "application/xlsx":
                                    odm.GenerateExcelFile(id, datasetVersion.Id, withUnits);
                                    break;

                                case "application/xlsm":
                                    odm.GenerateExcelFile(id, datasetVersion.Id, true);
                                    break;

                                default:
                                    odm.GenerateAsciiFile(id, datasetVersion.Id, format, withUnits);
                                    break;
                            }
                        }
                    }

                 

                    #endregion

                    #region data structure

                    if (datasetVersion.Dataset.DataStructure != null)
                    {
                        dataStructureId = datasetVersion.Dataset.DataStructure.Id;
                        DataStructure dataStructure = dataStructureManager.StructuredDataStructureRepo.Get(dataStructureId);

                        string dataStructurePath = "";
                        dataStructurePath = storeGeneratedFilePathToContentDiscriptor(id, datasetVersion,
                            "datastructure", ".json");
                        string datastructureFilePath = AsciiWriter.CreateFile(dataStructurePath);

                        string json = OutputDataStructureManager.GetDataStructureAsJson(dataStructureId);

                        AsciiWriter.AllTextToFile(datastructureFilePath, json);

                        generateDataStructureHtml(datasetVersion);
                    }

                    #endregion

                    #region zip file

                    string zipName = IOHelper.GetFileName(FileType.Bundle, id, datasetVersionNumber, dataStructureId, title); //publishingManager.GetZipFileName(id, datasetVersionNumber);
                    // add suffix if filter is in use
                    if (isFilterInUse) zipName += "_filtered";

                    string zipPath = Path.Combine(publishingManager.GetDirectoryPath(id, brokerName), zipName+".zip");
                    FileHelper.CreateDicrectoriesIfNotExist(Path.GetDirectoryName(zipPath));

                    using (var zipFileStream = new FileStream(zipPath, FileMode.Create))
                    using (var archive = new ZipArchive(zipFileStream, ZipArchiveMode.Update))
                    {
                        // stored contentdescriptior key name in db for the format and the dataname
                        string dataName = getCDTypeName(format);

                        // content descriptors
                        foreach (ContentDescriptor cd in datasetVersion.ContentDescriptors)
                        {
                            string path = Path.Combine(AppConfiguration.DataPath, cd.URI);
                            string name = cd.URI.Split('\\').Last();
                            string ext = Path.GetExtension(name).ToLower();

                            if(cd.Name.StartsWith("generated") && !cd.Name.Equals(dataName)) continue;

                            if (FileHelper.FileExist(path))
                            {
                                if (!archive.Entries.Any(entry => entry.Name.EndsWith(name)))
                                {
                                    if (cd.Name.Equals("metadata"))
                                    {
                                        name = IOHelper.GetFileName(FileType.Metadata, id, datasetVersionNumber, dataStructureId) + ext;
                                    }
                                    else if (cd.Name.Equals("datastructure"))
                                    {
                                        name = IOHelper.GetFileName(FileType.DataStructure, id, datasetVersionNumber, dataStructureId) + ext;
                                    }
                                    else if (cd.Name.Contains("generated"))
                                    {
                                        name = IOHelper.GetFileName(FileType.PrimaryData, id, datasetVersionNumber, dataStructureId) + ext;
                                    }
                                    else
                                    {
                                        name = IOHelper.GetFileName(FileType.None, id, datasetVersionNumber, dataStructureId,cd.Name) + ext;
                                    }

                                    archive.AddFileToArchive(path, name);
                                }
                            }
                        }

                        // if data was filtered add a text file with information about filtering
                        if (isFilterInUse)
                        {
                            string path = Path.Combine(AppConfiguration.DataPath, filteredFilePath);
                            string ext = Path.GetExtension(filteredFilePath).ToLower();
                            string name = IOHelper.GetFileName(FileType.PrimaryData, id, datasetVersionNumber, dataStructureId)+"_filtered"+ ext;

                            archive.AddFileToArchive(filteredFilePath, name);
                        }



                        // xml schema
                        string xsdPath = OutputMetadataManager.GetSchemaDirectoryPath(id);
                        if (Directory.Exists(xsdPath))
                            archive.AddFolderToArchive(xsdPath, "Schema");

                        #region manifest
                        // manifest
                        ApiDatasetHelper apiDatasetHelper = new ApiDatasetHelper();
                        // get content
                        ApiDatasetModel apimodel = apiDatasetHelper.GetContent(datasetVersion, id, datasetVersionNumber, datasetVersion.Dataset.MetadataStructure.Id, dataStructureId);
                        GeneralMetadataModel datasetModel = GeneralMetadataModel.Map(apimodel);

                        datasetModel.DownloadInformation.DownloadDate = DateTime.Now.ToString(new CultureInfo("en-US"));
                        datasetModel.DownloadInformation.DownloadSource = Request.Url.Host;
                        datasetModel.DownloadInformation.DownloadedBy = getPartyNameOrDefault();

                        // set filter info 
                        if(isFilterInUse)
                            datasetModel.DownloadInformation.DownloadedFilter = getFilterQuery();

                        if(datasetModel.DownloadInformation.DownloadedBy == "DEFAULT") datasetModel.DownloadInformation.DownloadedBy = "ANONYMOUS";

                        string manifest = JsonConvert.SerializeObject(datasetModel);
                       
                        if (manifest != null)
                        {
                            string manifestFileName = IOHelper.GetFileName(FileType.Manifest, id, datasetVersionNumber, dataStructureId);
                            string manifestPath = OutputDatasetManager.GetDynamicDatasetStorePath(id,
                                datasetVersionNumber, "general_metadata", ".json");
                            string fullFilePath = Path.Combine(AppConfiguration.DataPath, manifestPath);
                            string directory = Path.GetDirectoryName(fullFilePath);
                            if (!Directory.Exists(directory))
                                FileHelper.CreateDicrectoriesIfNotExist(directory);

                           System.IO.File.WriteAllText(fullFilePath, manifest, System.Text.Encoding.UTF8);

                            archive.AddFileToArchive(fullFilePath, manifestFileName + ".json");
                        }
                        #endregion

                        #region terms and conditions

                        string termsPath = this.Session.GetTenant().GetResourcePath("terms");

                        if(!string.IsNullOrEmpty(termsPath) && System.IO.File.Exists(termsPath))
                            archive.AddFileToArchive(termsPath, "terms.txt");

                        #endregion terms and conditions

                        title = datasetVersion.Title;
                        title = String.IsNullOrEmpty(title) ? "unknown" : title;

                        string message = string.Format("dataset {0} version {1} was downloaded as zip - {2}.", id,
                        datasetVersionNumber, format);
                        LoggerFactory.LogCustom(message);

                        using (var emailService = new EmailService())
                        {
                            emailService.Send(MessageHelper.GetDownloadDatasetHeader(id, datasetVersionNumber),
                            MessageHelper.GetDownloadDatasetMessage(id, title, getPartyNameOrDefault(), "zip - " + format, datasetVersionNumber),
                            GeneralSettings.SystemEmail
                            );
                        }

                        return File(zipPath, "application/zip", Path.GetFileName(zipPath));
                    }

                    #endregion

                }
            }
            catch (Exception ex)
            {
                LoggerFactory.LogCustom("Error: " + ex.Message);
                throw ex;
            }
            finally
            {
                dm.Dispose();
                dataStructureManager.Dispose();
                publicationManager.Dispose();
            }
        }

        private string getCDTypeName(string mimeType)
        {
            switch (mimeType)
            {
                case "text/csv":
                case "text/comma-separated-values":
                case "application/octet-stream":
                    /* of course this is a wrong  mimetype for csv.
                    but the c# class MimeMapping.GetMimeMapping(ext) currently returns this as a result for .csv.
                    since we don't use the datatype at the moment,
                    it will be rebuilt into the case here*/
                    {
                        return "generatedCSV";
                    }
                case "text/tsv":
                case "text/tab-separated-values":
                    {
                        return "generatedTSV";
                    }
                case "application/xlsx":
                    {
                        return "generatedExcel";
                    }
                default:
                    {
                        return "generatedTXT";
                    }
            }
        }

        private string storeGeneratedFilePathToContentDiscriptor(long datasetId, DatasetVersion datasetVersion,
            string title, string ext)
        {
            string name = "";
            string mimeType = "";

            if (ext.Contains("csv"))
            {
                name = "datastructure";
                mimeType = "text/comma-separated-values";
            }

            if (ext.Contains("json"))
            {
                name = "datastructure";
                mimeType = "application/json";
            }

            if (ext.Contains("html"))
            {
                name = title;
                mimeType = "application/html";
            }

            using (DatasetManager dm = new DatasetManager())
            {
                int versionNr = dm.GetDatasetVersionNr(datasetVersion);

                // create the generated FileStream and determine its location
                string dynamicPath = OutputDatasetManager.GetDynamicDatasetStorePath(datasetId, versionNr, title,
                    ext);
                //Register the generated data FileStream as a resource of the current dataset version
                //ContentDescriptor generatedDescriptor = new ContentDescriptor()
                //{
                //    OrderNo = 1,
                //    Name = name,
                //    MimeType = mimeType,
                //    URI = dynamicPath,
                //    DatasetVersion = datasetVersion,
                //};

                if (datasetVersion.ContentDescriptors.Count(p => p.Name.Equals(name) && p.MimeType.Equals(mimeType)) > 0)
                {
                    // remove the one contentdesciptor
                    foreach (ContentDescriptor cd in datasetVersion.ContentDescriptors)
                    {
                        if (cd.Name.Equals(name) && cd.MimeType.Equals(mimeType))
                        {
                            cd.Name = name;
                            cd.MimeType = mimeType;
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

        private void generateMetadataAsHtml(DatasetVersion dsv)
        {
            XmlDatasetHelper xmlDatasetHelper = new XmlDatasetHelper();
            long datasetId = dsv.Dataset.Id;
            long metadatastructureId = dsv.Dataset.MetadataStructure.Id;
            long datastructureId = dsv.Dataset.DataStructure==null?0: dsv.Dataset.DataStructure.Id;
            long researchplanId = dsv.Dataset.ResearchPlan.Id;

            string title = dsv.Title;
            Session["ShowDataMetadata"] = dsv.Metadata;
            int versionNr = 0;
            var view = this.Render("DCM", "Form", "LoadMetadataOfflineVersion", new RouteValueDictionary()
            {
                { "entityId", datasetId },
                { "title", title },
                { "metadatastructureId", metadatastructureId },
                { "datastructureId", datastructureId },
                { "researchplanId", researchplanId },
                { "sessionKeyForMetadata", "ShowDataMetadata" },
                { "resetTaskManager", false }
            });

            byte[] content = Encoding.ASCII.GetBytes(view.ToString());

    
            string dynamicPathOfMD = "";
            dynamicPathOfMD = storeGeneratedFilePathToContentDiscriptor(datasetId, dsv,
                "metadata", ".html");
            string metadataFilePath = AsciiWriter.CreateFile(dynamicPathOfMD);

            AsciiWriter.AllTextToFile(metadataFilePath, view.ToString());
        }

        private void generateDataStructureHtml(DatasetVersion dsv)
        {
            var view = this.Render("DIM", "Export", "SimpleDataStructure", new RouteValueDictionary()
            {
                { "id", dsv.Dataset.Id }
            });

            byte[] content = Encoding.ASCII.GetBytes(view.ToString());
       
            string dynamicPathOfMD = "";
            dynamicPathOfMD = storeGeneratedFilePathToContentDiscriptor(dsv.Dataset.Id, dsv,
                "datastructure", ".html");
            string metadataFilePath = AsciiWriter.CreateFile(dynamicPathOfMD);

            AsciiWriter.AllTextToFile(metadataFilePath, view.ToString());
        }

        private string getPartyNameOrDefault()
        {
            var userName = string.Empty;
            try
            {
                userName = HttpContext.User.Identity.Name;
            }
            catch { }

            if (userName != null)
            {
                using (var uow = this.GetUnitOfWork())
                using (var partyManager = new PartyManager())
                {
                    var userRepository = uow.GetReadOnlyRepository<User>();
                    var user = userRepository.Query(s => s.Name.ToUpperInvariant() == userName.ToUpperInvariant()).FirstOrDefault();

                    if (user != null)
                    {
                        Party party = partyManager.GetPartyByUser(user.Id);
                        if (party != null)
                        {
                            return party.Name;
                        }
                    }
                }
            }
            return !string.IsNullOrWhiteSpace(userName) ? userName : "DEFAULT";
        }

        private bool filterInUse()
        {
            if (Session["DataFilter"] != null || Session["DataOrderBy"] != null || Session["DataProjection"] != null )
            {
                return true;
            }

            return false;
        }

        private DataTable getFilteredData(long datasetId)
        {
            DatasetManager datasetManager = new DatasetManager();
            try
            {
                GridCommand command = null;
                FilterExpression filter = null;
                OrderByExpression orderBy = null;
                ProjectionExpression projection = null;
                string[] columns = null;

                if (Session["DataFilter"] != null) filter  = (FilterExpression)Session["DataFilter"];
                if (Session["DataOrderBy"] != null) orderBy = (OrderByExpression)Session["DataOrderBy"];
                if (Session["DataProjection"] != null) projection = (ProjectionExpression)Session["DataProjection"];


                long count = datasetManager.RowCount(datasetId, filter);

                DataTable table = datasetManager.GetLatestDatasetVersionTuples(datasetId, filter, orderBy, projection, "", 0, (int)count);

                if (projection == null) table.Strip();

                return table;
            }
            finally
            {
                datasetManager.Dispose();
            }
        }

        private string getFilterQuery()
        {
            FilterExpression filter = null;
            OrderByExpression orderBy = null;
            ProjectionExpression projection = null;
            string[] columns = null;

            if (Session["DataFilter"] != null) filter = (FilterExpression)Session["DataFilter"];
            if (Session["DataOrderBy"] != null) orderBy = (OrderByExpression)Session["DataOrderBy"];
            if (Session["DataProjection"] != null) projection = (ProjectionExpression)Session["DataProjection"];

            string query = filter?.ToSQL() + orderBy?.ToSQL() + projection?.ToSQL();

            return query;
        }

    }
}