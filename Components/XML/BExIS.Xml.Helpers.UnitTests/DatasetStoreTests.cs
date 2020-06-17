using BExIS.App.Testing;
using BExIS.Dlm.Entities.DataStructure;
using BExIS.Dlm.Services.Data;
using BExIS.Dlm.Services.MetadataStructure;
using BExIS.Utils.Config;
using BExIS.Xml.Helpers.UnitTests.Helpers;
using FluentAssertions;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BExIS.Xml.Helpers.UnitTests
{
    public class DatasetStoreTests
    {
        private int _numberOfDatasets;
        private TestSetupHelper helper = null;

        [OneTimeSetUp]
        /// It is called once prior to executing any of the tests in a fixture.
        /// Multiple methods can be marked. Order is not preserved.
        /// Inheritance is supported, call sequence form the parents
        public void OneTimeSetUp()
        {
            helper = new TestSetupHelper(WebApiConfig.Register, false);
            var dsHelper = new DatasetHelper();
            dsHelper.PurgeAllDatasets();
            dsHelper.PurgeAllDataStructures();
            dsHelper.PurgeAllResearchPlans();

            // gerenate datasets
            var mdm = new MetadataStructureManager();

            StructuredDataStructure dataStructure = dsHelper.CreateADataStructure();
            dataStructure.Should().NotBeNull("Failed to meet a precondition: a data strcuture is required.");

            var rp = dsHelper.CreateResearchPlan();
            rp.Should().NotBeNull("Failed to meet a precondition: a research plan is required.");

            var mds = mdm.Repo.Query().First();
            mds.Should().NotBeNull("Failed to meet a precondition: a metadata strcuture is required.");

            DatasetManager datasetManager = new DatasetManager();

            // because these tests are working on in-memory objects (datasets) only, there is no need to do the test app setup
            _numberOfDatasets = 1000;

            for (int i = 0; i < _numberOfDatasets; i++)
            {
                datasetManager.CreateEmptyDataset(dataStructure, rp, mds);
            }
        }

        [SetUp]
        /// performs the initial setup for the tests. This runs once per test, NOT per class!
        protected void SetUp()
        {
        }

        [TearDown]
        /// performs the cleanup after each test
        public void TearDown()
        {
        }

        [OneTimeTearDown]
        /// It is called once after executing all the tests in a fixture.
        /// Multiple methods can be marked. Order is not preserved.
        /// Inheritance is supported, call sequence form the children
        /// Executes only if: counterpart OneTimeSetUp exists and executed successfully.
        public void OneTimeTearDown()
        {
            var dsHelper = new DatasetHelper();
            dsHelper.PurgeAllDatasets();
            dsHelper.PurgeAllDataStructures();
            dsHelper.PurgeAllResearchPlans();
            helper.Dispose();
        }

        [Test()]
        public void GetEntities_ValidCall_ReturnList<EntityStoreItem>()
        {
            //Arrange
            DatasetStore datasetStore = new DatasetStore();

            //Act
            var datasets = datasetStore.GetEntities();

            //Assert
            Assert.IsNotNull(datasets);
            Assert.IsNotEmpty(datasets);
            Assert.That(datasets.Count, Is.EqualTo(_numberOfDatasets));
        }
    }
}