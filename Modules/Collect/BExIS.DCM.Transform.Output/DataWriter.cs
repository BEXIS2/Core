using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

        public string GetStorePath(long datasetId, long datasetVersionId, string title, string extention)
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

            return Path.Combine(storePath, datasetVersionId.ToString() +"_"+ title + extention);
        }

        public string GetStorePathOriginalFile(long datasetId, long datasetVersionId, string filename)
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
   
            return Path.Combine(storePath, datasetVersionId.ToString() + filename);
        }

        public string GetDataStructureTemplatePath(long dataStructureId, string extention)
        {
            DataStructureManager dataStructureManager = new DataStructureManager();

            DataStructure dataStructure = dataStructureManager.StructuredDataStructureRepo.Get(dataStructureId);
            string dataStructureTitle = dataStructure.Name;
            // load datastructure from db an get the filepath from this object

            string dataPath = AppConfiguration.DataPath; //Path.Combine(AppConfiguration.WorkspaceRootPath, "Data");
            return Path.Combine(dataPath, "DataStructure", dataStructureId.ToString(), dataStructureTitle + extention);
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
