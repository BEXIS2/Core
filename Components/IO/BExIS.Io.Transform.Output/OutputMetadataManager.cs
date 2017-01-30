using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using BExIS.Xml.Services;
using BExIS.Xml.Helpers.Mapping;
using Vaiona.Utils.Cfg;
using System.IO;
using System.Net;
using BExIS.Dlm.Entities.Data;
using BExIS.Dlm.Entities.MetadataStructure;
using BExIS.Dlm.Services.Data;
using BExIS.Dlm.Services.MetadataStructure;
using BExIS.Xml.Helpers;

namespace BExIS.IO.Transform.Output
{
    public class OutputMetadataManager
    {
        public static string IsValideAgainstSchema(long datasetId, TransmissionType type, string mappingName)
        {
            try
            {
                DatasetManager datasetManager = new DatasetManager();
                DatasetVersion datasetVersion = datasetManager.GetDatasetLatestVersion(datasetId);

                string mappingFileName = XmlDatasetHelper.GetTransmissionInformation(datasetVersion, type, mappingName);
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

                string mappingFileName = XmlDatasetHelper.GetTransmissionInformation(datasetVersion, type, mappingName);
                string pathMappingFile = Path.Combine(AppConfiguration.GetModuleWorkspacePath("DIM"), mappingFileName);

                XmlMapperManager xmlMapperManager = new XmlMapperManager(TransactionDirection.InternToExtern);
                xmlMapperManager.Load(pathMappingFile, "exporttest");

                newXml = xmlMapperManager.Export(datasetVersion.Metadata, datasetVersion.Id, mappingName, true);

                string title = XmlDatasetHelper.GetInformation(datasetVersion, NameAttributeValues.title);

                // store in content descriptor
                if (storing)
                {
                    if(String.IsNullOrEmpty(mappingName) || mappingName.ToLower() == "generic")
                        storeGeneratedFilePathToContentDiscriptor(datasetId, datasetVersion, "metadata", ".xml");
                    else
                        storeGeneratedFilePathToContentDiscriptor(datasetId, datasetVersion, "metadata_"+ mappingName, ".xml");

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

                if(!String.IsNullOrEmpty(path) && Directory.Exists(path))
                    return path;
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return String.Empty;
        }

        public static string GetSchemaDirectoryPathFromMetadataStructure(long metadataStructureId)
        {
            try
            {
                MetadataStructureManager metadataStructureManager = new MetadataStructureManager();
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
