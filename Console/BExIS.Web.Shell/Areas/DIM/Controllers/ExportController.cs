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
using BExIS.Modules.Dim.UI.Models.Export;
using BExIS.Security.Entities.Subjects;
using BExIS.Security.Services.Utilities;
using BExIS.Utils.Config;
using BExIS.Utils.Extensions;
using BExIS.Xml.Helpers;
using Ionic.Zip;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using System.Web.Routing;
using System.Xml;
using Vaiona.Logging;
using Vaiona.Persistence.Api;
using Vaiona.Utils.Cfg;
using Vaiona.Web.Mvc.Modularity;

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

        public ActionResult GenerateZip(long id, long versionid, string format)
        {
            XmlDatasetHelper xmlDatasetHelper = new XmlDatasetHelper();
            DatasetManager dm = new DatasetManager();
            DataStructureManager dataStructureManager = new DataStructureManager();
            PublicationManager publicationManager = new PublicationManager();
            SubmissionManager publishingManager = new SubmissionManager();

            string brokerName = "generic";

            try
            {
                using (var uow = this.GetUnitOfWork())
                {
                    LoggerFactory.LogCustom("Generate Zip Start");
                    long dsvId = versionid;
                    if (dsvId <= 0) dsvId = dm.GetDatasetLatestVersion(id).Id;
                    DatasetVersion datasetVersion = uow.GetUnitOfWork().GetReadOnlyRepository<DatasetVersion>().Get(dsvId);
                    int versionNr = dm.GetDatasetVersionNr(datasetVersion);

                    #region metadata

                    LoggerFactory.LogCustom("Metadata Start");

                    //metadata as xml output
                    XmlDocument document = OutputMetadataManager.GetConvertedMetadata(id, TransmissionType.mappingFileExport,
                             datasetVersion.Dataset.MetadataStructure.Name);

                    //metadata as html
                    generateMetadataHtml(datasetVersion);

                    #endregion metadata

                    #region primary data

                    LoggerFactory.LogCustom("Primary Data Start");

                    // check the data sturcture type ...
                    if (format != null && datasetVersion.Dataset.DataStructure.Self is StructuredDataStructure && dm.GetDataTuplesCount(datasetVersion.Id) > 0)
                    {
                        OutputDataManager odm = new OutputDataManager();
                        // apply selection and projection

                        //check wheter title is empty or not
                        string title = String.IsNullOrEmpty(datasetVersion.Title) ? "no title available" : datasetVersion.Title;

                        switch (format)
                        {
                            case "application/xlsx":
                                odm.GenerateExcelFile(id, datasetVersion.Id, false);
                                break;

                            case "application/xlsm":
                                odm.GenerateExcelFile(id, datasetVersion.Id, true);
                                break;

                            default:
                                odm.GenerateAsciiFile(id, datasetVersion.Id, format, false);
                                break;
                        }
                    }

                    #endregion primary data

                    LoggerFactory.LogCustom("check zip on server Start");

                    string zipName = publishingManager.GetZipFileName(id, versionNr);
                    string zipPath = publishingManager.GetDirectoryPath(id, brokerName);
                    string dynamicZipPath = publishingManager.GetDynamicDirectoryPath(id, brokerName);
                    string zipFilePath = Path.Combine(zipPath, zipName);
                    //string dynamicFilePath = Path.Combine(dynamicZipPath, zipName);

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

                    LoggerFactory.LogCustom("Datastructure Start");

                   

                    if (datasetVersion.Dataset.DataStructure != null)
                    {
                        long dataStructureId = datasetVersion.Dataset.DataStructure.Id;
                        DataStructure dataStructure = dataStructureManager.StructuredDataStructureRepo.Get(dataStructureId);

                        try
                        {
                            string dynamicPathOfDS = "";
                            dynamicPathOfDS = storeGeneratedFilePathToContentDiscriptor(id, datasetVersion,
                                "datastructure", ".txt");
                            string datastructureFilePath = AsciiWriter.CreateFile(dynamicPathOfDS);

                            string json = OutputDataStructureManager.GetDataStructureAsJson(dataStructureId);

                            AsciiWriter.AllTextToFile(datastructureFilePath, json);

                            //generate datastructure as html
                            DatasetVersion ds = uow.GetUnitOfWork().GetReadOnlyRepository<DatasetVersion>().Get(dsvId);
                            generateDataStructureHtml(ds);
                        }
                        catch (Exception ex)
                        {
                            throw ex;
                        }
                    }

                    #endregion datatructure

                    LoggerFactory.LogCustom("Zip Start");

                    using (ZipFile zip = new ZipFile())
                    {
                        foreach (ContentDescriptor cd in datasetVersion.ContentDescriptors)
                        {
                            bool addFile = true;

                            if (cd.Name.ToLower().Contains("generated"))
                            {
                                if (!cd.MimeType.ToLower().Equals(format)) addFile = false;
                            }

                            if (addFile)
                            {
                                string path = Path.Combine(AppConfiguration.DataPath, cd.URI);
                                string name = cd.URI.Split('\\').Last();

                                if (FileHelper.FileExist(path))
                                {
                                    if (!zip.Any(entry => entry.FileName.EndsWith(name)))
                                        zip.AddFile(path, "");
                                }
                            }
                        }

                        // add xsd of the metadata schema
                        LoggerFactory.LogCustom("Schema Start");

                        string xsdDirectoryPath = OutputMetadataManager.GetSchemaDirectoryPath(id);
                        if (Directory.Exists(xsdDirectoryPath))
                            zip.AddDirectory(xsdDirectoryPath, "Schema");

                        LoggerFactory.LogCustom("Manifest Start");

                        XmlDocument manifest = OutputDatasetManager.GenerateManifest(id, datasetVersion.Id);

                        if (manifest != null)
                        {
                            string dynamicManifestFilePath = OutputDatasetManager.GetDynamicDatasetStorePath(id,
                                versionNr, "manifest", ".xml");
                            string fullFilePath = Path.Combine(AppConfiguration.DataPath, dynamicManifestFilePath);

                            manifest.Save(fullFilePath);
                            zip.AddFile(fullFilePath, "");
                        }

                        LoggerFactory.LogCustom("Save zip Start");

                        zip.Save(zipFilePath);

                        LoggerFactory.LogCustom("Return");

                        string title = datasetVersion.Title;
                        title = String.IsNullOrEmpty(title) ? "unknown" : title;

                        string message = string.Format("dataset {0} version {1} was downloaded as zip - {2}.", id,
                        versionNr, format);
                        LoggerFactory.LogCustom(message);

                        var es = new EmailService();
                        es.Send(MessageHelper.GetDownloadDatasetHeader(id, versionNr),
                            MessageHelper.GetDownloadDatasetMessage(id, title, getPartyNameOrDefault(), "zip - " + format, versionNr),
                            GeneralSettings.SystemEmail
                            );

                        return File(zipFilePath, "application/zip", Path.GetFileName(zipFilePath));
                    }
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

        private void generateMetadataHtml(DatasetVersion dsv)
        {
            XmlDatasetHelper xmlDatasetHelper = new XmlDatasetHelper();
            long datasetId = dsv.Dataset.Id;
            long metadatastructureId = dsv.Dataset.MetadataStructure.Id;
            long datastructureId = dsv.Dataset.DataStructure==null?0: dsv.Dataset.DataStructure.Id;
            long researchplanId = dsv.Dataset.ResearchPlan.Id;

            string title = dsv.Title;
            Session["ShowDataMetadata"] = dsv.Metadata;

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
    }
}