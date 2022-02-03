using BExIS.App.Testing;
using BExIS.Dlm.Entities.Data;
using BExIS.Dlm.Entities.DataStructure;
using BExIS.Dlm.Services.Administration;
using BExIS.Dlm.Services.Data;
using BExIS.Dlm.Services.MetadataStructure;
using BExIS.Dlm.Tests.Helpers;
using BExIS.Utils;
using BExIS.Utils.Config;
using BExIS.Utils.Upload;
using FluentAssertions;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vaiona.Persistence.Api;

namespace BExIS.Dlm.Tests.Services.Data
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Reliability", "CA2000:Objekte verwerfen, bevor Bereich verloren geht", Justification = "<Ausstehend>")]
    [TestFixture()]
    public class DatasetManager_GetDatasetIdsTests
    {
        private TestSetupHelper helper = null;
        private long datasetId = 0;
        private long latestDataTupleId = 0;
        private string username = "David";
        private long numberOfTuples = 10;
        private DatasetHelper dsHelper;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            helper = new TestSetupHelper(WebApiConfig.Register, false);
            var dm = new DatasetManager();
            var rsm = new ResearchPlanManager();
            var mdm = new MetadataStructureManager();
            dsHelper = new DatasetHelper();

            dsHelper.PurgeAllDatasets();
            dsHelper.PurgeAllDataStructures();
            dsHelper.PurgeAllResearchPlans();

            // generate Data
            StructuredDataStructure dataStructure = dsHelper.CreateADataStructure();
            dataStructure.Should().NotBeNull("Failed to meet a precondition: a data strcuture is required.");

            var rp = dsHelper.CreateResearchPlan();
            rp.Should().NotBeNull("Failed to meet a precondition: a research plan is required.");

            var mds = mdm.Repo.Query().First();
            mds.Should().NotBeNull("Failed to meet a precondition: a metadata strcuture is required.");

            // create 10 Datasets
            for (int i = 0; i < 10; i++)
            {
                Dataset dataset = dm.CreateEmptyDataset(dataStructure, rp, mds);
                datasetId = dataset.Id;
                dataset = dsHelper.GenerateTuplesForDataset(dataset, dataStructure, numberOfTuples, "David");
                dataset.Should().NotBeNull("The dataset tuple generation has failed!");
                dm.CheckInDataset(dataset.Id, "for testing purposes 2", "David", ViewCreationBehavior.None);
                dm.SyncView(dataset.Id, ViewCreationBehavior.Create | ViewCreationBehavior.Refresh);
            }
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            var dsHelper = new DatasetHelper();

            dsHelper.PurgeAllDatasets();
            dsHelper.PurgeAllDataStructures();
            dsHelper.PurgeAllResearchPlans();
            helper.Dispose();
        }

        [Test()]
        public void GetDatasetIds_AllCheckedIn_ReturnListOfIds()
        {
            DatasetManager datasetManager = new DatasetManager();

            try
            {
                //Arrange
                // get count all datasets in the database
                var count = datasetManager.DatasetRepo.Get().Count;

                //Act
                var checkedInCount = datasetManager.GetDatasetIds().Count;

                //Assert
                Assert.That(checkedInCount, Is.EqualTo(count));
                Assert.That(checkedInCount, Is.EqualTo(10));
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                datasetManager.Dispose();
            }
        }
    }
}