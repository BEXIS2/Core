using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using BExIS.Io.Transform.Input;
using BExIS.Dlm.Entities.Data;
using BExIS.Dlm.Entities.DataStructure;
using BExIS.Dlm.Services.Data;
using BExIS.Dlm.Services.DataStructure;
using System.Security.Cryptography;
using BExIS.DCM.UploadWizard;
using System.Collections;
using System.Diagnostics;

/// <summary>
///
/// </summary>        
namespace BExIS.Dcm.UploadWizard
{
    /// <summary>
    ///
    /// </summary>
    /// <remarks></remarks>        
    public class UploadWizardHelper
    {

        #region datatuples

        /// <summary>
        /// 
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param name="newDatatuples"></param>
        /// <param name="primaryKeys"></param>
        /// <param name="workingCopy"></param>
        /// <returns></returns>
        public static Dictionary<string,List<DataTuple>> GetSplitDatatuples(List<DataTuple> newDatatuples, List<long> primaryKeys, DatasetVersion workingCopy)
        {

            Dictionary<string,List<DataTuple>> data = new Dictionary<string,List<DataTuple>>();
            List<DataTuple> newDtList = new List<DataTuple>();
            List<DataTuple> editDtList = new List<DataTuple>();
            List<DataTuple> deleteDtList = new List<DataTuple>();


            // alle gleichen raus filtern
            // alle ungleichen

            
            DatasetManager datasetManager = new DatasetManager();
            List<AbstractTuple> datatuplesSource = datasetManager.GetDatasetVersionEffectiveTuples(workingCopy).ToList();

            //if (datatuplesSource.Count > newDatatuples.Count)
            //{

            foreach (DataTuple newDt in newDatatuples)
            {
                if (!IsEmpty(newDt))
                {
                    Dictionary<long, string> PkValues = GetPrimaryKeyValues(newDt, primaryKeys);

                    bool exist = false;

                    foreach (DataTuple sourceDt in datatuplesSource)
                    {
                        if (SameDatatuple(sourceDt, PkValues))
                        {
                            // check for edit
                            exist = true;
                            if (!Equal(newDt, sourceDt))
                            {
                                editDtList.Add(Merge(newDt, sourceDt));
                            }

                        }

                    }

                    if (!exist)
                        newDtList.Add(newDt);

                }
            }
            //}

            
            data.Add("new", newDtList);
            data.Add("edit", editDtList);


            return data;

        }

