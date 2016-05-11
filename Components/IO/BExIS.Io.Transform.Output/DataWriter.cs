using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using BExIS.IO.Transform.Validation.DSValidation;
using BExIS.IO.Transform.Validation.Exceptions;
using BExIS.IO.DataType.DisplayPattern;
using BExIS.Dlm.Entities.DataStructure;
using BExIS.Dlm.Services.DataStructure;
using Vaiona.Utils.Cfg;
using BExIS.Dlm.Services.Data;
using BExIS.Dlm.Entities.Data;
using BExIS.Dlm.Entities.MetadataStructure;
using System.Xml.Linq;
using BExIS.Dlm.Services.TypeSystem;
using BExIS.Xml.Helpers;
using BExIS.Xml.Services;
using DocumentFormat.OpenXml.Drawing;
using Path = System.IO.Path;

/// <summary>
///
/// </summary>        
namespace BExIS.IO.Transform.Output
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
            public List<Error> ErrorMessages { get; set; }

            /// <summary>
            ///
            /// </summary>
            /// <remarks></remarks>
            /// <seealso cref=""/>        
            public String[] VisibleColumns { get; set; }
		 
	    #endregion

        #region protected

            /// <summary>
            /// File to be read as stream
            /// </summary>
            /// <remarks></remarks>
            /// <seealso cref="Stream"/>  
            protected Stream FileStream { get; set; }

            /// <summary>
            /// List of VariableIdentifiers 
            /// with VariableName and VariableID
            /// </summary>
            /// <remarks></remarks>
            /// <seealso cref=""/>        
            protected List<VariableIdentifier> VariableIdentifiers = new List<VariableIdentifier>();

            /// <summary>
            ///
            /// </summary>
            /// <remarks></remarks>
            /// <seealso cref=""/>        
            protected List<List<string>> VariableIdentifierRows = new List<List<string>>();

        #endregion

        //managers
        protected DatasetManager DatasetManager = new DatasetManager();

        //Constructor

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param>NA</param>       
        public DataWriter()
        {
            DatasetManager = new DatasetManager();
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

      
        public string CreateFile(string filepath)
        {
            string dicrectoryPath = Path.GetDirectoryName(filepath);
            createDicrectoriesIfNotExist(dicrectoryPath);

            try
            {
                if (!File.Exists(filepath))
                {
                    File.Create(filepath).Close();
                }

            }
            catch (Exception ex)
            {
                string message = ex.Message.ToString();
            }

            return filepath;
        }

        public string CreateFile(string path, string filename)
        {
            createDicrectoriesIfNotExist(path);

            string dataPath = Path.Combine(path, filename);

            try
            {
                if (!File.Exists(dataPath))
                {
                    File.Create(dataPath).Close();
                }

            }
            catch (Exception ex)
            {
                string message = ex.Message.ToString();
            }

            return dataPath;
        }

        protected void createDicrectoriesIfNotExist(string path)
        { 
            // if folder not exist
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
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

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param name="datasetId"></param>
        /// <param name="datasetVersionOrderNr"></param>
        /// <param name="title"></param>
        /// <param name="extention"></param>
        /// <returns></returns>
        public string GetDynamicStorePath(long datasetId, long datasetVersionOrderNr, string title, string extention)
        {
            string storePath = Path.Combine("Datasets", datasetId.ToString(), "DatasetVersions");

            return Path.Combine(storePath, datasetId.ToString() + "_" + datasetVersionOrderNr.ToString() + "_" + title + extention);
        }

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param name="datasetId"></param>
        /// <param name="datasetVersionOrderNr"></param>
        /// <param name="filename"></param>
        /// <returns></returns>
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

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param name="datasetId"></param>
        /// <param name="datasetVersionOrderNr"></param>
        /// <param name="filename"></param>
        /// <returns></returns>
        public string GetDynamicStorePathOriginalFile(long datasetId, long datasetVersionOrderNr, string filename)
        {
            //data/datasets/1/1/
            string storePath = Path.Combine("Datasets", datasetId.ToString());
            return Path.Combine(storePath, datasetId.ToString() + "_" + datasetVersionOrderNr.ToString() + "_" + filename);
        }

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param name="dataStructureId"></param>
        /// <param name="extention"></param>
        /// <returns></returns>
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

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param name="id"></param>       
        /// <returns></returns>
        protected StructuredDataStructure GetDataStructure(long id)
        {
            DataStructureManager dataStructureManager = new DataStructureManager();
            return dataStructureManager.StructuredDataStructureRepo.Get(id);
        }

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param name="id"></param>
        /// <returns></returns>
        public String GetTitle(long id)
        {
            if (DatasetManager.IsDatasetCheckedIn(id))
            {

                DatasetVersion datasetVersion = DatasetManager.GetDatasetLatestVersion(id);

                // get MetadataStructure 
                MetadataStructure metadataStructure = datasetVersion.Dataset.MetadataStructure;
                XDocument xDoc = XmlUtility.ToXDocument((XmlDocument) datasetVersion.Dataset.MetadataStructure.Extra);
                XElement temp = XmlUtility.GetXElementByAttribute("nodeRef", "name", "title", xDoc);

                string xpath = temp.Attribute("value").Value.ToString();
                string title = datasetVersion.Metadata.SelectSingleNode(xpath).InnerText;

                return title;
            }

            return "NoTitleAvailable";
        }

        /// <summary>
        /// select a subset of the variables from a datastructure
        /// based on a list of variablenames
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
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
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param name="source">full list of variables</param>
        /// <param name="selected">variablenames</param>
        /// <returns></returns>
        protected List<VariableValue> GetSubsetOfVariableValues(List<VariableValue> source, String[] selected)
        {
            return source.Where(p => selected.Contains(p.Variable.Id.ToString())).ToList();
        }

        protected string GetStringFormat(Dlm.Entities.DataStructure.DataType datatype)
        {
            DataTypeDisplayPattern ddp = DataTypeDisplayPattern.Materialize(datatype.Extra);
            if (ddp != null)
                return ddp.StringPattern;

            return "";
        }

        protected string GetFormatedValue( object value, Dlm.Entities.DataStructure.DataType datatype, string format)
        {
            string tmp = value.ToString();

            if (DataTypeUtility.IsTypeOf(value, datatype.SystemType))
            {
                //Type type = Type.GetType("System." + datatype.SystemType);

                switch (DataTypeUtility.GetTypeCode(datatype.SystemType))
                {
                    case DataTypeCode.Int16:
                    case DataTypeCode.Int32:
                    case DataTypeCode.Int64:
                    {
                        Int64 newValue = Convert.ToInt64(tmp);
                        return newValue.ToString(format);
                    }

                    case DataTypeCode.UInt16:
                    case DataTypeCode.UInt32:
                    case DataTypeCode.UInt64:
                    {
                        UInt64 newValue = Convert.ToUInt64(tmp);
                        return newValue.ToString(format);
                    }

                    case DataTypeCode.Decimal:
                    case DataTypeCode.Double:
                    {
                        Double newValue = Convert.ToDouble(tmp);
                        return newValue.ToString(format);
                    }

                    case DataTypeCode.DateTime:
                    {
                            DateTime dateTime;

                            if (DateTime.TryParse(tmp, out dateTime))
                            {
                                return dateTime.ToString(format);
                            }

                            if (DateTime.TryParse(tmp, new CultureInfo("de-DE", false), DateTimeStyles.None, out dateTime))
                            {
                                return dateTime.ToString(format);
                            }

                            if (DateTime.TryParse(tmp, new CultureInfo("en-US", false), DateTimeStyles.None, out dateTime))
                            {
                                return dateTime.ToString(format);
                            }

                            if (DateTime.TryParse(tmp, CultureInfo.InvariantCulture, DateTimeStyles.None, out dateTime))
                            {
                                return dateTime.ToString(format);
                            }

                            return tmp;
 ;                   }
                    default: return tmp;
                }
            }

            return "";
        }
    }
}
