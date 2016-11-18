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
        public static XmlDocument GetConvertedMetadata(long datasetId, TransmissionType type, string mappingName)
        {
            XmlDocument newXml;
            try
            {
                DatasetManager datasetManager = new DatasetManager();
                DatasetVersion datasetVersion = datasetManager.GetDatasetLatestVersion(datasetId);

                string mappingFileName = XmlDatasetHelper.GetTransmissionInformation(datasetVersion, type, mappingName);
                string pathMappingFile = Path.Combine(AppConfiguration.GetModuleWorkspacePath("DIM"), mappingFileName);

                XmlMapperManager xmlMapperManager = new XmlMapperManager();
                xmlMapperManager.Load(pathMappingFile, "exporttest");

                newXml = xmlMapperManager.Export(datasetVersion.Metadata, datasetVersion.Id, mappingName, true);

                string title = XmlDatasetHelper.GetInformation(datasetVersion, NameAttributeValues.title);

                // store in content descriptor
                storeGeneratedFilePathToContentDiscriptor(datasetId, datasetVersion, "metadata", ".xml");

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
