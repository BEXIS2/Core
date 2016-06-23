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
using BExIS.Dlm.Entities.Data;
using BExIS.Dlm.Services.Data;

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

                string mappingFileName = XmlDatasetHelper.GetExportInformation(datasetVersion, type, mappingName);
                string pathMappingFile = Path.Combine(AppConfiguration.GetModuleWorkspacePath("DIM"), mappingFileName);

                XmlMapperManager xmlMapperManager = new XmlMapperManager();
                xmlMapperManager.Load(pathMappingFile, "exporttest");

                newXml = xmlMapperManager.Export(datasetVersion.Metadata, datasetVersion.Id, true);
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return newXml;
        }
    }
}
