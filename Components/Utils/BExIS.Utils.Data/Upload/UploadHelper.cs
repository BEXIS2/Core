using BExIS.Dlm.Entities.Data;
using BExIS.Dlm.Entities.DataStructure;
using BExIS.Dlm.Services.Data;
using BExIS.Dlm.Services.DataStructure;
using BExIS.IO;
using BExIS.IO.Transform.Input;
using BExIS.Utils.Data.Upload;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Vaiona.Model.MTnt;
using Vaiona.Persistence.Api;

/// <summary>
///
/// </summary>
namespace BExIS.Utils.Upload
{
    /// <summary>
    ///
    /// </summary>
    /// <remarks></remarks>
    public class UploadHelper
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
        /// //original version
        public Dictionary<string, List<DataTuple>> GetSplitDatatuples2(List<DataTuple> newDatatuples, List<long> primaryKeys, DatasetVersion workingCopy, ref List<AbstractTuple> datatuplesFromDatabase)
        {
            using (IUnitOfWork unitOfWork = this.GetIsolatedUnitOfWork())
            {
                Dictionary<string, List<DataTuple>> data = new Dictionary<string, List<DataTuple>>();
                List<DataTuple> newDtList = new List<DataTuple>();
                List<DataTuple> editDtList = new List<DataTuple>();
                List<DataTuple> deleteDtList = new List<DataTuple>();

                DataTuple sourceDt;
                Dictionary<long, string> PkValues;

                // load datatuples from db
                // later packagesize

                for (int j = 0; j < newDatatuples.Count(); j++)
                {
                    DataTuple newDt = newDatatuples.ElementAt(j);

                    if (!IsEmpty(newDt))
                    {
                        PkValues = getPrimaryKeyValues(newDt, primaryKeys);

                        bool exist = false;

                        for (int i = 0; i < datatuplesFromDatabase.Count; i++)
                        {
                            IReadOnlyRepository<DataTuple> repo = unitOfWork.GetReadOnlyRepository<DataTuple>();
                            sourceDt = repo.Get(datatuplesFromDatabase.ElementAt(i).Id);
                            repo.LoadIfNot(sourceDt.VariableValues);

                            if (sourceDt != null && sameDatatuple(sourceDt, PkValues))
                            {
                                // check for edit
                                exist = true;
                                if (!Equal2(newDt, sourceDt))
                                {
                                    //sourceDt.Materialize();
                                    editDtList.Add(Merge(newDt, sourceDt));
                                }

                                if (datatuplesFromDatabase.Count > 0)
                                {
                                    datatuplesFromDatabase.RemoveAt(i);
                                }

                                break;
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
        }

        //temporary solution: norman :GetSplitDatatuples2
        public Dictionary<string, List<DataTuple>> GetSplitDatatuplesOld(List<DataTuple> incomingDatatuples, List<long> primaryKeys, DatasetVersion workingCopy, ref List<long> datatuplesFromDatabaseIds)
        {
            using (DatasetManager datasetManager = new DatasetManager())
            {
                Dictionary<string, List<DataTuple>> data = new Dictionary<string, List<DataTuple>>();
                Dictionary<string, DataTuple> newDtList = new Dictionary<string, DataTuple>();
                Dictionary<string, DataTuple> editDtList = new Dictionary<string, DataTuple>();
                List<DataTuple> deleteDtList = new List<DataTuple>();

                DataTupleIterator tupleIterator = new DataTupleIterator(datatuplesFromDatabaseIds, datasetManager, false);
                // Keep the DB loop outer to reduce the number of DB queries
                foreach (var existingTuple in tupleIterator)
                {
                    if (existingTuple == null || existingTuple.Id < 0) // it is unlikely to happen, but just to reinforce it.
                        continue;

                    // iterating over the in-memory newDataTuples is faster
                    for (int counter = 0; counter < incomingDatatuples.Count(); counter++)
                    //foreach (var incomingTuple in newDatatuples)
                    {
                        DataTuple incomingTuple = incomingDatatuples[counter];
                        if (!IsEmpty(incomingTuple))
                        {
                            // we first assume that any incoming tuple is new, and then try to check if it is not.
                            // this reduces the iterations of the inner loop.
                            // because the search takes the DB tuple and lloks for it in the coming tuples, it is posssible for an incoming tuple to be added more than once.
                            // So they are added to a dictionary to avoid duplicates
                            string keysValueNewDataTuple = getPrimaryKeysAsString(incomingTuple, primaryKeys);
                            if (!newDtList.ContainsKey(keysValueNewDataTuple))
                            {
                                newDtList.Add(keysValueNewDataTuple, incomingTuple); // by default, assume that the incoming tuple is new (not in the DB)
                            }
                            string keysValueSourceDatatuple = getPrimaryKeysAsString(existingTuple, primaryKeys);
                            if (keysValueNewDataTuple.Equals(keysValueSourceDatatuple)) // the incoming tuple exists in the DB
                            {
                                if (!Equal(incomingTuple, existingTuple)) // the incoming tuple is a changed version of an existing one
                                {
                                    // the incoming tuple is found in the DB and brings some changes, therefore not NEW!
                                    newDtList.Remove(keysValueNewDataTuple);
                                    if (!editDtList.ContainsKey(keysValueNewDataTuple))
                                    {
                                        // apply the changes to the exisiting one and register is an edited tuple
                                        editDtList.Add(keysValueNewDataTuple, Merge(incomingTuple, (DataTuple)existingTuple));
                                        // remove the current incoming item to shorten the list for the next round
                                        incomingDatatuples.RemoveAt(counter);
                                    }
                                    // the decision is made, hence break the inner loop
                                    break;
                                }
                                else // the incoming tuple is found in the BD, but introduces no change., hence no action is needed.
                                {
                                    // remove the incoming tuple from the list and from the new ones.
                                    newDtList.Remove(keysValueNewDataTuple);
                                    incomingDatatuples.RemoveAt(counter);
                                }
                            }
                            else // the incoming tuple does not match the PK, so it should be a new tuple, which is already added to the list.
                            { // DO NOTHING
                            }
                        }
                    }
                }

                // the rest of the incoming datatuples are in the new datatuples, all existing datatuples will be removed from that list
                data.Add("new", incomingDatatuples);
                data.Add("edit", editDtList.Values.ToList());
                return data;
            }
        }

        public Dictionary<string, List<DataTuple>> GetSplitDatatuples(List<DataTuple> incomingDatatuples, List<long> primaryKeys, DatasetVersion workingCopy, ref List<long> datatuplesFromDatabaseIds)
        {
            using (DatasetManager datasetManager = new DatasetManager())
            {
                Dictionary<string, List<DataTuple>> data = new Dictionary<string, List<DataTuple>>();
                Dictionary<string, DataTuple> newDtList = new Dictionary<string, DataTuple>();
                Dictionary<string, DataTuple> editDtList = new Dictionary<string, DataTuple>();
                List<DataTuple> deleteDtList = new List<DataTuple>();

                Dictionary<string, DataTuple> newDTDic = new Dictionary<string, DataTuple>();

                //iterating over incoming datatuples to gerenate the primary key and add it to a dictionary
                for (int counter = 0; counter < incomingDatatuples.Count(); counter++)
                {
                    DataTuple incomingTuple = incomingDatatuples[counter];
                    if (!IsEmpty(incomingTuple))
                    {
                        string keysValueNewDataTuple = getPrimaryKeysAsString(incomingTuple, primaryKeys);
                        newDTDic.Add(keysValueNewDataTuple, incomingTuple);
                    }
                }

                DataTupleIterator tupleIterator = new DataTupleIterator(datatuplesFromDatabaseIds, datasetManager, false);
                // Keep the DB loop outer to reduce the number of DB queries
                foreach (var existingTuple in tupleIterator)
                {
                    if (existingTuple == null || existingTuple.Id < 0) // it is unlikely to happen, but just to reinforce it.
                        continue;

                    string keysValueSourceDatatuple = getPrimaryKeysAsString(existingTuple, primaryKeys);

                    // check if a datatuple exist in the incoming dictionary
                    try
                    {
                        DataTuple incomingTuple = newDTDic[keysValueSourceDatatuple];

                        // a incoming datatuple with this key exist, check if there are equal
                        if (!Equal(incomingTuple, existingTuple)) // the incoming tuple is a changed version of an existing one
                        {
                            if (!editDtList.ContainsKey(keysValueSourceDatatuple))
                            {
                                // apply the changes to the exisiting one and register is an edited tuple
                                editDtList.Add(keysValueSourceDatatuple, Merge(incomingTuple, (DataTuple)existingTuple));
                                // remove the current incoming item to shorten the list for the next round
                            }
                        }

                        // the incoming tuple is found in the DB and brings some changes, therefore not NEW!
                        // so remove it from the dictionary
                        newDTDic.Remove(keysValueSourceDatatuple);
                    }
                    catch//if
                    {
                        //there is no datatuple incoming theat matches the db datatuple
                    }
                }

                // the rest of the incoming datatuples are in the new datatuples, all existing datatuples will be removed from that list
                data.Add("new", newDTDic.Values.ToList());
                data.Add("edit", editDtList.Values.ToList());
                return data;
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param name="dataTuple"></param>
        /// <returns></returns>
        private bool IsEmpty(DataTuple dataTuple)
        {
            foreach (VariableValue variableValue in dataTuple.VariableValues)
            {
                if (variableValue.Value != null) return false;
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
        private DataTuple Merge(DataTuple newDatatuple, DataTuple sourceDatatuple)
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
        //temporary solution: norman :Equal2
        //ToDo need Tests
        private bool Equal(AbstractTuple newDatatuple, AbstractTuple sourceDatatuple)
        {
            if (newDatatuple.JsonVariableValues == null && newDatatuple.VariableValues != null)
            {
                newDatatuple.Dematerialize();
            }

            if (sourceDatatuple.JsonVariableValues == null && sourceDatatuple.VariableValues != null)
            {
                sourceDatatuple.Dematerialize();
            }

            return newDatatuple.JsonVariableValues.Equals(sourceDatatuple.JsonVariableValues);
        }

        private bool Equal2(DataTuple newDatatuple, DataTuple sourceDatatuple)
        {
            foreach (VariableValue newVariableValue in newDatatuple.VariableValues)
            {
                foreach (VariableValue sourceVariableValue in sourceDatatuple.VariableValues)
                {
                    if (newVariableValue.VariableId.Equals(sourceVariableValue.VariableId))
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
        private Dictionary<long, string> getPrimaryKeyValues(DataTuple dt, List<long> pks)
        {
            Dictionary<long, string> temp = new Dictionary<long, string>();

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
        private bool sameDatatuple(DataTuple dt, Dictionary<long, string> pksVs)
        {
            bool IsSame = true;

            foreach (KeyValuePair<long, string> kvp in pksVs)
            {
                if (dt.VariableValues.Count > 0)
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
                else
                {
                    IsSame = false; break;
                }
            }

            return IsSame;
        }

        #endregion datatuples

        #region identifier

        /// <summary>
        /// test unique of primary keys in a FileStream
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param name="taskManager"></param>
        /// <param name="datasetId"></param>
        /// <param name="primaryKeys"></param>
        /// <param name="ext"></param>
        /// <param name="filename"></param>
        /// <returns></returns>
        public bool IsUnique(long datasetId, string ext, string filename, string filepath, FileReaderInfo info, long datastructureId)
        {
            Hashtable hashtable = new Hashtable();

            return IsUnique(datasetId, ext, filename, filepath, info, datastructureId, ref hashtable);

        }

        /// <summary>
        /// test unique of primary keys in a FileStream
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param name="taskManager"></param>
        /// <param name="datasetId"></param>
        /// <param name="primaryKeys"></param>
        /// <param name="ext"></param>
        /// <param name="filename"></param>
        /// <returns></returns>
        public bool IsUnique(long datasetId, string ext, string filename, string filepath, FileReaderInfo info, long datastructureId,ref Hashtable hashtable)
        {
            if(hashtable==null) hashtable=new Hashtable();
            Hashtable test = new Hashtable();
            List<string> testString = new List<string>();

            List<string> primaryValuesAsOneString = new List<string>();

            int packageSize = 1000;
            int position = 1;

            if (ext.Equals(".txt") || ext.Equals(".csv") || ext.Equals(".tsv"))
            {
                #region csv

                do
                {
                    primaryValuesAsOneString = new List<string>();

                    using (DataStructureManager datastructureManager = new DataStructureManager())
                    {
                        StructuredDataStructure sds = datastructureManager.StructuredDataStructureRepo.Get(datastructureId);
                        List<long> primaryKeys = sds.Variables.Where(v => v.IsKey).Select(v => v.Id).ToList();

                        AsciiFileReaderInfo afri = (AsciiFileReaderInfo)info;

                        AsciiReader reader = new AsciiReader(sds, afri, new IOUtility());
                        reader.Position = position;
                        using (Stream stream = reader.Open(filepath))
                        {

                            // get a list of values for each row
                            // e.g.
                            // primarky keys id, name
                            // 1 [1][David]
                            // 2 [2][Javad]
                            List<List<string>> tempList = reader.ReadValuesFromFile(stream, filename, datasetId, primaryKeys, packageSize);

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
                        }
                    }
                } while (primaryValuesAsOneString.Count > 0);

                #endregion csv
            }

            if (ext.Equals(".xlsm"))
            {
                #region excel template

                do
                {
                    //reset
                    primaryValuesAsOneString = new List<string>();

                    using (DataStructureManager datastructureManager = new DataStructureManager())
                    {
                        StructuredDataStructure sds = datastructureManager.StructuredDataStructureRepo.Get(datastructureId);
                        List<long> primaryKeys = sds.Variables.Where(v => v.IsKey).Select(v => v.Id).ToList();

                        ExcelReader reader = new ExcelReader(sds, new ExcelFileReaderInfo());
                        reader.Position = position;
                        using (Stream stream = reader.Open(filepath))
                        {

                            // get a list of values for each row
                            // e.g.
                            // primarky keys id, name
                            // 1 [1][David]
                            // 2 [2][Javad]
                            List<List<string>> tempList = reader.ReadValuesFromFile(stream, filename, datasetId, primaryKeys, packageSize);

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
                                    catch (Exception ex)
                                    {
                                        stream.Close();
                                        return false;
                                    }
                                }
                            }

                            position = reader.Position + 1;
                        }
                    }
                } while (primaryValuesAsOneString.Count > 0);

                #endregion excel template
            }

            return true;
        }



        /// <summary>
        /// test unique of primary keys in a string[][] as data
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param name="primaryIndex"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public bool IsUnique(int[] primaryIndex, string[][] data)
        {
            Hashtable hashtable = new Hashtable();
            List<string> primaryValuesAsOneString = new List<string>();

            primaryValuesAsOneString = new List<string>();

            foreach (string[] row in data)
            {
                string k = "";
                foreach (int i in primaryIndex)
                {
                    k += row[i];
                }

                primaryValuesAsOneString.Add(k);
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
        ////[MeasurePerformance]
        public Boolean IsUnique(long datasetId, ref Hashtable hashtable)
        {

            using (DataStructureManager datastructureManager = new DataStructureManager())
            using (DatasetManager datasetManager = new DatasetManager())
            {
                // load data

                Dataset dataset = datasetManager.GetDataset(datasetId);
                DatasetVersion datasetVersion;

                StructuredDataStructure sds = datastructureManager.StructuredDataStructureRepo.Get(dataset.DataStructure.Id);
                List<long> primaryKeys = sds.Variables.Where(v => v.IsKey).Select(v => v.Id).ToList();

                List<long> dataTupleIds = new List<long>();

                if (datasetManager.IsDatasetCheckedIn(datasetId))
                {
                    datasetVersion = datasetManager.GetDatasetLatestVersion(datasetId);

                    #region load all datatuples first

                    int size = 10000;
                    int counter = 0;
                    IEnumerable<AbstractTuple> dataTuples;

                    do
                    {
                        dataTuples = datasetManager.GetDatasetVersionEffectiveTuples(datasetVersion, counter, size);

                        //byte[] pKey;
                        string pKey;
                        foreach (DataTuple dt in dataTuples)
                        {
                            //pKey = getPrimaryKeysAsByteArray(dt, primaryKeys);
                            pKey = getPrimaryKeysAsString(dt, primaryKeys);

                            if (pKey.Count() > 0)
                            {
                                try
                                {
                                    //Debug.WriteLine(pKey +"   : " +Utility.ComputeKey(pKey));
                                    hashtable.Add(pKey, "");
                                    //hashtable.Add(pKey, 0);
                                }
                                catch
                                {
                                    return false;
                                }
                            }
                        }

                        counter++;
                    }
                    while (dataTuples.Count() >= (size * counter));

                    #endregion load all datatuples first
                }
                else
                {
                    throw new Exception("Dataset is not checked in.");
                }

                return true;
            }

        }

        /// <summary>
        /// test unique of primary keys on a dataset
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param name="datasetId"></param>
        /// <param name="primaryKeys"></param>
        /// <returns></returns>
        ////[MeasurePerformance]
        public Boolean IsUnique2(long datasetId, List<long> primaryKeys)
        {
            using (DatasetManager datasetManager = new DatasetManager())
            {
                Hashtable hashtable = new Hashtable();

                // load data
                Dataset dataset = datasetManager.GetDataset(datasetId);
                DatasetVersion datasetVersion;

                List<long> dataTupleIds = new List<long>();

                if (datasetManager.IsDatasetCheckedIn(datasetId))
                {
                    datasetVersion = datasetManager.GetDatasetLatestVersion(datasetId);

                    #region load all datatuples first

                    int size = 10000;
                    int counter = 0;
                    IEnumerable<long> dataTuplesIds;
                    dataTuplesIds = datasetManager.GetDatasetVersionEffectiveTupleIds(datasetVersion);
                    IEnumerable<long> currentIds;
                    DataTuple dt;
                    do
                    {
                        currentIds = dataTuplesIds.Skip(counter * size).Take(size);

                        //byte[] pKey;
                        string pKey;
                        foreach (long dtId in currentIds)
                        {
                            dt = datasetManager.DataTupleRepo.Query(d => d.Id.Equals(dtId)).FirstOrDefault();

                            //pKey = getPrimaryKeysAsByteArray(dt, primaryKeys);
                            pKey = pKey = getPrimaryKeysAsString(dt, primaryKeys);

                            if (pKey.Count() > 0)
                            {
                                try
                                {
                                    //Debug.WriteLine(pKey +"   : " +Utility.ComputeKey(pKey));
                                    hashtable.Add(pKey, "");
                                    //hashtable.Add(pKey, 0);
                                }
                                catch
                                {
                                    return false;
                                }
                            }
                        }

                        counter++;
                    }
                    while (currentIds.Count() >= (size * counter));

                    #endregion load all datatuples first
                }
                else
                {
                    throw new Exception("Dataset is not checked in.");
                }
                                return true;
            }
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

        private string getPrimaryKeysAsString(AbstractTuple datatuple, List<long> primaryKeys)
        {
            string value = "";

            foreach (long t in primaryKeys)
            {
                // empty means not equals value
                // so if value is empty add timestamp millisec
                datatuple.Materialize();
                object v = datatuple.VariableValues.Where(p => p.VariableId.Equals(t)).First().Value;
                if (v != null && !String.IsNullOrEmpty(v.ToString()))
                    value += ";" + v;
            }
            return value;
        }

        /// <summary>
        /// Return allowed extention list based on the DataStructureType
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param name="type"></param>
        /// <returns></returns>
        public static List<string> GetExtentionList(DataStructureType type, Tenant tenant = null)
        {
            if (type.Equals(DataStructureType.Structured))
            {
                return new List<string>()
                    {
                        ".xlsm",
                        ".xlsx",
                        ".txt",
                        ".csv",
                        ".tsv"
                    };
            }

            if (type.Equals(DataStructureType.None) || type.Equals(DataStructureType.Unstructured))
            {
                if (tenant != null) return tenant.AllowedFileExtensions;

                //Info
                // is not used anymore: list came from the this.Session.GetTenant().AllowedFileExtensions
                return new List<string>()
                {
                    //".avi",
                    //".bmp",
                    //".csv",
                    //".dbf",
                    //".doc",
                    //".docx",
                    //".gif",
                    //".jpg",
                    //".jpeg",
                    //".mp3",
                    //".mp4",
                    //".pdf",
                    //".png",
                    //".shp",
                    //".shx",
                    //".tif",
                    //".txt",
                    //".xls",
                    //".xlsm",
                    //".xlsx",
                    //".xsd",
                    //".zip"
                };
            }

            return new List<string>();
        }

        #endregion identifier
    }
}