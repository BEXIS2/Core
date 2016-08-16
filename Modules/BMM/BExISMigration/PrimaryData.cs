using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using BExIS.Dlm.Entities.Data;
using BExIS.Dlm.Services.Data;
using BExIS.Dlm.Entities.DataStructure;
using BExIS.Dlm.Services.DataStructure;
//using BExIS.Dlm.Entities.Administration;
//using BExIS.Dlm.Services.Administration;
//using BExIS.Dlm.Entities.MetadataStructure;
//using BExIS.Dlm.Services.MetadataStructure;
//using BExIS.Security.Entities.Authorization;
//using BExIS.Security.Services.Authorization;
using BExIS.Security.Entities.Subjects;
using BExIS.Security.Services.Subjects;
//using BExIS.Security.Entities.Objects;
//using BExIS.Security.Services.Objects;
using IBM.Data.DB2;
using IBM.Data.DB2Types;
using BExIS.Xml.Helpers;

namespace BExISMigration
{
    public class PrimaryData
    {
        /// <summary>
        /// CAUTION !!!!!!!!!!!!!!!!!
        /// upload bezieht sich derzeit nur auf daten mit einem block (einer tabelle)
        /// umsetzung von mehreren blöckes noch nicht geklärt.
        /// (für einen wert gibt es eine weitere tabelle)
        /// </summary>
        /// <param name="dataSetID"></param>
        /// <param name="DataBase"></param>
        public void uploadData(string dataSetID, string DataBase)
        {
            DatasetManager datasetManager = new DatasetManager();
            DataStructureManager dataStructureManager = new DataStructureManager();
            XmlDataReader xmlDataReader = new XmlDataReader();

            User user = new User();
            string variableNames = "";
            // query metadataAuthor and variable names from explorer.datasets
            queryAuthorAndVariables(ref user, ref variableNames, dataSetID, DataBase);
            List<string> varNames = variableNames.Split(',').ToList();

            // get all dataStructures with equal variables count
            List<StructuredDataStructure> dataStructures = dataStructureManager.StructuredDataStructureRepo.Get(s =>
                varNames.Count().Equals(s.Variables.Count)).ToList();

            // get all Ids of dataStructures with equal variables
            List<long> dataStructureIds = new List<long>();
            foreach (StructuredDataStructure dataStructure in dataStructures)
            {
                bool isSimilarStructure = true;
                foreach (Variable variable in dataStructure.Variables)
                {
                    if (!varNames.Contains(variable.Label))
                    {
                        isSimilarStructure &= false;
                        break;
                    }
                }
                if (isSimilarStructure)
                    dataStructureIds.Add(dataStructure.Id);
            }

            // get the wanted dataset by comparing the old B1 datasetId out of the datasets with similar dataStructure
            Dataset dataset = null;
            List<Dataset> datasets = datasetManager.DatasetRepo.Get(d => dataStructureIds.Contains(d.DataStructure.Id)).ToList();
            foreach (Dataset ds in datasets)
            {
                string oldDatasetId = "";
                try
                {
                    XmlNode extraID = ds.Versions.FirstOrDefault().Metadata.SelectSingleNode("Metadata/general/general/id/id");
                    oldDatasetId = extraID.InnerText;
                }
                catch
                {
                }
                if (oldDatasetId == dataSetID)
                {
                    dataset = ds;
                    break;
                }
            }

            if (dataset != null)
            {
                // get distinct and ascending ordered insertdates from DB
                List<DB2TimeStamp> distInsertDates = queryDistInsertDates(dataSetID, DataBase);

                bool checkObsIds = false;
                foreach (DB2TimeStamp insertDate in distInsertDates)
                {
                    List<DataTuple> createdDataTuples = new List<DataTuple>();
                    List<DataTuple> editedDataTuples = new List<DataTuple>();
                    List<DataTuple> deletedDataTuples = new List<DataTuple>();

                    DatasetVersion workingCopy = datasetManager.GetDatasetLatestVersion(dataset.Id); // get dataset
                    // get obsid, data, deleted and newest for each observation from DB
                    List<Observation> observations = queryObservation(dataSetID, DataBase, insertDate.ToString());
                    Dictionary<long, long> obsIdMapsToTupleId = new Dictionary<long, long>();
                    if (checkObsIds)
                    {
                        // get all EffectiveTupleIds
                        List<long> datasetTupleIds = datasetManager.GetDatasetVersionEffectiveTupleIds(workingCopy).ToList();
                        // id-mapping-list key=obsId, value=TupleId foreach EffectiveTupleId and obsId from Tuple.Extra
                        obsIdMapsToTupleId = idMapping(datasetTupleIds, ref datasetManager);
                    }
                    // observation counter
                    int observationIndex = 0;
                    bool isNewest = true;
                    foreach (Observation observation in observations) //////////////parallel
                    {
                        isNewest = (isNewest && observation.newest != 'Y') ? false : isNewest;
                        // create dataTuple with xmlDataReader
                        // split xml to string list and use DataReader.ReadRow
                        DataTuple dataRow = xmlDataReader.XmlRowReader(observation.data, dataset.DataStructure.Id, observationIndex);
                        // check if observation.obsid is in id-mapping-list
                        long TupleId;
                        if (checkObsIds && obsIdMapsToTupleId.TryGetValue(observation.obsid, out TupleId))
                        {
                            DataTuple dataTuple = datasetManager.DataTupleRepo.Get(TupleId);
                            if (observation.deleted != 'Y')
                            {
                                dataTuple.VariableValues = dataRow.VariableValues;
                                // edit tuple if observation exists as tuple in EffectiveTuple and observation is not deleted
                                editedDataTuples.Add(dataTuple);
                            }
                            else
                            {
                                // delete tuple if observation exists as tuple in EffectiveTuple and observ. is deleted
                                deletedDataTuples.Add(dataTuple);
                            }
                        }
                        else
                        {
                            // write the obsId and oldBExISdatasetId in Extra: <extra><obsid>[obsid]</obsid><oldBExISdatasetId>[dataSetID]</oldBExISdatasetId></extra>
                            dataRow.Extra = oldIdsIntoExtra(observation.obsid, dataSetID, dataRow.Extra);
                            // create tuple if observation exists not in EffectiveTuple
                            createdDataTuples.Add(dataRow);
                        }
                        observationIndex++; // observation counter
                    }
                    checkObsIds = (!checkObsIds && !isNewest) ? true : checkObsIds;

                    // checkOut
                    if (datasetManager.IsDatasetCheckedOutFor(dataset.Id, user.Name) || datasetManager.CheckOutDataset(dataset.Id, user.Name))
                    {
                        workingCopy = datasetManager.GetDatasetWorkingCopy(dataset.Id); // get dataset
                        datasetManager.EditDatasetVersion(workingCopy, createdDataTuples, editedDataTuples, deletedDataTuples); // edit dataset
                        datasetManager.CheckInDataset(dataset.Id, "Primary data row was submited.", user.Name); // checkIn
                    }
                }
            }
        }

