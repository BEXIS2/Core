using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using BExIS.Dim.Entities;
using BExIS.IO;
using BExIS.Xml.Helpers;
using Vaiona.Utils.Cfg;

namespace BExIS.Dim.Helpers
{
    public class SubmissionManager
    {
        private const string sourceFile = "submissionConfig.xml";
        private XmlDocument requirementXmlDocument = null;

        public List<DataRepository> DataRepositories;

        public SubmissionManager()
        {
            requirementXmlDocument = new XmlDocument();
            DataRepositories = new List<DataRepository>();
        }

        public void Load()
        {
            string filepath = Path.Combine(AppConfiguration.GetModuleWorkspacePath("DIM"),sourceFile);

            if (FileHelper.FileExist(filepath))
            {
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.Load(filepath);
                requirementXmlDocument = xmlDoc;
                XmlNodeList dataRepositoryNodes = requirementXmlDocument.GetElementsByTagName("datarepository");

                foreach (XmlNode child in dataRepositoryNodes)
                {
                    DataRepositories.Add(createDataRepository(child));
                }
            }
        }

        private DataRepository createDataRepository(XmlNode node)
        {
            DataRepository tmp = new DataRepository();
            tmp.Name = XmlUtility.GetXmlNodeByName(node, "name").InnerText;
            tmp.ReqiuredMetadataStandard = XmlUtility.GetXmlNodeByName(node, "metadatastandard").InnerText;
            tmp.PrimaryDataFormat = XmlUtility.GetXmlNodeByName(node, "primarydataformat").InnerText;
            tmp.Server = XmlUtility.GetXmlNodeByName(node, "server").InnerText;
            tmp.User = XmlUtility.GetXmlNodeByName(node, "user").InnerText;
            tmp.Password = XmlUtility.GetXmlNodeByName(node, "password").InnerText;

            return tmp;
        }

        #region helper function

        public string GetDirectoryPath(long datasetid, DataRepository dataRepository)
        {
            return Path.Combine(AppConfiguration.DataPath, "Datasets", datasetid.ToString(), "publish", dataRepository.Name);
        }

        public string GetZipFileName(long datasetid, long datasetVersionid)
        {
            return datasetid + "_" + datasetVersionid + "_Dataset.zip";
        }

        public bool Exist(long datasetid, long datasetVersionid, DataRepository dataRepository)
        {
            // check directory
            string d = GetDirectoryPath(datasetid, dataRepository);
            if (!Directory.Exists(d)) return false;

            //check file
            string file = GetZipFileName(datasetid, datasetVersionid);
            string filePath = Path.Combine(d, file);

            if (File.Exists(filePath)) return true;

            return false;
        }
        
        #endregion



    }
}
