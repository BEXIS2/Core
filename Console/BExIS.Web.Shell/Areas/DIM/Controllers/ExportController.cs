using BExIS.Dim.Entities.Publication;
using BExIS.Dim.Helpers;
using BExIS.Dim.Helpers.Export;
using BExIS.Dim.Services;
using BExIS.Dlm.Entities.Data;
using BExIS.Dlm.Entities.DataStructure;
using BExIS.Dlm.Entities.MetadataStructure;
using BExIS.Dlm.Services.Data;
using BExIS.Dlm.Services.DataStructure;
using BExIS.IO;
using BExIS.IO.Transform.Output;
using BExIS.Modules.Dim.UI.Models.Export;
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
        /// <param name="metadataFormat">name of the internal metadatastructure, if empty then </param>
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
                dataRepo.Broker = broker;

                GenericDataRepoConverter dataRepoConverter = new GenericDataRepoConverter(dataRepo);
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

        public ActionResult GenerateZip(long id, string format)
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
                    long dsId = dm.GetDatasetLatestVersion(id).Id;
                    DatasetVersion datasetVersion = uow.GetUnitOfWork().GetReadOnlyRepository<DatasetVersion>().Get(dsId);

                    #region metadata

                    //metadata as xml output
                    XmlDocument document = OutputMetadataManager.GetConvertedMetadata(id, TransmissionType.mappingFileExport,
                             datasetVersion.Dataset.MetadataStructure.Name);

                    //metadata as html
                    generateMetadataHtml(datasetVersion);

                    #endregion metadata

                    #region primary data

                    // check the data sturcture type ...
                    if (format != null && datasetVersion.Dataset.DataStructure.Self is StructuredDataStructure)
                    {
                        OutputDataManager odm = new OutputDataManager();
                        // apply selection and projection

                        string title = xmlDatasetHelper.GetInformation(id, NameAttributeValues.title);

                        switch (format)
                        {
                            case "application/xlsx":
                                odm.GenerateExcelFile(id, title, false);
                                break;

                            case "application/xlsm":
                                odm.GenerateExcelFile(id, title, true);
                                break;

                            default:
                                odm.GenerateAsciiFile(id, title, format, false);
                                break;
                        }
                    }

                    #endregion primary data

                    string zipName = publishingManager.GetZipFileName(id, datasetVersion.Id);
                    string zipPath = publishingManager.GetDirectoryPath(id, brokerName);
                    string dynamicZipPath = publishingManager.GetDynamicDirectoryPath(id, brokerName);
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

                    long dataStructureId = datasetVersion.Dataset.DataStructure.Id;
                    DataStructure dataStructure = dataStructureManager.StructuredDataStructureRepo.Get(dataStructureId);

                    if (dataStructure != null)
                    {
                        try
                        {
                            string dynamicPathOfDS = "";
                            dynamicPathOfDS = storeGeneratedFilePathToContentDiscriptor(id, datasetVersion,
                                "datastructure", ".txt");
                            string datastructureFilePath = AsciiWriter.CreateFile(dynamicPathOfDS);

                            string json = OutputDataStructureManager.GetVariableListAsJson(dataStructureId);

                            AsciiWriter.AllTextToFile(datastructureFilePath, json);
                        }
                        catch (Exception ex)
                        {
                            throw ex;
                        }

                        //generate datastructure as html
                        try
                        {
                            DatasetVersion ds = uow.GetUnitOfWork().GetReadOnlyRepository<DatasetVersion>().Get(dsId);
                            generateDataStructureHtml(ds);
                        }
                        catch (Exception ex)
                        {
                            throw ex;
                        }
                    }

                    #endregion datatructure

                    ZipFile zip = new ZipFile();

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
                                zip.AddFile(path, "");
                            }
                        }
                    }

                    // add xsd of the metadata schema
                    string xsdDirectoryPath = OutputMetadataManager.GetSchemaDirectoryPath(id);
                    if (Directory.Exists(xsdDirectoryPath))
                        zip.AddDirectory(xsdDirectoryPath, "Schema");

                    XmlDocument manifest = OutputDatasetManager.GenerateManifest(id, datasetVersion.Id);

                    if (manifest != null)
                    {
                        string dynamicManifestFilePath = OutputDatasetManager.GetDynamicDatasetStorePath(id,
                            datasetVersion.Id, "manifest", ".xml");
                        string fullFilePath = Path.Combine(AppConfiguration.DataPath, dynamicManifestFilePath);

                        manifest.Save(fullFilePath);
                        zip.AddFile(fullFilePath, "");
                    }

                    zip.Save(zipFilePath);

                    return File(zipFilePath, "application/zip", Path.GetFileName(zipFilePath));
                }
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

            // create the generated FileStream and determine its location
            string dynamicPath = OutputDatasetManager.GetDynamicDatasetStorePath(datasetId, datasetVersion.Id, title,
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

            DatasetManager dm = new DatasetManager();
            if (datasetVersion.ContentDescriptors.Count(p => p.Name.Equals(name)) > 0)
            {
                // remove the one contentdesciptor
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

        private void generateMetadataHtml(DatasetVersion dsv)
        {
            XmlDatasetHelper xmlDatasetHelper = new XmlDatasetHelper();
            long datasetId = dsv.Dataset.Id;
            long metadatastructureId = dsv.Dataset.MetadataStructure.Id;
            long datastructureId = dsv.Dataset.DataStructure.Id;
            long researchplanId = dsv.Dataset.ResearchPlan.Id;

            string title = xmlDatasetHelper.GetInformation(dsv.Dataset.Id, NameAttributeValues.title);
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
    }
}