        /// <summary>
        /// 
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param name="dataTuple"></param>
        /// <returns></returns>
        private static bool IsEmpty(DataTuple dataTuple)
        {
            foreach(VariableValue variableValue in dataTuple.VariableValues)
            {
                if (variableValue.Value!=null) return false;
            }

            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param name="newDatatuple"></param>
        /// <param name="sourceDatatuple"></param>
        /// <returns></returns>
        private static DataTuple Merge(DataTuple newDatatuple, DataTuple sourceDatatuple)
        {
            sourceDatatuple.VariableValues = newDatatuple.VariableValues;

            return sourceDatatuple;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param name="newDatatuple"></param>
        /// <param name="sourceDatatuple"></param>
        /// <returns></returns>
        private static bool Equal(DataTuple newDatatuple, DataTuple sourceDatatuple)
        {

            foreach(VariableValue newVariableValue in newDatatuple.VariableValues )
            {
                foreach(VariableValue sourceVariableValue in sourceDatatuple.VariableValues )
                {
                    if(newVariableValue.VariableId.Equals(sourceVariableValue.VariableId))
                    {
                        if (!newVariableValue.Value.Equals(sourceVariableValue.Value))
                            return false;
                        else break;
                    }
                }
            }


            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param name="dt"></param>
        /// <param name="pks"></param>
        /// <returns></returns>
        private static Dictionary<long, string> GetPrimaryKeyValues(DataTuple dt, List<long> pks)
        {
            Dictionary<long, string> temp = new Dictionary<long,string>();

            foreach (long k in pks)
            {
                object value = dt.VariableValues.Where(p => p.VariableId.Equals(k)).First().Value;
                if (value != null)
                    temp.Add(k, value.ToString());
                else
                    temp.Add(k, "");
            }

            return temp;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param name="dt"></param>
        /// <param name="pksVs"></param>
        /// <returns></returns>
        private static bool SameDatatuple(DataTuple dt, Dictionary<long, string> pksVs)
        {
            bool IsSame = true;

            foreach (KeyValuePair<long, string> kvp in pksVs)
            {

                object value = dt.VariableValues.Where(p => p.VariableId.Equals(kvp.Key)).First().Value;

                if (value != null)
                {
                    //value not equal different datatuples
                    if (value.ToString() != kvp.Value)
                    {
                        IsSame = false; break;
                    }
                }
                // if value is null means not equal to a not null value and 
                // v1 = null != v2 = null
                else
                {
                    IsSame = false; break;
                }
                
            }

            return IsSame;
        }


        #endregion

        #region identifier

            /// <summary>
            /// test unique of primary keys in a file
            /// </summary>
            /// <remarks></remarks>
            /// <seealso cref=""/>
            /// <param name="taskManager"></param>
            /// <param name="datasetId"></param>
            /// <param name="primaryKeys"></param>
            /// <param name="ext"></param>
            /// <param name="filename"></param>
            /// <returns></returns>
            public static bool IsUnique(TaskManager taskManager, long datasetId, List<long> primaryKeys, string ext, string filename)
            {

                Hashtable hashtable = new Hashtable();
                Hashtable test = new Hashtable();
                List<string> testString = new List<string>();

                List<string> primaryValuesAsOneString = new List<string>();

                TaskManager TaskManager = taskManager;
                int packageSize = 1000;
                int position = 1;

                if (ext.Equals(".txt") || ext.Equals(".csv"))
                {
                    #region csv
                    do
                    {
                        primaryValuesAsOneString = new List<string>();

                        AsciiReader reader = new AsciiReader();
                        reader.Position = position;
                        Stream stream = reader.Open(TaskManager.Bus["FilePath"].ToString());

                        AsciiFileReaderInfo afri = (AsciiFileReaderInfo)TaskManager.Bus["FileReaderInfo"];

                        DataStructureManager datastructureManager = new DataStructureManager();
                        StructuredDataStructure sds = datastructureManager.StructuredDataStructureRepo.Get(Convert.ToInt64(TaskManager.Bus["DataStructureId"].ToString()));
                        // get a list of values for each row
                        // e.g.
                        // primarky keys id, name
                        // 1 [1][David]
                        // 2 [2][Javad]
                        List<List<string>> tempList = reader.ReadValuesFromFile(stream, filename, afri, sds, datasetId, primaryKeys, packageSize);

                        // convert List of Lists to list of strings
                        // 1 [1][David] = 1David
                        // 2 [2][Javad] = 2Javad
                        foreach (List<string> l in tempList)
                        {
                            string tempString = "";
                            foreach (string s in l)
                            {
                                tempString += s;
                            }
                            if (!String.IsNullOrEmpty(tempString)) primaryValuesAsOneString.Add(tempString);
                        }

                        // add all primary keys pair into the hasttable
                        foreach (string pKey in primaryValuesAsOneString)
                        {
                            if (pKey != "")
                            {

                                try
                                {
                                    hashtable.Add(Utility.ComputeKey(pKey), "pKey");
                                }
                                catch
                                {
                                    return false;
                                }
                            }

                        }


                        position = reader.Position + 1;
                        stream.Close();

                    } while (primaryValuesAsOneString.Count > 0);

                    #endregion
                }


                if (ext.Equals(".xlsm") )
                {
                    #region excel template

                    do
                    {
                        //reset
                        primaryValuesAsOneString = new List<string>();

                        ExcelReader reader = new ExcelReader();
                        reader.Position = position;
                        Stream stream = reader.Open(TaskManager.Bus["FilePath"].ToString());

                        DataStructureManager datastructureManager = new DataStructureManager();
                        StructuredDataStructure sds = datastructureManager.StructuredDataStructureRepo.Get(Convert.ToInt64(TaskManager.Bus["DataStructureId"].ToString()));
                        // get a list of values for each row
                        // e.g.
                        // primarky keys id, name
                        // 1 [1][David]
                        // 2 [2][Javad]
                        List<List<string>> tempList = reader.ReadValuesFromFile(stream, filename, sds, datasetId, primaryKeys, packageSize);

                        // convert List of Lists to list of strings
                        // 1 [1][David] = 1David
                        // 2 [2][Javad] = 2Javad
                        foreach (List<string> l in tempList)
                        {
                            string tempString = "";
                            foreach (string s in l)
                            {
                                tempString += s;
                            }
                            if (!String.IsNullOrEmpty(tempString)) primaryValuesAsOneString.Add(tempString);
                        }

                        // add all primary keys pair into the hasttable
                        foreach (string pKey in primaryValuesAsOneString)
                        {
                            if (pKey != "")
                            {

                                try
                                {
                                    hashtable.Add(Utility.ComputeKey(pKey), pKey);
                                }
                                catch
                                {
                                    stream.Close();
                                    return false;
                                }
                            }

                        }


                        position = reader.Position + 1;
                        stream.Close();

                    } while (primaryValuesAsOneString.Count > 0);


                    #endregion
                }

                return true;
            }

            /// <summary>
            /// test unique of primary keys on a dataset
            /// </summary>
            /// <remarks></remarks>
            /// <seealso cref=""/>
            /// <param name="datasetId"></param>
            /// <param name="primaryKeys"></param>
            /// <returns></returns>
            public static Boolean IsUnique(long datasetId, List<long> primaryKeys)
            {

                Hashtable hashtable = new Hashtable();

                 // load data
                DatasetManager datasetManager = new DatasetManager();
                Dataset dataset = datasetManager.GetDataset(datasetId);
                DatasetVersion datasetVersion;
                
        
                List<long> dataTupleIds = new List<long>();

                if (datasetManager.IsDatasetCheckedIn(datasetId))
                {
                    datasetVersion = datasetManager.GetDatasetLatestVersion(datasetId);
                    dataTupleIds = datasetManager.GetDatasetVersionEffectiveTupleIds(datasetVersion);

                    string pKey = "";
                    DataTuple dataTuple;
                    foreach (long id in dataTupleIds)
                    {
                        dataTuple = datasetManager.DataTupleRepo.Get(id);


                        pKey = GetPrimaryKeysAsString(dataTuple, primaryKeys);

                        if (pKey != "")
                        {

                            try
                            {
                                Debug.WriteLine(pKey +"   : " +Utility.ComputeKey(pKey));
                                hashtable.Add(Utility.ComputeKey(pKey), "");
                            }
                            catch
                            {
                                return false;
                            }
                        }
                    }
                }
                else
                {
                    throw new Exception("Dataset is not checked in.");
                }

                return true;
            }

            /// <summary>
            ///  convert primary keys to string
            ///  returns null if a emtpy string is inside
            /// </summary>
            /// <remarks></remarks>
            /// <seealso cref=""/>
            /// <param name="datatuple"></param>
            /// <param name="primaryKeys"></param>
            /// <returns></returns>
            private static string GetPrimaryKeysAsString(DataTuple datatuple, List<long> primaryKeys)
            {
                string value = "";

                foreach (long t in primaryKeys)
                {
                    // empty means not equals value
                    // so if value is empty add timestamp millisec
                    datatuple.Materialize();
                    object v = datatuple.VariableValues.Where(p => p.VariableId.Equals(t)).First().Value;
                    if (v != null)
                        if (!String.IsNullOrEmpty(v.ToString()))
                            value += v;
                        else
                            return "";
                    else
                        return "";
                }
                return value;
            }
         
            /// <summary>
            /// 
            /// </summary>
            /// <remarks></remarks>
            /// <seealso cref=""/>
            /// <param name="type"></param>
            /// <returns></returns>
            public static List<string> GetExtentionList(DataStructureType type)
            { 
                if(type.Equals(DataStructureType.Structured))
                {
                    return new List<string>()
                    {
                        ".xlsm",
                        ".txt",
                        ".csv"
                    };
                }

                if (type.Equals(DataStructureType.Unstructured))
                {
                    return new List<string>()
                    {
                        ".avi",
                        ".csv",
                        ".doc",
                        ".docx",
                        ".gif",
                        ".jpg",
                        ".mp3",
                        ".mp4",
                        ".pdf",
                        ".png",
                        ".shp",
                        ".tif",
                        ".txt",
                        ".xls",
                        ".xlsm",
                        ".zip"
                    };
                }

                return new List<string>();
            }

        #endregion

    }
}
