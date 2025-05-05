using BExIS.Dlm.Entities.Data;
using BExIS.Dlm.Entities.MetadataStructure;
using BExIS.Dlm.Services.Data;
using BExIS.Dlm.Services.MetadataStructure;
using BExIS.Xml.Helpers;
using BExIS.Xml.Helpers.Mapping;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using Vaiona.Utils.Cfg;

namespace BExIS.IO.Transform.Output
{
    public class OutputMetadataManager
    {
        public static string IsValideAgainstSchema(long datasetId, TransmissionType type, string mappingName)
        {
            DatasetManager datasetManager = new DatasetManager();

            try
            {
                XmlDatasetHelper xmlDatasetHelper = new XmlDatasetHelper();

                DatasetVersion datasetVersion = datasetManager.GetDatasetLatestVersion(datasetId);

                string mappingFileName = xmlDatasetHelper.GetTransmissionInformation(datasetVersion.Id, type, mappingName);
                // if mapping file not exists
                if (string.IsNullOrEmpty(mappingFileName)) return "";
                string pathMappingFile = Path.Combine(AppConfiguration.GetModuleWorkspacePath("DIM"), mappingFileName);

                XmlMapperManager xmlMapperManager = new XmlMapperManager(TransactionDirection.InternToExtern);
                xmlMapperManager.Load(pathMappingFile, "exporttest");

                XmlDocument tmp = GetConvertedMetadata(datasetId, type, mappingName, false);

                string path = Path.Combine(AppConfiguration.DataPath, "Temp", "System", "convertedMetadata.xml");

                if (FileHelper.FileExist(path))
                    FileHelper.Delete(path);

                FileHelper.CreateDicrectoriesIfNotExist(Path.GetDirectoryName(path));

                tmp.Save(path);
                XmlDocument metadataForImport = new XmlDocument();
                metadataForImport.Load(path);

                return xmlMapperManager.Validate(metadataForImport);
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
            finally
            {
                datasetManager.Dispose();
            }
        }

        public static XmlDocument GetConvertedMetadata(long datasetId, TransmissionType type, string mappingName, bool storing = true)
        {
            using (DatasetManager datasetManager = new DatasetManager())
            {
                XmlDocument newXml;

                try
                {
                    DatasetVersion datasetVersion = datasetManager.GetDatasetLatestVersion(datasetId);
                    XmlDatasetHelper xmlDatasetHelper = new XmlDatasetHelper();

                    // if no mapping name  is provided, use the metadata structure name
                    if(string.IsNullOrEmpty(mappingName)) mappingName = datasetVersion.Dataset.MetadataStructure.Name;

                    string mappingFileName = xmlDatasetHelper.GetTransmissionInformation(datasetVersion.Id, type, mappingName);
                    // no mapping files with mappingName exist
                    if (string.IsNullOrEmpty(mappingFileName)) return null;

                    string pathMappingFile = Path.Combine(AppConfiguration.GetModuleWorkspacePath("DIM"), mappingFileName);

                    XmlMapperManager xmlMapperManager = new XmlMapperManager(TransactionDirection.InternToExtern);
                    xmlMapperManager.Load(pathMappingFile, "exporttest");

                    newXml = xmlMapperManager.Export(datasetVersion.Metadata, datasetVersion.Id, mappingName, true);

                    string title = datasetVersion.Title;

                    // store in content descriptor
                    if (storing)
                    {
                        if (String.IsNullOrEmpty(mappingName) || mappingName.ToLower() == "generic")
                            storeGeneratedFilePathToContentDiscriptor(datasetId, datasetVersion, "metadata", ".xml");
                        else
                            storeGeneratedFilePathToContentDiscriptor(datasetId, datasetVersion, "metadata_" + mappingName, ".xml");
                    }
                }
                catch (Exception ex)
                {
                    throw ex;
                }

                return newXml;
            }
        }

        public static string GetSchemaDirectoryPath(long datasetId)
        {
            using (DatasetManager datasetManager = new DatasetManager())
            using (MetadataStructureManager metadataStructureManager = new MetadataStructureManager())
            {
                try
                {
                    DatasetVersion datasetVersion = datasetManager.GetDatasetLatestVersion(datasetId);

                    MetadataStructure metadataStructure = metadataStructureManager.Repo.Get(datasetVersion.Dataset.MetadataStructure.Id);

                    string path = Path.Combine(AppConfiguration.GetModuleWorkspacePath("DCM"), "Metadata",
                        metadataStructure.Name);

                    if (!String.IsNullOrEmpty(path) && Directory.Exists(path))
                        return path;
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }

            return String.Empty;
        }

        public static string GetSchemaDirectoryPathFromMetadataStructure(long metadataStructureId, MetadataStructureManager metadataStructureManager)
        {
            try
            {
                MetadataStructure metadataStructure =
                    metadataStructureManager.Repo.Get(metadataStructureId);

                string path = Path.Combine(AppConfiguration.GetModuleWorkspacePath("DCM"), "Metadata",
                    metadataStructure.Name);

                if (!String.IsNullOrEmpty(path) && Directory.Exists(path))
                    return path;
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return String.Empty;
        }

        /// <summary>
        /// Returns the full path for a xmlmetadata xml is exist
        /// </summary>
        /// <param name="datasetVersionId"></param>
        /// <returns></returns>
        public static string GetMetadataPath(ICollection<ContentDescriptor> contentDescriptors)
        {
            using (DatasetManager datasetManager = new DatasetManager())
            {
                string path = "";

                ContentDescriptor contentDescriptor = contentDescriptors.ToList().FirstOrDefault(c => c.Name.Equals("metadata"));

                if (contentDescriptor != null)
                {
                    path = Path.Combine(AppConfiguration.DataPath, contentDescriptor.URI);

                    if (FileHelper.FileExist(path))
                        return path;
                }

                return "";
            }
        }

        public static string CreateConvertedMetadata(long datasetId, TransmissionType type)
        {
            XmlDocument newXml;
            DatasetManager datasetManager = new DatasetManager();
            MetadataStructureManager metadataMetadataStructureManager = new MetadataStructureManager();

            try
            {
                DatasetVersion datasetVersion = datasetManager.GetDatasetLatestVersion(datasetId);
                MetadataStructure metadataStructure = metadataMetadataStructureManager.Repo.Get(datasetVersion.Dataset.MetadataStructure.Id);
                XmlDatasetHelper xmlDatasetHelper = new XmlDatasetHelper();

                int versionNr = datasetManager.GetDatasetVersionNr(datasetVersion);
                string mappingName = metadataStructure.Name;

                string mappingFileName = xmlDatasetHelper.GetTransmissionInformation(datasetVersion.Id, type, mappingName);
                //if mapping file not exist
                if (string.IsNullOrEmpty(mappingFileName)) return "";
                string pathMappingFile = Path.Combine(AppConfiguration.GetModuleWorkspacePath("DIM"), mappingFileName);

                XmlMapperManager xmlMapperManager = new XmlMapperManager(TransactionDirection.InternToExtern);
                xmlMapperManager.Load(pathMappingFile, "exporttest");

                newXml = xmlMapperManager.Export(datasetVersion.Metadata, datasetVersion.Id, mappingName, true);

                string title = datasetVersion.Title;

                // store in content descriptor
                string filename = "metadata";
                if (String.IsNullOrEmpty(mappingName) || mappingName.ToLower() == "generic")
                {
                    storeGeneratedFilePathToContentDiscriptor(datasetId, datasetVersion, filename, ".xml");
                }
                else
                {
                    filename = "metadata_" + mappingName;
                    storeGeneratedFilePathToContentDiscriptor(datasetId, datasetVersion, filename,
                        ".xml");
                }

                return OutputDatasetManager.GetDynamicDatasetStorePath(datasetId, versionNr, filename, ".xml");
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                datasetManager.Dispose();
                metadataMetadataStructureManager.Dispose();
            }
        }

        private static void storeGeneratedFilePathToContentDiscriptor(long datasetId, DatasetVersion datasetVersion, string title, string ext)
        {
            string name = "";
            string mimeType = "";

            if (ext.Contains("xml"))
            {
                name = "metadata";
                mimeType = "text/xml";
            }

            DatasetManager dm = new DatasetManager();
            int versionNr = dm.GetDatasetVersionNr(datasetVersion);

            // create the generated FileStream and determine its location
            string dynamicPath = OutputDatasetManager.GetDynamicDatasetStorePath(datasetId, versionNr, title, ext);
            //Register the generated data FileStream as a resource of the current dataset version
            //ContentDescriptor generatedDescriptor = new ContentDescriptor()
            //{
            //    OrderNo = 1,
            //    Name = name,
            //    MimeType = mimeType,
            //    URI = dynamicPath,
            //    DatasetVersion = datasetVersion,
            //};

            try
            {
                datasetVersion = dm.GetDatasetVersion(datasetVersion.Id);

                if (datasetVersion.ContentDescriptors.Count(p => p.Name.Equals(name) && p.MimeType.Equals(mimeType)) > 0)
                {   // remove the one contentdesciptor
                    foreach (ContentDescriptor cd in datasetVersion.ContentDescriptors)
                    {
                        if (cd.Name == name && cd.MimeType.Equals(mimeType))
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
            }
            finally
            {
                dm.Dispose();
            }
        }
    }
}