        private List<Observation> queryObservation(string dataSetID, string DataBase, string insertDateStrg)
        {
            List<Observation> observations = new List<Observation>();

            string mySelectQuery = "select obsid, data, deleted, newest";
            mySelectQuery += " from explorer.observation";
            mySelectQuery += " where datasetid = " + dataSetID;
            mySelectQuery += " and insertdate = '" + insertDateStrg + "';";
            DB2Connection connect = new DB2Connection(DataBase);
            DB2Command myCommand = new DB2Command(mySelectQuery, connect);
            connect.Open();
            DB2DataReader myReader = myCommand.ExecuteReader();
            while (myReader.Read())
            {
                Observation observation = new Observation();
                observation.obsid = myReader.GetInt64(0);
                observation.data = new XmlDocument();
                observation.data.LoadXml(myReader.GetString(1));
                observation.deleted = myReader.GetChar(2);
                observation.newest = myReader.GetChar(3);
                observations.Add(observation);
            }
            myReader.Close();
            connect.Close();

            return observations;
        }

        public void queryAuthorAndVariables(ref User user, ref string variableNames, string dataSetID, string DataBase)
        {
            string mySelectQuery = "select X.* from explorer.datasets, XMLTABLE ('$METADATA/*:metaProfile' Columns " +
                                        "Author varchar(256) Path '*:general/*:metadataCreator'," +
                                        "VarNames varchar(1028) Path 'string-join(*:data/*:dataStructure/*:variables/*:variable/*:name,\",\")'" +
                                    ") as X where datasetid = " + dataSetID + ";";

            DB2Connection connect = new DB2Connection(DataBase);
            DB2Command myCommand = new DB2Command(mySelectQuery, connect);
            connect.Open();
            DB2DataReader myReader = myCommand.ExecuteReader();

            string author = "";
            while (myReader.Read())
            {
                author = myReader.GetString(0);
                variableNames = myReader.GetString(1);
            }
            myReader.Close();
            connect.Close();

            SubjectManager subjectManager = new SubjectManager();
            user = subjectManager.UsersRepo.Get(u => author.Equals(u.FullName)).FirstOrDefault();
        }

