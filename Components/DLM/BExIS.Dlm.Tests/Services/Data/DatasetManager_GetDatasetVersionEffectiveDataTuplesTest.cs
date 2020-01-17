using BExIS.App.Testing;
using BExIS.Dlm.Entities.Data;
using BExIS.Dlm.Entities.DataStructure;
using BExIS.Dlm.Services.Administration;
using BExIS.Dlm.Services.Data;
using BExIS.Dlm.Services.MetadataStructure;
using BExIS.Dlm.Tests.Helpers;
using BExIS.Utils.Config;
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
    [TestFixture()]
    public class DatasetManager_GetDatasetVersionEffectiveDataTuplesTest
    {
        private TestSetupHelper helper = null;
        private long datasetId = 0;
        private long latestDataTupleId = 0;
        private string username = "David";

        private DatasetHelper dsHelper;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {

            helper = new TestSetupHelper(WebApiConfig.Register, false);
            var dm = new DatasetManager();
            var rsm = new ResearchPlanManager();
            var mdm = new MetadataStructureManager();
            dsHelper = new DatasetHelper();


            try
            {


                dsHelper.PurgeAllDatasets();
                dsHelper.PurgeAllDataStructures();
                dsHelper.PurgeAllResearchPlans();


                // generate Data
                long numberOfTuples = 10000;

                StructuredDataStructure dataStructure = dsHelper.CreateADataStructure();
                dataStructure.Should().NotBeNull("Failed to meet a precondition: a data strcuture is required.");

                var rp = dsHelper.CreateResearchPlan();
                rp.Should().NotBeNull("Failed to meet a precondition: a research plan is required.");

                var mds = mdm.Repo.Query().First();
                mds.Should().NotBeNull("Failed to meet a precondition: a metadata strcuture is required.");

                Dataset dataset = dm.CreateEmptyDataset(dataStructure, rp, mds);
                datasetId = dataset.Id;



                // add datatuples
                dataset = dsHelper.GenerateTuplesForDataset(dataset, dataStructure, numberOfTuples, username);
                dm.CheckInDataset(dataset.Id, "for testing  datatuples with versions", username, ViewCreationBehavior.None);



                //dm.SyncView(ds.Id, ViewCreationBehavior.Create);
                //dm.SyncView(ds.Id, ViewCreationBehavior.Refresh);
                dm.SyncView(dataset.Id, ViewCreationBehavior.Create | ViewCreationBehavior.Refresh);
            }
            finally
            {
                dm.CheckInDataset(datasetId, "for testing  datatuples with versions", username, ViewCreationBehavior.None);
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
        public void GetDatasetVersionEffectiveDataTuples_WhenCalledLatestVersionValid_ReturnListOfAbstractTuples()
        {

            //Arrange
            DatasetManager datasetManager = new DatasetManager();

            try
            {
                //Act
                DatasetVersion datasetversion = datasetManager.GetDatasetLatestVersion(datasetId);
                var result = datasetManager.GetDatasetVersionEffectiveTuples(datasetversion);

                //Assert
                Assert.That(result.Count(), Is.EqualTo(10));
            }
            finally
            {
                datasetManager.Dispose();
            }

        }

        [Test()]
        public void GetDatasetVersionEffectiveDataTuples_WhenCalledOlderVersionValid_ReturnListOfAbstractTuples()
        {

            //Arrange
            DatasetManager datasetManager = null;

            try
            {
                datasetManager = new DatasetManager();

                //get latest datatupleid before create a new dataset and data
                using (var uow = this.GetUnitOfWork())
                {
                    var latestDataTuple = uow.GetReadOnlyRepository<DataTuple>().Get().LastOrDefault();
                    if (latestDataTuple != null) latestDataTupleId = latestDataTuple.Id;
                }

                var dataset = datasetManager.GetDataset(datasetId);

                dataset = dsHelper.UpdateOneTupleForDataset(dataset, (StructuredDataStructure)dataset.DataStructure, latestDataTupleId, 1);
                datasetManager.CheckInDataset(dataset.Id, "for testing  datatuples with versions", username, ViewCreationBehavior.None);


                //Act
                List<DatasetVersion> datasetversions = datasetManager.GetDatasetVersions(datasetId).OrderBy(d => d.Timestamp).ToList();
                var result = datasetManager.GetDatasetVersionEffectiveTuples(datasetversions.ElementAt(datasetversions.Count - 2)); // get datatuples from the one before the latest

                //Assert
                Assert.That(result.Count(), Is.EqualTo(10));
            }
            finally
            {
                datasetManager.Dispose();
            }

        }


        [Test()]
        public void GetDataTuples_WhenCalledValid_ReturnIQueryable()
        {
            //Arrange
            DatasetManager datasetManager = new DatasetManager();

            try
            {
                //Act
                DatasetVersion datasetversion = datasetManager.GetDatasetLatestVersion(datasetId);
                var result = datasetManager.GetDataTuples(datasetversion.Id);
                int c = datasetManager.GetDataTuples(datasetversion.Id).Count();
                //Assert
                Assert.That(result.Count(), Is.EqualTo(10));
                Assert.That(c, Is.EqualTo(result.Count()));


            }
            finally
            {
                datasetManager.Dispose();
            }

        }

    }
}
