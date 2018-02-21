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
using NUnit.Framework;
using System;
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
            Assert.That(ds, Is.Not.Null, "Failed to meet a precondition: a data strcuture is required.");

            var rp = rsm.Repo.Query().First();
            Assert.That(ds, Is.Not.Null, "Failed to meet a precondition: a research plan is required.");

            var mds = mdm.Repo.Query().First();
            Assert.That(ds, Is.Not.Null, "Failed to meet a precondition: a metadata strcuture is required.");

            Dataset dataset = dm.CreateEmptyDataset(ds, rp, mds);

            Assert.That(dataset, Is.Not.Null);
            Assert.That(dataset.Id, Is.GreaterThan(0), "Dataset is not persisted.");
            Assert.That(dataset.LastCheckIOTimestamp, Is.LessThanOrEqualTo(DateTime.UtcNow), "The dataset's timestamp is wrong.");
            Assert.That(dataset.DataStructure, Is.Not.Null, "Dataset must have a data structure.");
            Assert.That(dataset.Status, Is.EqualTo(DatasetStatus.CheckedIn), "Dataset must be in CheckedIn status.");
        }

    }

}