        private List<DB2TimeStamp> queryDistInsertDates(string dataSetID, string DataBase)
        {
            List<DB2TimeStamp> distInsertDates = new List<DB2TimeStamp>();

            string mySelectQuery = "select distinct insertdate";
            mySelectQuery += " from explorer.observation where datasetid = " + dataSetID + " order by insertdate asc;";
            DB2Connection connect = new DB2Connection(DataBase);
            DB2Command myCommand = new DB2Command(mySelectQuery, connect);
            connect.Open();
            DB2DataReader myReader = myCommand.ExecuteReader();
            while (myReader.Read())
            {
                distInsertDates.Add(myReader.GetDB2TimeStamp(0));
            }
            myReader.Close();
            connect.Close();

            return distInsertDates;
        }

        private Dictionary<long, long> idMapping(List<long> TupleIdList, ref DatasetManager datasetManager)
        {
            Dictionary<long, long> obsIdMapsToTupleId = new Dictionary<long, long>();

            foreach (long datasetTupleId in TupleIdList)
            {
                DataTuple datasetTuple = datasetManager.DataTupleRepo.Get(datasetTupleId);
                if (datasetTuple.Extra != null && XmlUtility.GetXmlNodeByName(datasetTuple.Extra, "obsid") != null)
                {
                    try
                    {
                        long id = datasetTuple.Id;
                        long obsid = long.Parse(XmlUtility.GetXmlNodeByName(datasetTuple.Extra, "obsid").InnerText);
                        obsIdMapsToTupleId.Add(obsid, id);
                    }
                    catch (Exception ex)
                    {
                        throw new Exception(ex.Message);
                    }
                }
                //if (datasetTuple.Extra != null)
                //{
                //    XmlNode extraNode = datasetTuple.Extra.FirstChild;
                //    foreach (XmlNode extraChild in extraNode)
                //    {
                //        if (extraChild.Name == "obsid")
                //        {
                //            long id = datasetTuple.Id;
                //            long obsid = long.Parse(extraChild.InnerText);
                //            obsIdMapsToTupleId.Add(obsid, id);
                //        }
                //    }
                //}
            }

            return obsIdMapsToTupleId;
        }


        /// <summary>
        ///  zusätzlich von bexis 1 genutzte informationen wie bsp obsID 
        ///  wird aus wichtigen gründen in dem extra bereich im xml doc gespeichert.
        /// </summary>
        /// <param name="obsId"></param>
        /// <param name="dataSetID"></param>
        /// <param name="existExtra"></param>
        /// <returns></returns>
        private XmlDocument oldIdsIntoExtra(long obsId, string dataSetID, XmlNode existExtra)
        {
            XmlDocument newExtra = new XmlDocument();
            if (existExtra != null)
            {
                newExtra.LoadXml(existExtra.OuterXml);
            }
            else
            {
                XmlElement root = newExtra.CreateElement("extra");
                newExtra.AppendChild(root);
            }
            XmlElement obsid = newExtra.CreateElement("obsid");
            obsid.InnerText = obsId.ToString();
            newExtra.DocumentElement.AppendChild(obsid);
            XmlElement datid = newExtra.CreateElement("oldBExISdatasetId");
            datid.InnerText = dataSetID;
            newExtra.DocumentElement.AppendChild(datid);

            return newExtra;
        }
    }
}
