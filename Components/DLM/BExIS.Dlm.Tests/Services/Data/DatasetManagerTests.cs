using BExIS.App.Testing;
using BExIS.Dlm.Entities.Data;
using BExIS.Dlm.Services.Administration;
using BExIS.Dlm.Services.Data;
using BExIS.Dlm.Services.DataStructure;
using BExIS.Dlm.Services.MetadataStructure;
using BExIS.Utils.Config;
using BExIS.Utils.Data.Helpers;
using BExIS.Web.Shell;
using BExIS.Web.Shell.Helpers;
using FluentAssertions;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BExIS.Dlm.Tests.Services.Data
{    
    public class DatasetManagerTests
    {
        private TestSetupHelper helper = null;
        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            helper = new TestSetupHelper(WebApiConfig.Register, false);
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            helper.Dispose();
        }

        [Test()]
        public void CreateEmptyDatasetTest()
        {
            DatasetManager dm = new DatasetManager();
            var dsm = new DataStructureManager();
            var rsm = new ResearchPlanManager();
            var mdm = new MetadataStructureManager();

            var ds = dsm.StructuredDataStructureRepo.Query().First();
            ds.Should().NotBeNull("Failed to meet a precondition: a data strcuture is required.");

            var rp = rsm.Repo.Query().First();
            rp.Should().NotBeNull("Failed to meet a precondition: a research plan is required.");

            var mds = mdm.Repo.Query().First();
            mds.Should().NotBeNull("Failed to meet a precondition: a metadata strcuture is required.");

            Dataset dataset = dm.CreateEmptyDataset(ds, rp, mds);

            dataset.Should().NotBeNull();
            dataset.Id.Should().BeGreaterThan(0, "Dataset is not persisted.");
            dataset.LastCheckIOTimestamp.Should().NotBeAfter(DateTime.UtcNow, "The dataset's timestamp is wrong.");
            dataset.DataStructure.Should().NotBeNull("Dataset must have a data structure.");
            dataset.Status.Should().Be(DatasetStatus.CheckedIn, "Dataset must be in CheckedIn status.");

            dm.PurgeDataset(dataset.Id);
        }

        [Test()]
        public void DeleteDatasetTest()
        {
            DatasetManager dm = new DatasetManager();
            var dsm = new DataStructureManager();
            var rsm = new ResearchPlanManager();
            var mdm = new MetadataStructureManager();

            var ds = dsm.StructuredDataStructureRepo.Query().First();
            ds.Should().NotBeNull("Failed to meet a precondition: a data strcuture is required.");

            var rp = rsm.Repo.Query().First();
            rp.Should().NotBeNull("Failed to meet a precondition: a research plan is required.");

            var mds = mdm.Repo.Query().First();
            mds.Should().NotBeNull("Failed to meet a precondition: a metadata strcuture is required.");

            Dataset dataset = dm.CreateEmptyDataset(ds, rp, mds);
            dm.DeleteDataset(dataset.Id, "Javad", false);

            dataset.Should().NotBeNull();
            dataset.Id.Should().BeGreaterThan(0, "Dataset is not persisted.");
            dataset.LastCheckIOTimestamp.Should().NotBeAfter(DateTime.UtcNow, "The dataset's timestamp is wrong.");
            dataset.DataStructure.Should().NotBeNull("Dataset must have a data structure.");
            dataset.Status.Should().Be(DatasetStatus.Deleted, "Dataset must be in Deleted status.");

            dm.PurgeDataset(dataset.Id);
        }


        [Test()]
        public void CreateDatasetVersionTest()
        {
            long numberOfTuples = 1000;
            DatasetManager dm = new DatasetManager();
            var dsm = new DataStructureManager();
            var rsm = new ResearchPlanManager();
            var mdm = new MetadataStructureManager();

            var dss = dsm.StructuredDataStructureRepo.Query().First();
            dss.Should().NotBeNull("Failed to meet a precondition: a data strcuture is required.");

            var rp = rsm.Repo.Query().First();
            rp.Should().NotBeNull("Failed to meet a precondition: a research plan is required.");

            var mds = mdm.Repo.Query().First();
            mds.Should().NotBeNull("Failed to meet a precondition: a metadata strcuture is required.");

            Dataset dataset = dm.CreateEmptyDataset(dss, rp, mds);

            if (dm.IsDatasetCheckedOutFor(dataset.Id, "Javad") || dm.CheckOutDataset(dataset.Id, "Javad"))
            {
                dataset.Status.Should().Be(DatasetStatus.CheckedOut, "Dataset must be in Checkedout status.");

                DatasetVersion workingCopy = dm.GetDatasetWorkingCopy(dataset.Id);

                DataTuple dt = dm.DataTupleRepo.Query().First(); // its sample data, should be generated properly for the test
                dt.Should().NotBeNull();
                dt.XmlVariableValues.Should().NotBeNull();

                List<DataTuple> tuples = new List<DataTuple>();

                for (int i = 0; i < numberOfTuples; i++)
                {
                    DataTuple newDt = new DataTuple();
                    newDt.XmlAmendments = dt.XmlAmendments;
                    newDt.XmlVariableValues = dt.XmlVariableValues; // in normal cases, the VariableValues are set and then Dematerialize is called
                    newDt.Materialize();
                    newDt.OrderNo = i;
                    //newDt.TupleAction = TupleAction.Created;//not required
                    //newDt.Timestamp = DateTime.UtcNow; //required? no, its set in the Edit
                    //newDt.DatasetVersion = workingCopy;//required? no, its set in the Edit
                    tuples.Add(newDt);
                }
                dm.EditDatasetVersion(workingCopy, tuples, null, null);
                dataset.Status.Should().Be(DatasetStatus.CheckedOut, "Dataset must be in Checkedout status.");
                
                //dm.CheckInDataset(ds.Id, "for testing purposes 1", "Javad", ViewCreationBehavior.Create | ViewCreationBehavior.Refresh);
                dm.CheckInDataset(dataset.Id, "for testing purposes 2", "Javad", ViewCreationBehavior.None);
                //dm.SyncView(ds.Id, ViewCreationBehavior.Create);
                //dm.SyncView(ds.Id, ViewCreationBehavior.Refresh);
                dm.SyncView(dataset.Id, ViewCreationBehavior.Create | ViewCreationBehavior.Refresh);

                dataset.Should().NotBeNull();
                dataset.Id.Should().BeGreaterThan(0, "Dataset is not persisted.");
                dataset.LastCheckIOTimestamp.Should().NotBeAfter(DateTime.UtcNow, "The dataset's timestamp is wrong.");
                dataset.DataStructure.Should().NotBeNull("Dataset must have a data structure.");
                dataset.Status.Should().Be(DatasetStatus.CheckedIn, "Dataset must be in CheckedIn status.");
                dm.GetDatasetLatestVersionEffectiveTupleCount(dataset.Id).Should().Be(numberOfTuples);
            }

            dm.DatasetVersionRepo.Evict();
            dm.DataTupleRepo.Evict();
            dm.DatasetRepo.Evict();
            dm.PurgeDataset(dataset.Id, true);
        }

    }

}
