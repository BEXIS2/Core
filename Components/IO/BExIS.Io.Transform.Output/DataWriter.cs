using BExIS.Dlm.Entities.Data;
using BExIS.Dlm.Entities.DataStructure;
using BExIS.Dlm.Entities.MetadataStructure;
using BExIS.Dlm.Services.Data;
using BExIS.Dlm.Services.DataStructure;
using BExIS.Dlm.Services.TypeSystem;
using BExIS.IO.DataType.DisplayPattern;
using BExIS.IO.Transform.Validation.DSValidation;
using BExIS.IO.Transform.Validation.Exceptions;
using BExIS.Xml.Helpers;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using Vaiona.Utils.Cfg;
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

        #endregion public

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

        protected StructuredDataStructure dataStructure = null;

        // line number
        protected int rowIndex;

        #endregion protected

        //managers
        protected DatasetManager DatasetManager;

        protected IOUtility IOUtility;

        //Constructor

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param>NA</param>
        public DataWriter() : this(new IOUtility(), new DatasetManager())
        {
        }

        public DataWriter(IOUtility iOUtility) : this(iOUtility, new DatasetManager())
        {
        }

        public DataWriter(DatasetManager datasetManager) : this(new IOUtility(), datasetManager)
        {
        }

        public DataWriter(IOUtility iOUtility, DatasetManager datasetManager)
        {
            IOUtility = iOUtility;
            DatasetManager = datasetManager;
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


        public string CreateFile(string path, string filename)
        {
            createDirectoriesIfNotExist(path);

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

        protected void createDirectoriesIfNotExist(string path)
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
        public string GetFullStorePath(long datasetId, long datasetVersionOrderNr, string title, string extension)
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

            return Path.Combine(storePath, datasetId.ToString() + "_" + datasetVersionOrderNr.ToString() + "_" + title + extension);
        }

        /// <summary>
        /// create the general store path under AppConfiguration.DataPath
        /// using the given namespace and file name
        /// </summary>
        /// <param name="ns"></param>
        /// <param name="title"></param>
        /// <param name="extension"></param>
        /// <returns></returns>
        public string GetFullStorePath(string ns, string title, string extension)
        {
            string dataPath = AppConfiguration.DataPath;

            // data/datasets/ns/
            string storePath = Path.Combine(dataPath, "Datasets", ns);

            if (Directory.Exists(dataPath))
            {
                createDirectoriesIfNotExist(storePath);
            }

            return Path.Combine(storePath, title + extension);
        }

        /// <summary>
        /// Generate the general store path based on
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
        /// <param name="extension"></param>
        /// <returns></returns>
        public string GetNewTitle(long datasetId, long datasetVersionOrderNr, string title, string extension)
        {
            return datasetId.ToString() + "_" + datasetVersionOrderNr.ToString() + "_" + title + extension;
        }

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param name="datasetId"></param>
        /// <param name="datasetVersionOrderNr"></param>
        /// <param name="title"></param>
        /// <param name="extension"></param>
        /// <returns></returns>
        public string GetDynamicStorePath(long datasetId, long datasetVersionOrderNr, string title, string extension)
        {
            return IOHelper.GetDynamicStorePath(datasetId, datasetVersionOrderNr, "data", extension);
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
        /// <param name="extension"></param>
        /// <returns></returns>
        public string GetDataStructureTemplatePath(long dataStructureId, string extension)
        {
            using (DataStructureManager dataStructureManager = new DataStructureManager())
            {
                StructuredDataStructure dataStructure = dataStructureManager.StructuredDataStructureRepo.Get(dataStructureId);
                string dataStructureTitle = dataStructure.Name;
                // load datastructure from db an get the filepath from this object

                ExcelTemplateProvider provider = new ExcelTemplateProvider("BExISppTemplate_Clean.xlsm");
                string path = "";

                if (dataStructure.TemplatePaths != null)
                {
                    XmlNode resources = dataStructure.TemplatePaths.FirstChild;

                    XmlNodeList resource = resources.ChildNodes;

                    foreach (XmlNode x in resource)
                    {
                        if (x.Attributes.GetNamedItem("Type").Value == "Excel")
                            if (File.Exists(x.Attributes.GetNamedItem("Path").Value))
                            {
                                path = x.Attributes.GetNamedItem("Path").Value;
                            }
                            else
                            {
                                path = provider.CreateTemplate(dataStructure);
                            }
                    }
                    //string dataPath = AppConfiguration.DataPath; //Path.Combine(AppConfiguration.WorkspaceRootPath, "Data");
                    return Path.Combine(AppConfiguration.DataPath, path);
                }
                path = provider.CreateTemplate(dataStructure);
                return Path.Combine(AppConfiguration.DataPath, path);
            }
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
            if (dataStructure == null)
            {
                using (DataStructureManager dataStructureManager = new DataStructureManager())
                {
                    dataStructure = dataStructureManager.StructuredDataStructureRepo.Get(id);
                    dataStructure.Variables = dataStructure.Variables.OrderBy(v => v.OrderNo).ToList();

                    dataStructureManager.StructuredDataStructureRepo.LoadIfNot(dataStructure.Variables.Select(v => v.DataAttribute));
                    dataStructureManager.StructuredDataStructureRepo.LoadIfNot(dataStructure.Variables.Select(v => v.MissingValues));

                    foreach (var v in dataStructure.Variables)
                    {
                        var d = v.DataAttribute.DataType.Description;
                        var m = v.MissingValues.ToList().Select(mis => mis.Id);
                    }

                }
            }

            return dataStructure;
        }

        protected StructuredDataStructure GetDataStructure()
        {
            return dataStructure;
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
                XDocument xDoc = XmlUtility.ToXDocument((XmlDocument)datasetVersion.Dataset.MetadataStructure.Extra);
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
            return source.Where(p => selected.Contains(p.Id.ToString())).ToList();
        }

        /// <summary>
        /// select a subset of the variable ids from a datastructure
        /// based on a list of variablenames
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param name="source">full list of variables</param>
        /// <param name="selected">variablenames</param>
        /// <returns></returns>
        protected List<long> GetSubsetOfVariableIds(IEnumerable<Variable> source, String[] selected)
        {
            return source.Where(p => selected.Contains(p.Label.ToString())).ToList().Select(v => v.Id).ToList();
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

        protected string GetFormatedValue(object value, Dlm.Entities.DataStructure.DataType datatype, string format)
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

                            return IOUtility.ExportDateTimeString(value.ToString(), format, out dateTime);
                        }
                    default: return tmp;
                }
            }

            return "";
        }

        #region abstract definitions

        // startup / close actions
        protected abstract void Init(string filepath, long dataStructureId);

        protected abstract void Close();

        // add table header
        protected abstract bool AddHeader(StructuredDataStructure header);

        protected abstract bool AddHeader(DataColumnCollection header);

        protected abstract bool AddHeader(string[] header);

        //Add Units
        protected abstract bool AddUnits(StructuredDataStructure structure);

        protected abstract bool AddUnits(string[] units);

        // add a single row to the output file
        protected abstract bool AddRow(AbstractTuple tuple, long rowIndex);

        protected abstract bool AddRow(DataRow row, long rowIndex, bool internalId=false);

        protected abstract bool AddRow(string[] row, long rowIndex);

        #endregion abstract definitions

        #region add data to files

        /// <summary>
        /// Add Datatuples to a Excel Template file
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param name="dataTuples"> Datatuples to add</param>
        /// <param name="filePath">Path of the excel template file</param>
        /// <param name="dataStructureId">Id of datastructure</param>
        /// <returns>List of Errors or null</returns>
        public List<Error> AddDataTuples(DatasetManager datasetManager, List<long> dataTuplesIds, string filePath, long dataStructureId)
        {
            if (File.Exists(filePath))
            {
                using (DataStructureManager dataStructureManager = new DataStructureManager())
                {
                    dataStructure = dataStructureManager.StructuredDataStructureRepo.Get(dataStructureId);

                    // setup file
                    Init(filePath, dataStructureId);

                    // add header
                    StructuredDataStructure sds = GetDataStructure(dataStructureId);
                    AddHeader(sds);

                    // iterate over all input rows
                    DataTupleIterator tupleIterator = new DataTupleIterator(dataTuplesIds, datasetManager);
                    foreach (var tuple in tupleIterator)
                    {
                        // add row and increment current index
                        if (AddRow(tuple, rowIndex) && !tuple.Id.Equals(dataTuplesIds.Last()))
                        {
                            rowIndex += 1;
                        }
                    }

                    // close the excel file
                    Close();
                }
            }

            return ErrorMessages;
        }

        /// <summary>
        /// Add Datatuples to a Excel Template file
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param name="dataTuples"> Datatuples to add</param>
        /// <param name="filePath">Path of the excel template file</param>
        /// <param name="dataStructureId">Id of datastructure</param>
        /// <returns>List of Errors or null</returns>
        //public List<Error> AddDataTuples(List<AbstractTuple> dataTuples, string filePath, long dataStructureId)
        //{
        //    if (File.Exists(filePath))
        //    {
        //        // setup file
        //        Init(filePath, dataStructureId);

        //        // add header
        //        StructuredDataStructure sds = GetDataStructure(dataStructureId);
        //        AddHeader(sds);

        //        // iterate over all input rows
        //        foreach (DataTuple dataTuple in dataTuples)
        //        {
        //            // add row and increment current index
        //            if (AddRow(dataTuple, rowIndex) && !dataTuple.Equals(dataTuples.Last()))
        //            {
        //                rowIndex += 1;
        //            }
        //        }

        //        // close the excel file
        //        Close();

        //    }

        //    return ErrorMessages;
        //}

        /// <summary>
        /// add all rows of the given array to a file using the given datastructure
        /// </summary>
        /// <param name="table"></param>
        /// <param name="filePath"></param>
        /// <param name="dataStructureId"></param>
        /// <returns></returns>
        public List<Error> AddData(string[][] data, string[] columns, string filePath, long dataStructureId, string[] units = null)
        {
            if (File.Exists(filePath))
            {
                using (DataStructureManager dataStructureManager = new DataStructureManager())
                {
                    dataStructure = dataStructureManager.StructuredDataStructureRepo.Get(dataStructureId);

                    if (rowIndex == 0) rowIndex = 1;

                    // setup excel file
                    Init(filePath, dataStructureId);

                    // add header
                    AddHeader(columns);

                    // add unit
                    if (units != null) AddUnits(units);

                    // iterate over all input rows
                    foreach (string[] row in data)
                    {
                        // add row and increment current index
                        if (AddRow(row, rowIndex))
                        {
                            rowIndex += 1;
                        }
                    }

                    // close the excel file
                    Close();
                }
            }

            return ErrorMessages;
        }

        /// <summary>
        /// add all rows of the given datatable to a file using the given datastructure
        /// </summary>
        /// <param name="table"></param>
        /// <param name="filePath"></param>
        /// <param name="dataStructureId"></param>
        /// <returns></returns>
        public List<Error> AddData(DataTable table, string filePath, long dataStructureId, string[] units = null, bool internalId = false)
        {
            if (File.Exists(filePath))
            {
                using (DataStructureManager dataStructureManager = new DataStructureManager())
                {
                    dataStructure = dataStructureManager.StructuredDataStructureRepo.Get(dataStructureId);
                    if (rowIndex == 0) rowIndex = 1;

                    // setup excel file
                    Init(filePath, dataStructureId);

                    // add header
                    AddHeader(table.Columns);

                    // Add Units
                    if (units != null) AddUnits(units);

                    // iterate over all input rows
                    foreach (DataRow row in table.Rows)
                    {
                        // add row and increment current index
                        if (AddRow(row, rowIndex, internalId))
                        {
                            rowIndex += 1;
                        }
                    }

                    // close the excel file
                    Close();
                }
            }

            return ErrorMessages;
        }

        /// <summary>
        /// add all rows of the given list to a file using the given datastructure
        /// </summary>
        /// <param name="rows"></param>
        /// <param name="filePath"></param>
        /// <param name="dataStructureId"></param>
        /// <returns></returns>
        public List<Error> AddData(DataRowCollection rows, string filePath, long dataStructureId)
        {
            if (File.Exists(filePath))
            {
                using (DataStructureManager dataStructureManager = new DataStructureManager())
                {
                    dataStructure = dataStructureManager.StructuredDataStructureRepo.Get(dataStructureId);

                    if (rowIndex == 0) rowIndex = 1;

                    // setup excel file
                    Init(filePath, dataStructureId);

                    // iterate over all input rows
                    foreach (DataRow row in rows)
                    {
                        // add row and increment current index
                        if (AddRow(row, rowIndex))
                        {
                            rowIndex += 1;
                        }
                    }

                    // close the excel file
                    Close();
                }
            }

            return ErrorMessages;
        }

        #endregion add data to files

        #region add data to files (compatibility aliases)

        //public List<Error> AddDataTuplesToTemplate(DatasetManager datasetManager, List<long> dataTuplesIds, string filePath, long dataStructureId)
        //{
        //    return AddDataTuples(datasetManager, dataTuplesIds, filePath, dataStructureId);
        //}

        //public List<Error> AddDataTuplesToTemplate(List<AbstractTuple> dataTuples, string filePath, long dataStructureId)
        //{
        //    return AddDataTuples(dataTuples, filePath, dataStructureId);
        //}

        public List<Error> AddDataTuplesToFile(DataTable table, string filePath, long dataStructureId, string[] units)
        {
            return AddData(table, filePath, dataStructureId, units);
        }

        #endregion add data to files (compatibility aliases)
    }
}