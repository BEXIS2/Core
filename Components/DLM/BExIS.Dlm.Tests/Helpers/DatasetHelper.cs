using BExIS.Dlm.Entities.Administration;
using BExIS.Dlm.Entities.Data;
using BExIS.Dlm.Entities.DataStructure;
using BExIS.Dlm.Services.Administration;
using BExIS.Dlm.Services.Data;
using BExIS.Dlm.Services.DataStructure;
using FluentAssertions;
using NHibernate.Id.Insert;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BExIS.Dlm.Tests.Helpers
{
    public class DatasetHelper
    {
        public void PurgeAllDatasets()
        {
            DatasetManager dm = new DatasetManager();
            try
            {
                dm.GetDatasetLatestIds(true).ForEach(dsId => dm.PurgeDataset(dsId));
            }
            finally
            {
                dm.Dispose();
            }
        }

        public void PurgeAllDataStructures()
        {
            var manager = new DataStructureManager();
            try
            {
                var sts = manager.StructuredDataStructureRepo.Query().ToList();
                sts.ForEach(p => manager.DeleteStructuredDataStructure(p));
                //manager.DeleteStructuredDataStructure(sts); // Does not delete all the entities of the list!!!! Javad, 29.08.2018

                var unsts = manager.UnStructuredDataStructureRepo.Query().ToList();
                unsts.ForEach(p => manager.DeleteUnStructuredDataStructure(p));
                //manager.DeleteUnStructuredDataStructure(unsts);
            }
            finally
            {
                manager.Dispose();
            }
        }

        public void PurgeAllResearchPlans()
        {
            var manager = new ResearchPlanManager();
            try
            {
                var plans = manager.Repo.Query().ToList();
                manager.Delete(plans);
            }
            finally
            {
                manager.Dispose();
            }
        }

        public StructuredDataStructure CreateADataStructure()
        {
            var unitManager = new UnitManager();
            var dataTypeManager = new DataTypeManager();
            var attributeManager = new DataContainerManager();
            var dsManager = new DataStructureManager();
            try
            {
                var dim = unitManager.Create("TestDimnesion", "For Unit Testing", "");
                var unit = unitManager.Create("None_UT", "NoneUT", "Use in unit tsting", dim, Dlm.Entities.DataStructure.MeasurementSystem.Metric);

                var intType = dataTypeManager.Create("Integer", "Integer", TypeCode.Int32);
                var strType = dataTypeManager.Create("String", "String", TypeCode.String);
                var doubleType = dataTypeManager.Create("Double", "Double", TypeCode.Double);
                var boolType = dataTypeManager.Create("Bool", "Bool", TypeCode.Boolean);
                var dateTimeType = dataTypeManager.Create("DateTime", "DateTime", TypeCode.DateTime);

                var dataAttribute1 = attributeManager.CreateDataAttribute(
                    "att1UT", "att1UT", "Attribute for Unit testing",
                    false, false, "", Dlm.Entities.DataStructure.MeasurementScale.Nominal, Dlm.Entities.DataStructure.DataContainerType.ValueType,
                    "", intType, unit,
                    null, null, null, null, null, null
                    );

                var dataAttribute2 = attributeManager.CreateDataAttribute(
                    "att2UT", "att1UT", "Attribute for Unit testing",
                    false, false, "", Dlm.Entities.DataStructure.MeasurementScale.Nominal, Dlm.Entities.DataStructure.DataContainerType.ValueType,
                    "", strType, unit,
                    null, null, null, null, null, null
                    );

                var dataAttribute3 = attributeManager.CreateDataAttribute(
                    "att3UT", "att3UT", "Attribute for Unit testing",
                    false, false, "", Dlm.Entities.DataStructure.MeasurementScale.Nominal, Dlm.Entities.DataStructure.DataContainerType.ValueType,
                    "", doubleType, unit,
                    null, null, null, null, null, null
                    );

                var dataAttribute4 = attributeManager.CreateDataAttribute(
                    "att4UT", "att4UT", "Attribute for Unit testing",
                    false, false, "", Dlm.Entities.DataStructure.MeasurementScale.Nominal, Dlm.Entities.DataStructure.DataContainerType.ValueType,
                    "", boolType, unit,
                    null, null, null, null, null, null
                    );

                var dataAttribute5 = attributeManager.CreateDataAttribute(
                    "att5UT", "att5UT", "Attribute for Unit testing",
                    false, false, "", Dlm.Entities.DataStructure.MeasurementScale.Nominal, Dlm.Entities.DataStructure.DataContainerType.ValueType,
                    "", dateTimeType, unit,
                    null, null, null, null, null, null
                    );

                StructuredDataStructure dataStructure = dsManager.CreateStructuredDataStructure("dsForTesting", "DS for unit testing", "", "", Dlm.Entities.DataStructure.DataStructureCategory.Generic);
                dsManager.AddVariableUsage(dataStructure, dataAttribute1, true, "var1UT", "", "", "Used for unit testing");
                dsManager.AddVariableUsage(dataStructure, dataAttribute2, true, "var2UT", "", "", "Used for unit testing");
                dsManager.AddVariableUsage(dataStructure, dataAttribute3, true, "var3UT", "", "", "Used for unit testing");
                dsManager.AddVariableUsage(dataStructure, dataAttribute4, true, "var4UT", "", "", "Used for unit testing");
                dsManager.AddVariableUsage(dataStructure, dataAttribute5, true, "var5UT", "", "", "Used for unit testing");
                return dataStructure;
            }
            catch(Exception ex) { return null; }
            finally
            {
                unitManager.Dispose();
                dataTypeManager.Dispose();
                attributeManager.Dispose();
                dsManager.Dispose();
            }
        }

        public Dataset GenerateTuplesForDataset(Dataset dataset, StructuredDataStructure dataStructure, long numberOfTuples,string username)
        {
            dataset.Status.Should().Be(DatasetStatus.CheckedIn);
            dataset.Should().NotBeNull();
            numberOfTuples.Should().BeGreaterThan(0);

            var r = new Random();

            DatasetManager dm = new DatasetManager();
            try
            {
                if (dm.IsDatasetCheckedOutFor(dataset.Id, username) || dm.CheckOutDataset(dataset.Id, username))
                {
                    dataset.Status.Should().Be(DatasetStatus.CheckedOut, "Dataset must be in Checkedout status.");

                    DatasetVersion workingCopy = dm.GetDatasetWorkingCopy(dataset.Id);

                    DataTuple dt = new DataTuple();
                    dt.VariableValues.Add(new VariableValue() { VariableId = dataStructure.Variables.First().Id, Value = r.Next()});
                    dt.VariableValues.Add(new VariableValue() { VariableId = dataStructure.Variables.Skip(1).First().Id, Value = "Test" });
                    dt.VariableValues.Add(new VariableValue() { VariableId = dataStructure.Variables.Skip(2).First().Id, Value = Convert.ToDouble(r.Next()) });
                    dt.VariableValues.Add(new VariableValue() { VariableId = dataStructure.Variables.Skip(3).First().Id, Value =  true});
                    dt.VariableValues.Add(new VariableValue() { VariableId = dataStructure.Variables.Skip(4).First().Id, Value = "01.01.2017" });
                    dt.Dematerialize();

                    dt.Should().NotBeNull();
                    //dt.XmlVariableValues.Should().NotBeNull();

                    List<DataTuple> tuples = new List<DataTuple>();

                    for (int i = 0; i < numberOfTuples; i++)
                    {
                        DataTuple newDt = new DataTuple();
                        newDt.XmlAmendments = dt.XmlAmendments;
                        //newDt.XmlVariableValues = dt.XmlVariableValues;
                        newDt.JsonVariableValues = dt.JsonVariableValues;
                        newDt.Materialize();
                        newDt.OrderNo = i;
                        tuples.Add(newDt);
                    }
                    dm.EditDatasetVersion(workingCopy, tuples, null, null);
                    dataset.Status.Should().Be(DatasetStatus.CheckedOut, "Dataset must be in Checkedout status.");
                }
                return dataset;
            }
            catch (Exception ex) {
                return null;
            }
            finally
            {
                dm.Dispose();
            }
        }

        public Dataset GenerateTuplesWithRandomValuesForDataset(Dataset dataset, StructuredDataStructure dataStructure, long numberOfTuples, string username)
        {
            dataset.Status.Should().Be(DatasetStatus.CheckedIn);
            dataset.Should().NotBeNull();
            numberOfTuples.Should().BeGreaterThan(0);

            var r = new Random();

            DatasetManager dm = new DatasetManager();
            try
            {
                if (dm.IsDatasetCheckedOutFor(dataset.Id, username) || dm.CheckOutDataset(dataset.Id, username))
                {
                    dataset.Status.Should().Be(DatasetStatus.CheckedOut, "Dataset must be in Checkedout status.");

                    DatasetVersion workingCopy = dm.GetDatasetWorkingCopy(dataset.Id);

                    

                    List<DataTuple> tuples = new List<DataTuple>();

                    for (int i = 0; i < numberOfTuples; i++)
                    {
                        DataTuple dt = new DataTuple();
                        dt.VariableValues.Add(new VariableValue() { VariableId = dataStructure.Variables.First().Id, Value = r.Next() });
                        dt.VariableValues.Add(new VariableValue() { VariableId = dataStructure.Variables.Skip(1).First().Id, Value = "Test" });
                        dt.VariableValues.Add(new VariableValue() { VariableId = dataStructure.Variables.Skip(2).First().Id, Value = Convert.ToDouble(r.Next()) });
                        dt.VariableValues.Add(new VariableValue() { VariableId = dataStructure.Variables.Skip(3).First().Id, Value = true });
                        dt.VariableValues.Add(new VariableValue() { VariableId = dataStructure.Variables.Skip(4).First().Id, Value = "01.01.2017" });
                        dt.Dematerialize();

                        dt.Should().NotBeNull();
                        //dt.XmlVariableValues.Should().NotBeNull();

                        DataTuple newDt = new DataTuple();
                        newDt.XmlAmendments = dt.XmlAmendments;
                        //newDt.XmlVariableValues = dt.XmlVariableValues;
                        newDt.JsonVariableValues = dt.JsonVariableValues;
                        newDt.Materialize();
                        newDt.OrderNo = i;
                        tuples.Add(newDt);
                    }
                    dm.EditDatasetVersion(workingCopy, tuples, null, null);
                    dataset.Status.Should().Be(DatasetStatus.CheckedOut, "Dataset must be in Checkedout status.");
                }
                return dataset;
            }
            catch (Exception ex)
            {
                return null;
            }
            finally
            {
                dm.Dispose();
            }
        }

        public Dataset UpdateOneTupleForDataset(Dataset dataset, StructuredDataStructure dataStructure, long id, int value, DatasetManager datasetManager)
        {
            dataset.Status.Should().Be(DatasetStatus.CheckedIn);
            dataset.Should().NotBeNull();

            try
            {
                if (datasetManager.IsDatasetCheckedOutFor(dataset.Id, "David") || datasetManager.CheckOutDataset(dataset.Id, "David"))
                {
                    dataset.Status.Should().Be(DatasetStatus.CheckedOut, "Dataset must be in Checkedout status.");

                    DatasetVersion workingCopy = datasetManager.GetDatasetWorkingCopy(dataset.Id);

                    DataTuple oldDt = datasetManager.DataTupleRepo.Get(id);

                    DataTuple dt = new DataTuple();
                    dt.VariableValues.Add(new VariableValue() { VariableId = dataStructure.Variables.First().Id, Value = value });
                    dt.VariableValues.Add(new VariableValue() { VariableId = dataStructure.Variables.Skip(1).First().Id, Value = "Test" });
                    dt.VariableValues.Add(new VariableValue() { VariableId = dataStructure.Variables.Skip(2).First().Id, Value = 5 });
                    dt.VariableValues.Add(new VariableValue() { VariableId = dataStructure.Variables.Skip(3).First().Id, Value = true });
                    dt.VariableValues.Add(new VariableValue() { VariableId = dataStructure.Variables.Skip(4).First().Id, Value = DateTime.Now.ToString(new CultureInfo("en-US")) });
                    dt.Dematerialize();

                    dt.Should().NotBeNull();
                    dt.JsonVariableValues.Should().NotBeNull();

                    List<DataTuple> tuples = new List<DataTuple>();


                    DataTuple newDt = new DataTuple();
                    newDt.Id = id;
                    newDt.XmlAmendments = dt.XmlAmendments;
                    newDt.JsonVariableValues = dt.JsonVariableValues;
                    newDt.Materialize();
                    newDt.OrderNo = oldDt.OrderNo;
                    tuples.Add(newDt);

                    datasetManager.EditDatasetVersion(workingCopy, null, tuples, null);
                    dataset.Status.Should().Be(DatasetStatus.CheckedOut, "Dataset must be in Checkedout status.");
                }
                return dataset;
            }
            catch (Exception ex)
            {
                return null;
            }

        }

        public Dataset UpdateAnyTupleForDataset(Dataset dataset, StructuredDataStructure dataStructure, DatasetManager datasetManager)
        {
            dataset.Status.Should().Be(DatasetStatus.CheckedIn);
            dataset.Should().NotBeNull();

            try
            {
                DatasetVersion dsv = datasetManager.GetDatasetLatestVersion(dataset.Id);
                var datatuples = datasetManager.GetDataTuples(dsv.Id);


                if (datasetManager.IsDatasetCheckedOutFor(dataset.Id, "David") || datasetManager.CheckOutDataset(dataset.Id, "David"))
                {
                    dataset.Status.Should().Be(DatasetStatus.CheckedOut, "Dataset must be in Checkedout status.");

                    DatasetVersion workingCopy = datasetManager.GetDatasetWorkingCopy(dataset.Id);

                    List<DataTuple> editedTuples = new List<DataTuple>();

                    foreach (var dataTuple in datatuples)
                    {
                        var vv = dataTuple.VariableValues.Where(v => v.VariableId.Equals(dataStructure.Variables.Skip(4).First().Id)).FirstOrDefault();
                        if (vv != null) vv.Value = DateTime.Now.ToString(new CultureInfo("en-US"));

                        dataTuple.Dematerialize();
                        dataTuple.Should().NotBeNull();
                        //dataTuple.XmlVariableValues.Should().NotBeNull();
                        dataTuple.Materialize();

                        editedTuples.Add((DataTuple)dataTuple);
                    }

                    datasetManager.EditDatasetVersion(workingCopy, null, editedTuples, null);
                    dataset.Status.Should().Be(DatasetStatus.CheckedOut, "Dataset must be in Checkedout status.");
                }

                return dataset;
            }
            catch (Exception ex)
            {
                return null;
            }

        }

        public List<DataTuple> GetUpdatedDatatuples(DatasetVersion datasetVersion, StructuredDataStructure dataStructure, DatasetManager datasetManager)
        {
            datasetVersion.Should().NotBeNull();
            var dataset = datasetVersion.Dataset;
            dataset.Status.Should().Be(DatasetStatus.CheckedIn);
            dataset.Should().NotBeNull();

            try
            {
                var datatuples = datasetManager.GetDataTuples(datasetVersion.Id);
                List<DataTuple> editedTuples = new List<DataTuple>();


                foreach (var dataTuple in datatuples)
                {
                    dataTuple.Materialize();

                    var vv = dataTuple.VariableValues.Where(v => v.VariableId.Equals(dataStructure.Variables.Skip(4).First().Id)).FirstOrDefault();
                    if (vv != null) vv.Value = DateTime.Now.ToString(new CultureInfo("en-US"));

                    dataTuple.Dematerialize();
                    dataTuple.Should().NotBeNull();
                    //dataTuple.XmlVariableValues.Should().NotBeNull();

                    editedTuples.Add((DataTuple)dataTuple);
                }

             

                return editedTuples;
            }
            catch (Exception ex)
            {
                return null;
            }

        }

        /// <summary>
        /// get a updated datatuple on a specific value.
        /// based on the created test datastructure
        /// index:
        /// 0 = int
        /// 1 = text
        /// 2 = double
        /// 3 = bool 
        /// 4 = datetime
        /// </summary>
        /// <param name="source"></param>
        /// <param name="updateVarIndex"></param>
        /// <param name="datasetManager"></param>
        /// <returns></returns>
        public DataTuple  GetUpdatedDatatuple(DataTuple source, int updateVarIndex)
        {

                if (source == null) return null;

                source.Materialize();

                var vv = source.VariableValues[updateVarIndex];
                if (vv != null)
                {
                    switch (updateVarIndex)
                    {

                    case 0://int
                        {
                            vv.Value = new Random().Next();

                            break;
                        }
                    case 1://text
                        {
                            var chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
                            var stringChars = new char[8];
                            var random = new Random();

                            for (int i = 0; i < stringChars.Length; i++)
                            {
                                stringChars[i] = chars[random.Next(chars.Length)];
                            }
           
                            vv.Value = new String(stringChars);

                            break;
                        }
                    case 2://double
                        {
                            vv.Value = new Random().NextDouble();

                            break;
                        }
                    case 3://bool
                        {
                            vv.Value = false;

                            break;
                        }
                    case 4: 
                        { 
                            vv.Value = DateTime.Now.ToString(new CultureInfo("en-US"));

                            break; 
                        }

                        //default:
                    }


                }
                source.Dematerialize();
                source.Should().NotBeNull();

                return source;
        }



        public ResearchPlan CreateResearchPlan()
        {
            ResearchPlanManager researchPlanManager = new ResearchPlanManager();
            try
            {
                return researchPlanManager.Create("ResearchPlan_UT", "Researchplan for unit tests.");
            }
            finally
            {
                researchPlanManager.Dispose();
            }
        }

    }
}
