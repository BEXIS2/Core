using BExIS.Dim.Entities.Publication;
using BExIS.Dim.Services;
using BExIS.IO;
using BExIS.Xml.Helpers;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using Vaiona.Utils.Cfg;

namespace BExIS.Dim.Helpers
{
    public class SubmissionManager
    {
        private const string sourceFile = "submissionConfig.xml";
        private XmlDocument requirementXmlDocument = null;


        public List<Broker> Brokers;

        public SubmissionManager()
        {
            requirementXmlDocument = new XmlDocument();
            Brokers = new List<Broker>();
        }

        public void Load()
        {
            PublicationManager publicationManager = new PublicationManager();

            try
            {

                string filepath = Path.Combine(AppConfiguration.GetModuleWorkspacePath("DIM"), sourceFile);

                if (FileHelper.FileExist(filepath))
                {
                    XmlDocument xmlDoc = new XmlDocument();
                    xmlDoc.Load(filepath);
                    requirementXmlDocument = xmlDoc;
                    XmlNodeList brokerNodes = requirementXmlDocument.GetElementsByTagName("broker");

                    foreach (XmlNode child in brokerNodes)
                    {
                        Brokers.Add(createBroker(child, publicationManager));
                    }
                }
            }
            finally
            {
                publicationManager.Dispose();
            }
        }

        private Broker createBroker(XmlNode node, PublicationManager publicationManager)
        {
            Broker tmp = new Broker();


            //create broker in DB
            string brokerName = XmlUtility.GetXmlNodeByName(node, "name").InnerText;

            if (publicationManager.BrokerRepo.Query().Any(b => b.Name.Equals(brokerName)))
            {
                tmp = publicationManager.BrokerRepo.Query().Where(b => b.Name.Equals(brokerName)).FirstOrDefault();
            }
            else
            {
                tmp.Name = XmlUtility.GetXmlNodeByName(node, "name").InnerText;
                tmp.MetadataFormat = XmlUtility.GetXmlNodeByName(node, "metadatastandard").InnerText;
                tmp.PrimaryDataFormat = XmlUtility.GetXmlNodeByName(node, "primarydataformat").InnerText;
                tmp.Server = XmlUtility.GetXmlNodeByName(node, "server").InnerText;
                tmp.UserName = XmlUtility.GetXmlNodeByName(node, "user").InnerText;
                tmp.Password = XmlUtility.GetXmlNodeByName(node, "password").InnerText;
                tmp = publicationManager.CreateBroker(tmp);
            }



            XmlNode dataRepos = XmlUtility.GetXmlNodeByName(node, "dataRepos");

            foreach (XmlNode dataRepo in dataRepos.ChildNodes)
            {

                Repository repo = null;
                string repoName = XmlUtility.GetXmlNodeByName(dataRepo, "name").InnerText;

                if (publicationManager.RepositoryRepo.Query().Any(b => b.Name.Equals(repoName)))
                {
                    repo = publicationManager.RepositoryRepo.Get().Where(b => b.Name.Equals(repoName)).FirstOrDefault();
                }
                else
                {
                    repo = createRepository(dataRepo as XmlNode);
                }

                if (repo != null) publicationManager.CreateRepository(repo.Name, repo.Url, tmp);
            }


            return tmp;
        }

        private Repository createRepository(XmlNode node)
        {
            if (node != null)
            {
                Repository tmp = new Repository();
                tmp.Name = XmlUtility.GetXmlNodeByName(node, "name").InnerText;
                tmp.Url = XmlUtility.GetXmlNodeByName(node, "url").InnerText;

                return tmp;
            }

            return null;
        }

        #region helper function

        public string GetDirectoryPath(long datasetid, string dataRepositoryName)
        {
            return Path.Combine(AppConfiguration.DataPath, "Datasets", datasetid.ToString(), "publish", dataRepositoryName);
        }

        public string GetDynamicDirectoryPath(long datasetid, string dataRepositoryName)
        {
            return Path.Combine("Datasets", datasetid.ToString(), "publish", dataRepositoryName);
        }

        public string GetZipFileName(long datasetid, long datasetVersionid)
        {
            return datasetid + "_" + datasetVersionid + "_Dataset.zip";
        }

        public string GetFileNameForDataRepo(long datasetid, long datasetVersionid, string datarepo, string ext)
        {
            return datasetid + "_" + datasetVersionid + "_Dataset_" + datarepo + "." + ext;
        }

        public bool Exist(long datasetid, long datasetVersionid, string dataRepositoryName)
        {
            // check directory
            string d = GetDirectoryPath(datasetid, dataRepositoryName);
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
