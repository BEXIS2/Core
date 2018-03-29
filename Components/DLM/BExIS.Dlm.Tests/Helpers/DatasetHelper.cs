using BExIS.Dlm.Entities.Data;
using BExIS.Dlm.Entities.DataStructure;
using BExIS.Dlm.Services.Data;
using BExIS.Dlm.Services.DataStructure;
using FluentAssertions;
using System;
using System.Collections.Generic;
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
                manager.DeleteStructuredDataStructure(sts);

                var unsts = manager.UnStructuredDataStructureRepo.Query().ToList();
                manager.DeleteUnStructuredDataStructure(unsts);
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

                StructuredDataStructure dataStructure = dsManager.CreateStructuredDataStructure("dsForTesting", "DS for unit testing", "", "", Dlm.Entities.DataStructure.DataStructureCategory.Generic);
                dsManager.AddVariableUsage(dataStructure, dataAttribute1, true, "var1UT", "", "", "Used for unit testing");
                dsManager.AddVariableUsage(dataStructure, dataAttribute2, true, "var2UT", "", "", "Used for unit testing");
                return dataStructure;
            }
            catch { return null; }
            finally
            {
                unitManager.Dispose();
                dataTypeManager.Dispose();
                attributeManager.Dispose();
                dsManager.Dispose();
            }
        }

        public Dataset GenerateTuplesForDataset(Dataset dataset, StructuredDataStructure dataStructure, long numberOfTuples)
        {
            dataset.Status.Should().Be(DatasetStatus.CheckedIn);
            dataset.Should().NotBeNull();
            numberOfTuples.Should().BeGreaterThan(0);

            DatasetManager dm = new DatasetManager();
            try
            {
                if (dm.IsDatasetCheckedOutFor(dataset.Id, "Javad") || dm.CheckOutDataset(dataset.Id, "Javad"))
                {
                    dataset.Status.Should().Be(DatasetStatus.CheckedOut, "Dataset must be in Checkedout status.");

                    DatasetVersion workingCopy = dm.GetDatasetWorkingCopy(dataset.Id);

                    DataTuple dt = new DataTuple();
                    dt.VariableValues.Add(new VariableValue() { VariableId = dataStructure.Variables.First().Id, Value = 22 });
                    dt.VariableValues.Add(new VariableValue() { VariableId = dataStructure.Variables.Skip(1).First().Id, Value = "Test" });
                    dt.Dematerialize();

                    dt.Should().NotBeNull();
                    dt.XmlVariableValues.Should().NotBeNull();

                    List<DataTuple> tuples = new List<DataTuple>();

                    for (int i = 0; i < numberOfTuples; i++)
                    {
                        DataTuple newDt = new DataTuple();
                        newDt.XmlAmendments = dt.XmlAmendments;
                        newDt.XmlVariableValues = dt.XmlVariableValues; 
                        newDt.Materialize();
                        newDt.OrderNo = i;
                        tuples.Add(newDt);
                    }
                    dm.EditDatasetVersion(workingCopy, tuples, null, null);
                    dataset.Status.Should().Be(DatasetStatus.CheckedOut, "Dataset must be in Checkedout status.");
                }
                return dataset;
            }
            catch { return null; }
            finally
            {
                dm.Dispose();
            }
        }

    }
}
