using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using BExIS.DCM.Transform.Validation.DSValidation;
using BExIS.DCM.Transform.Validation.Exceptions;
using BExIS.Dlm.Entities.DataStructure;
using BExIS.Dlm.Services.DataStructure;
using Vaiona.Util.Cfg;

namespace BExIS.DCM.Transform.Output
{
    public abstract class DataWriter
    {
        #region public
            public List<Error> errorMessages { get; set; }
		 
	    #endregion

        #region protected
            protected Stream file { get; set; }

            // List of Variables from file
            protected List<VariableIdentifier> VariableIndentifiers = new List<VariableIdentifier>();
            protected List<List<string>> variableIdentifierRows = new List<List<string>>();

        #endregion


        public static FileStream Open(string fileName)
        {
            FileStream stream;

            if (File.Exists(fileName))
            {
                try
                {
                    stream = File.Open(fileName, FileMode.Open, FileAccess.ReadWrite);
                }
                catch (Exception ex)
                {

                    return null;
                }

                
                return stream;
  
            }
            else
                return null;
        }

        public string GetStorePath(long datasetId, long datasetVersionOrderNr, string title, string extention)
        {
            string dataPath = AppConfiguration.DataPath; //Path.Combine(AppConfiguration.WorkspaceRootPath, "Data");

            //data/datasets/1/1/
            string storePath = Path.Combine(dataPath, "Datasets", datasetId.ToString(), "DatasetVersions");

            if (Directory.Exists(dataPath))
            {
                // if folder not exist
                if (!Directory.Exists(storePath))
                {
                    Directory.CreateDirectory(storePath);
                }
            }

            return Path.Combine(storePath,datasetId.ToString()+"_"+datasetVersionOrderNr.ToString() + "_" + title + extention);
        }

        public string GetDynamicStorePath(long datasetId, long datasetVersionOrderNr, string title, string extention)
        {
            string storePath = Path.Combine("Datasets", datasetId.ToString(), "DatasetVersions");

            return Path.Combine(storePath, datasetId.ToString() + "_" + datasetVersionOrderNr.ToString() + "_" + title + extention);
        }

        public string GetStorePathOriginalFile(long datasetId, long datasetVersionOrderNr, string filename)
        {
            string dataPath = AppConfiguration.DataPath; //Path.Combine(AppConfiguration.WorkspaceRootPath, "Data");

            //data/datasets/1/1/
            string storePath = Path.Combine(dataPath, "Datasets", datasetId.ToString());

            if (Directory.Exists(dataPath))
            {
                // if folder not exist
                if (!Directory.Exists(storePath))
                {
                    Directory.CreateDirectory(storePath);
                }
            }

            return Path.Combine(storePath, datasetId.ToString() + "_" + datasetVersionOrderNr.ToString() + "_" + filename);
        }

        public string GetDynamicStorePathOriginalFile(long datasetId, long datasetVersionOrderNr, string filename)
        {
            //data/datasets/1/1/
            string storePath = Path.Combine("Datasets", datasetId.ToString());
            return Path.Combine(storePath, datasetId.ToString() + "_" + datasetVersionOrderNr.ToString() + "_" + filename);
        }

        public string GetDataStructureTemplatePath(long dataStructureId, string extention)
        {
            DataStructureManager dataStructureManager = new DataStructureManager();

            StructuredDataStructure dataStructure = dataStructureManager.StructuredDataStructureRepo.Get(dataStructureId);
            string dataStructureTitle = dataStructure.Name;
            // load datastructure from db an get the filepath from this object

            string path = "";

            if (dataStructure.TemplatePaths != null)
            {
                XmlNode resources = dataStructure.TemplatePaths.FirstChild;

                XmlNodeList resource = resources.ChildNodes;

                foreach (XmlNode x in resource)
                {
                    if (x.Attributes.GetNamedItem("Type").Value == "Excel")
                        path = x.Attributes.GetNamedItem("Path").Value;
                }


                //string dataPath = AppConfiguration.DataPath; //Path.Combine(AppConfiguration.WorkspaceRootPath, "Data");
                return Path.Combine(AppConfiguration.DataPath, path);
            }
            return "";
        }

        public bool MoveFile(string tempFile, string destinationPath)
        {
            if (File.Exists(tempFile))
            {
                File.Move(tempFile, destinationPath);

                if (File.Exists(destinationPath))
                {
                    return true;
                }else return false;
            }
            else return false;

        }

        protected StructuredDataStructure GetDataStructure(long id)
        {
            DataStructureManager dataStructureManager = new DataStructureManager();
            return dataStructureManager.StructuredDataStructureRepo.Get(id);
        }

    }
}
