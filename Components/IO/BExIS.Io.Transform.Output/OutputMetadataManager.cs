using BExIS.Dlm.Entities.Data;
using BExIS.Dlm.Entities.MetadataStructure;
using BExIS.Dlm.Services.Data;
using BExIS.Dlm.Services.MetadataStructure;
using BExIS.Xml.Helpers;
using BExIS.Xml.Helpers.Mapping;
using System;
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
            try
            {
                XmlDatasetHelper xmlDatasetHelper = new XmlDatasetHelper();

                DatasetManager datasetManager = new DatasetManager();
                DatasetVersion datasetVersion = datasetManager.GetDatasetLatestVersion(datasetId);

                string mappingFileName = xmlDatasetHelper.GetTransmissionInformation(datasetVersion.Id, type, mappingName);
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

        }

        public static XmlDocument GetConvertedMetadata(long datasetId, TransmissionType type, string mappingName, bool storing = true)
        {
            XmlDocument newXml;
            try
            {
                DatasetManager datasetManager = new DatasetManager();
                DatasetVersion datasetVersion = datasetManager.GetDatasetLatestVersion(datasetId);
                XmlDatasetHelper xmlDatasetHelper = new XmlDatasetHelper();

                string mappingFileName = xmlDatasetHelper.GetTransmissionInformation(datasetVersion.Id, type, mappingName);
                string pathMappingFile = Path.Combine(AppConfiguration.GetModuleWorkspacePath("DIM"), mappingFileName);

                XmlMapperManager xmlMapperManager = new XmlMapperManager(TransactionDirection.InternToExtern);
                xmlMapperManager.Load(pathMappingFile, "exporttest");

                newXml = xmlMapperManager.Export(datasetVersion.Metadata, datasetVersion.Id, mappingName, true);

                string title = xmlDatasetHelper.GetInformationFromVersion(datasetVersion.Id, NameAttributeValues.title);

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

        public static string GetSchemaDirectoryPath(long datasetId)
        {
            try
            {
                DatasetManager datasetManager = new DatasetManager();
                DatasetVersion datasetVersion = datasetManager.GetDatasetLatestVersion(datasetId);

                MetadataStructureManager metadataStructureManager = new MetadataStructureManager();
                MetadataStructure metadataStructure =
                    metadataStructureManager.Repo.Get(datasetVersion.Dataset.MetadataStructure.Id);

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
        public static string GetMetadataPath(long datasetVersionId)
        {
            string path = "";

            DatasetManager datasetManager = new DatasetManager();
            ContentDescriptor contentDescriptor = datasetManager.GetDatasetVersion(datasetVersionId).
                ContentDescriptors.ToList().FirstOrDefault(c => c.Name.Equals("metadata"));

            if (contentDescriptor != null)
            {
                path = Path.Combine(AppConfiguration.DataPath, contentDescriptor.URI);

                if (FileHelper.FileExist(path))
                    return path;
            }

            return "";
        }

        public static string CreateConvertedMetadata(long datasetId, TransmissionType type)
        {
            XmlDocument newXml;
            try
            {


                DatasetManager datasetManager = new DatasetManager();
                MetadataStructureManager metadataMetadataStructureManager = new MetadataStructureManager();

                DatasetVersion datasetVersion = datasetManager.GetDatasetLatestVersion(datasetId);
                MetadataStructure metadataStructure = metadataMetadataStructureManager.Repo.Get(datasetVersion.Dataset.MetadataStructure.Id);
                XmlDatasetHelper xmlDatasetHelper = new XmlDatasetHelper();

                string mappingName = metadataStructure.Name;

                string mappingFileName = xmlDatasetHelper.GetTransmissionInformation(datasetVersion.Id, type, mappingName);
                string pathMappingFile = Path.Combine(AppConfiguration.GetModuleWorkspacePath("DIM"), mappingFileName);

                XmlMapperManager xmlMapperManager = new XmlMapperManager(TransactionDirection.InternToExtern);
                xmlMapperManager.Load(pathMappingFile, "exporttest");

                newXml = xmlMapperManager.Export(datasetVersion.Metadata, datasetVersion.Id, mappingName, true);

                string title = xmlDatasetHelper.GetInformationFromVersion(datasetVersion.Id, NameAttributeValues.title);

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

                return OutputDatasetManager.GetDynamicDatasetStorePath(datasetId, datasetVersion.Id, filename, ".xml");

            }
            catch (Exception ex)
            {
                throw ex;
            }

            return "";
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

        }
    }
}
