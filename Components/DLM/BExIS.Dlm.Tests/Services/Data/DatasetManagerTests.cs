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


        [Test()]
        public void CreateEmptyDatasetTest()
        {
            DatasetManager dm = new DatasetManager();
            var dsm = new DataStructureManager();
            var rsm = new ResearchPlanManager();
            var mdm = new MetadataStructureManager();

            Dataset dataset = dm.CreateEmptyDataset(dsm.StructuredDataStructureRepo.Query().First(),
                                  rsm.Repo.Query().First(), mdm.Repo.Query().First());

            Assert.That(dataset, Is.Not.Null);
            Assert.That(dataset.Id, Is.GreaterThan(0));
            Assert.That(dataset.LastCheckIOTimestamp, Is.LessThanOrEqualTo(DateTime.UtcNow));
            Assert.That(dataset.DataStructure, Is.Not.Null, "Dataset must have a data structure.");
            Assert.That(dataset.Status, Is.EqualTo(DatasetStatus.CheckedIn), "Dataset must be in CheckedIn status.");
        }

    }

}
