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
    public class PublishingManager
    {
        private const string sourceFile = "publishingRequirements.xml";
        private XmlDocument requirementXmlDocument = null;

        public List<DataRepository> DataRepositories;

        public PublishingManager()
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

            return tmp;
        }


    }
}
