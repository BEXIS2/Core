using BExIS.Dim.Entities.Publications;
using BExIS.Dim.Services;
using BExIS.Dlm.Entities.Data;
using BExIS.Dlm.Entities.DataStructure;
using BExIS.Dlm.Services.Data;
using BExIS.Dlm.Services.DataStructure;
using BExIS.IO;
using BExIS.IO.Transform.Output;
using BExIS.Utils.Extensions;
using BExIS.Xml.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Xml;
using Vaiona.Logging;
using Vaiona.Utils.Cfg;

namespace BExIS.Dim.Helpers.Export
{
    public class GenericDataRepoConverter : IDataRepoConverter
    {
        private XmlDatasetHelper xmlDatasetHelper = new XmlDatasetHelper();
        private Repository _dataRepo { get; set; }
        private Broker _broker { get; set; }

        //ToDO -> David <- do not store the files on the server
        public string Convert(long datasetVersionId)
        {
            string datarepo = _dataRepo.Name;
            SubmissionManager publishingManager = new SubmissionManager();

            using (DatasetManager datasetManager = new DatasetManager())
            using (DataStructureManager dataStructureManager = new DataStructureManager())
            using (PublicationManager publicationManager = new PublicationManager())
            {
                DatasetVersion datasetVersion = datasetManager.GetDatasetVersion(datasetVersionId);
                long datasetId = datasetVersion.Dataset.Id;

                Publication publication =
                    publicationManager.GetPublication()
                        .Where(
                            p =>
                                p.DatasetVersion.Id.Equals(datasetVersion.Id) &&
                                p.Broker.Name.ToLower().Equals(_broker.Name))
                        .FirstOrDefault();

                // if(broker exist)
                if (publication == null && _broker != null)
                {
                    Broker broker = _broker;

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

                            odm.GenerateAsciiFile(datasetId, datasetVersion.Id, broker.PrimaryDataFormat, false);
                        }

                        int versionNr = datasetManager.GetDatasetVersionNr(datasetVersion);

                        string zipName = publishingManager.GetZipFileName(datasetId, versionNr);
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

                        long dataStructureId = datasetVersion.Dataset.DataStructure.Id;
                        DataStructure dataStructure = dataStructureManager.StructuredDataStructureRepo.Get(dataStructureId);

                        if (dataStructure != null)
                        {
                            try
                            {
                                string dynamicPathOfDS = "";
                                dynamicPathOfDS = storeGeneratedFilePathToContentDiscriptor(datasetId, datasetVersion,
                                    "datastructure", ".txt");
                                string datastructureFilePath = AsciiWriter.CreateFile(dynamicPathOfDS);

                                string json = OutputDataStructureManager.GetDataStructureAsJson(dataStructureId);

                                AsciiWriter.AllTextToFile(datastructureFilePath, json);
                            }
                            catch (Exception ex)
                            {
                                throw ex;
                            }
                        }

                        #endregion datatructure

                        using (var zipFileStream = new FileStream(zipFilePath, FileMode.Create))
                        using (var archive = new ZipArchive(zipFileStream, ZipArchiveMode.Update))
                        {
                            foreach (ContentDescriptor cd in datasetVersion.ContentDescriptors)
                            {
                                string path = Path.Combine(AppConfiguration.DataPath, cd.URI);
                                string name = cd.URI.Split('\\').Last();

                                if (FileHelper.FileExist(path))
                                {
                                    archive.AddFileToArchive(path, "");
                                }
                            }

                            // add xsd of the metadata schema
                            string xsdDirectoryPath = OutputMetadataManager.GetSchemaDirectoryPath(datasetId);
                            if (Directory.Exists(xsdDirectoryPath))
                                archive.AddFolderToArchive(xsdDirectoryPath, "Schema");

                            XmlDocument manifest = OutputDatasetManager.GenerateManifest(datasetId, datasetVersion.Id);

                            if (manifest != null)
                            {
                                string dynamicManifestFilePath = OutputDatasetManager.GetDynamicDatasetStorePath(datasetId,
                                    versionNr, "manifest", ".xml");
                                string fullFilePath = Path.Combine(AppConfiguration.DataPath, dynamicManifestFilePath);

                                manifest.Save(fullFilePath);
                                archive.AddFileToArchive(fullFilePath, "");
                            }

                            string message = string.Format("dataset {0} version {1} was published for repository {2}", datasetId,
                                datasetVersion.Id, broker.Name);
                            LoggerFactory.LogCustom(message);
                        }

                        return zipFilePath;
                    }
                }

                return "";
            }
        }

        public bool Validate(long datasetVersionId, out List<string> errors)
        {
            errors = new List<string>();
            return true; //throw new NotImplementedException();
        }

        private static string storeGeneratedFilePathToContentDiscriptor(long datasetId, DatasetVersion datasetVersion,
            string title, string ext)
        {
            string name = "";
            string mimeType = "";
            using (DatasetManager dm = new DatasetManager())
            {
                if (ext.Contains("csv"))
                {
                    name = "datastructure";
                    mimeType = "text/comma-separated-values";
                }

                var versionNr = dm.GetDatasetVersionNr(datasetVersion);

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
        }

        public GenericDataRepoConverter(Broker broker)
        {
            _dataRepo = broker.Repository;
            _broker = broker;
        }
    }
}