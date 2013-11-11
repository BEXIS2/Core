using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using BExIS.DCM.Transform.Input;
using BExIS.Dlm.Entities.Data;
using BExIS.Dlm.Entities.DataStructure;
using BExIS.Dlm.Services.Data;
using BExIS.Dlm.Services.DataStructure;

namespace BExIS.DCM.UploadWizard
{
    public class UploadWizardHelper
    {
        #region datatuples

        public static Dictionary<string,List<DataTuple>> GetSplitDatatuples(List<DataTuple> newDatatuples, List<long> primaryKeys, DatasetVersion workingCopy)
        {

            Dictionary<string,List<DataTuple>> data = new Dictionary<string,List<DataTuple>>();
            List<DataTuple> newDtList = new List<DataTuple>();
            List<DataTuple> editDtList = new List<DataTuple>();
            List<DataTuple> deleteDtList = new List<DataTuple>();


            // alle gleichen raus filtern
            // alle ungleichen


            DatasetManager datasetManager = new DatasetManager();
            List<DataTuple> datatuplesSource = datasetManager.GetDatasetVersionEffectiveTuples(workingCopy).ToList();

            //if (datatuplesSource.Count > newDatatuples.Count)
            //{

            foreach (DataTuple newDt in newDatatuples)
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
            //}

            
            data.Add("new", newDtList);
            data.Add("edit", editDtList);


            return data;

        }

        private static DataTuple Merge(DataTuple newDatatuple, DataTuple sourceDatatuple)
        {
            sourceDatatuple.VariableValues = newDatatuple.VariableValues;

            return sourceDatatuple;
        }

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

        private static Dictionary<long, string> GetPrimaryKeyValues(DataTuple dt, List<long> pks)
        {
            Dictionary<long, string> temp = new Dictionary<long,string>();

            foreach (long k in pks)
            {
                string value = dt.VariableValues.Where(p => p.VariableId.Equals(k)).First().Value.ToString();
                temp.Add(k, value);
            }

            return temp;
        }

        private static bool SameDatatuple(DataTuple dt, Dictionary<long, string> pksVs)
        {
            bool IsSame = true;

            foreach (KeyValuePair<long, string> kvp in pksVs)
            {
                string value = dt.VariableValues.Where(p => p.VariableId.Equals(kvp.Key)).First().Value.ToString();
                if (value != kvp.Value)
                {
                    IsSame = false; break;
                }
            }

            return IsSame;
        }


        #endregion

        #region identifier

            public static List<string> GetIdentifierList(long datasetId, List<long> primaryKeys)
            {
                List<string> temp = new List<string>();

                // load data
                DatasetManager dm = new DatasetManager();

                DatasetVersion datasetVersion = dm.GetDatasetLatestVersion(datasetId);

                List<DataTuple> datatuples = dm.GetDatasetVersionEffectiveTuples(datasetVersion);
                if (datatuples.Count > 0)
                {
                    foreach (DataTuple dt in datatuples)
                    {
                        string value = "";

                        foreach (long t in primaryKeys)
                        {
                            value += dt.VariableValues.Where(p => p.VariableId.Equals(t)).First().Value;
                        }

                        temp.Add(value);
                    }
                }
                return temp;

            }

            public static List<string> GetIdentifierList(TaskManager taskManager,  long datasetId, List<long> primaryKeys, string ext, string filename)
            {
                List<string> temp = new List<string>();
                TaskManager TaskManager = taskManager;

                if (ext.Equals(".txt") || ext.Equals(".csv"))
                {
                    AsciiReader reader = new AsciiReader();
                    Stream stream = reader.Open(TaskManager.Bus["FilePath"].ToString());

                    AsciiFileReaderInfo afri = (AsciiFileReaderInfo)TaskManager.Bus["FileReaderInfo"];

                    DataStructureManager datastructureManager = new DataStructureManager();
                    StructuredDataStructure sds = datastructureManager.StructuredDataStructureRepo.Get(Convert.ToInt64(TaskManager.Bus["DataStructureId"].ToString()));

                    List<List<string>> tempList = reader.ReadValuesFromFile(stream, filename, afri, sds, datasetId, primaryKeys);

                    //tempList.ForEach(p => p.ForEach(s => temp.Add(s)));
                    foreach (List<string> l in tempList)
                    {
                        string tempString = "";
                        foreach (string s in l)
                        {
                            tempString += s;
                        }
                        if (!String.IsNullOrEmpty(tempString)) temp.Add(tempString);
                    }

                    stream.Close();
                }

                return temp;
            }

            public static bool CheckDuplicates(List<string> data)
            {
                if (data.Count == 0)
                {
                    // wenn no data inside thean no duplicates
                    return false;
                }
                List<string> temp = data.Distinct().ToList();

                // data larger then temp
                // duplicates existing
                if (temp.Count() == data.Count())
                {
                    return false;
                }

                return true;
            }

        #endregion

    }
}
