using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using BExIS.Io.Transform.Validation.DSValidation;
using BExIS.Io.Transform.Validation.Exceptions;
using BExIS.Dlm.Entities.DataStructure;
using BExIS.Dlm.Services.DataStructure;
using Vaiona.Util.Cfg;
using BExIS.Dlm.Services.Data;
using BExIS.Dlm.Entities.Data;

namespace BExIS.Io.Transform.Output
{

    /// <summary>
    /// DataWriter is an abstract class that has basic functions for storing file.
    /// 
    /// </summary>
    /// <remarks></remarks>        
    public abstract class DataWriter
    {
        #region public
            /// <summary>
            /// if a few errors occur, they are stored here
            /// </summary>
            /// <remarks></remarks>
            /// <seealso cref="Error"/>    
            public List<Error> errorMessages { get; set; }
            public String[] visibleColumns { get; set; }
		 
	    #endregion

        #region protected

            /// <summary>
            /// File to be read as stream
            /// </summary>
            /// <remarks></remarks>
            /// <seealso cref="Stream"/>  
            protected Stream file { get; set; }


            /// <summary>
            /// List of VariableIdentifiers 
            /// with VariableName and VariableID
            /// </summary>
            /// <remarks></remarks>
            /// <seealso cref=""/>        
            protected List<VariableIdentifier> VariableIndentifiers = new List<VariableIdentifier>();


            protected List<List<string>> variableIdentifierRows = new List<List<string>>();

        #endregion

        //managers
        protected DatasetManager datasetManager = new DatasetManager();

        //Constructor
        public DataWriter()
        {
            datasetManager = new DatasetManager();
        }

        /// <summary>
        /// If file exist open a FileStream
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref="File"/>
        /// <param ="fileName">Full path of the file</param>   
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

        /// <summary>
        /// Create the general store path under AppConfiguration.DataPath
        /// with filename
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref="AppConfiguration"/>
        /// <param></param>       
        public string GetFullStorePath(long datasetId, long datasetVersionOrderNr, string title, string extention)
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

        /// <summary>
        /// Generate the gernal store path based on
        /// AppConfiguration.DataPath without filename
        /// </summary>
        /// <param name="datasetId"></param>
        /// <param name="datasetVersionOrderNr"></param>
        /// <returns></returns>
        public string GetStorePath(long datasetId, long datasetVersionOrderNr)
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

            return storePath;
        }

        /// <summary>
        /// Returns a Title based on incoming 
        /// parameters
        /// </summary>
        /// <param name="datasetId"></param>
        /// <param name="datasetVersionOrderNr"></param>
        /// <param name="title"></param>
        /// <param name="extention"></param>
        /// <returns></returns>
        public string GetNewTitle(long datasetId, long datasetVersionOrderNr, string title, string extention)
        {
            return datasetId.ToString() + "_" + datasetVersionOrderNr.ToString() + "_" + title + extention;
        }

        public string GetDynamicStorePath(long datasetId, long datasetVersionOrderNr, string title, string extention)
        {
            string storePath = Path.Combine("Datasets", datasetId.ToString(), "DatasetVersions");

            return Path.Combine(storePath, datasetId.ToString() + "_" + datasetVersionOrderNr.ToString() + "_" + title + extention);
        }

        public string GetFullStorePathOriginalFile(long datasetId, long datasetVersionOrderNr, string filename)
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

        //public bool MoveFile(string tempFile, string destinationPath)
        //{
        //    if (File.Exists(tempFile))
        //    {
        //        File.Move(tempFile, destinationPath);

        //        if (File.Exists(destinationPath))
        //        {
        //            return true;
        //        }else return false;
        //    }
        //    else return false;

        //}

        protected StructuredDataStructure GetDataStructure(long id)
        {
            DataStructureManager dataStructureManager = new DataStructureManager();
            return dataStructureManager.StructuredDataStructureRepo.Get(id);
        }

        public String GetTitle(long id)
        { 
            DatasetManager datasetManager = new DatasetManager();

            return datasetManager.GetDatasetLatestVersion(id).Metadata.SelectNodes("Metadata/Description/Description/Title/Title")[0].InnerText;
        }

        /// <summary>
        /// select a subset of the variables from a datastructure
        /// based on a list of variablenames
        /// </summary>
        /// <param name="source">full list of variables</param>
        /// <param name="selected">variablenames</param>
        /// <returns></returns>
        protected List<Variable> GetSubsetOfVariables(List<Variable> source, String[] selected)
        {
            return source.Where(p=> selected.Contains(p.Id.ToString())).ToList();
        }

        /// <summary>
        /// select a subset of the variables from a datastructure
        /// based on a list of variablenames
        /// </summary>
        /// <param name="source">full list of variables</param>
        /// <param name="selected">variablenames</param>
        /// <returns></returns>
        protected List<VariableValue> GetSubsetOfVariableValues(List<VariableValue> source, String[] selected)
        {
            return source.Where(p => selected.Contains(p.Variable.Id.ToString())).ToList();
        }

    }
}
