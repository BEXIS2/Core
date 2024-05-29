

using BExIS.Dim.Entities.Publications;
using BExIS.Dim.Services;
using BExIS.IO;
using BExIS.Xml.Helpers;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using Vaiona.Utils.Cfg;

namespace BExIS.Dim.Helpers
{
    public class SubmissionManager
    {
        private const string sourceFile = "submissionConfig.json";
 

        public List<Broker> Brokers;

        public SubmissionManager()
        {
            Brokers = new List<Broker>();
        }

        public void Load()
        {
            using (PublicationManager publicationManager = new PublicationManager())
            {
                string filepath = Path.Combine(AppConfiguration.GetModuleWorkspacePath("DIM"), sourceFile);

                if (FileHelper.FileExist(filepath))
                {

                    // Read the file contents as a string
                    string jsonString = File.ReadAllText(filepath);

                    // Deserialize the JSON into your C# class object
                    List<Broker> Brokers = JsonConvert.DeserializeObject<List<Broker>>(jsonString); // Deserialize the JSON into a Person object

                    for (int i = 0; i < Brokers.Count(); i++)
                    {
                        createBroker(Brokers.ElementAt(i), publicationManager);
                    }

                }
            }

        }

        private Broker createBroker(Broker broker, PublicationManager publicationManager)
        {
            Repository repo = broker.Repository;

            if (publicationManager.RepositoryRepo.Query().Any(b => b.Name.Equals(repo.Name)))
            {
                repo = publicationManager.RepositoryRepo.Get().Where(b => b.Name.Equals(repo.Name)).FirstOrDefault();
            }
            else
            {
                if (repo != null) repo = publicationManager.CreateRepository(repo.Name, repo.Url);
            }


            //create broker in DB
            string brokerName = broker.Name;

            if (publicationManager.BrokerRepo.Query().Any(b => b.Name.Equals(broker.Name) && b.Type.Equals(broker.Type)))
            {
                broker = publicationManager.BrokerRepo.Query().Where(b => b.Name.Equals(brokerName)).FirstOrDefault();
            }
            else
            {
                broker.Repository = repo;
                broker = publicationManager.CreateBroker(broker);
            }

  
            return broker;
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

        public string GetZipFileName(long datasetid, long versionNr)
        {
            return datasetid + "_" + versionNr + "_Dataset.zip";
        }

        public string GetFileNameForDataRepo(long datasetid, long versionNr, string datarepo, string ext)
        {
            return datasetid + "_" + versionNr + "_Dataset_" + datarepo + "." + ext;
        }

        public bool Exist(long datasetid, long versionNr, string dataRepositoryName)
        {
            // check directory
            string d = GetDirectoryPath(datasetid, dataRepositoryName);
            if (!Directory.Exists(d)) return false;

            //check file
            string file = GetZipFileName(datasetid, versionNr);
            string filePath = Path.Combine(d, file);

            if (File.Exists(filePath)) return true;

            return false;
        }

        #endregion helper function
    }
}