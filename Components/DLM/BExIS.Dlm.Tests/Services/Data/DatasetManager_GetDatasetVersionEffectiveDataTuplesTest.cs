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
using System.Collections.Generic;
using System.Linq;
using Vaiona.Persistence.Api;

namespace BExIS.Dlm.Tests.Services.Data
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Reliability", "CA2000:Objekte verwerfen, bevor Bereich verloren geht", Justification = "<Ausstehend>")]
    [TestFixture()]
    public class DatasetManager_GetDatasetVersionEffectiveDataTuplesTest
    {
        private TestSetupHelper helper = null;
        private long datasetId = 0;
        private long latestDataTupleId = 0;
        private long firstDataTupleId = 0;
        private string username = "David";

        private DatasetHelper dsHelper;
        private long numberOfTuples = 10;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            helper = new TestSetupHelper(WebApiConfig.Register, false);
            var dm = new DatasetManager();
            var rsm = new ResearchPlanManager();
            var mdm = new MetadataStructureManager();
            var etm = new EntityTemplateManager();
            dsHelper = new DatasetHelper();

            try
            {
                dsHelper.PurgeAllDatasets();
                dsHelper.PurgeAllDataStructures();
                dsHelper.PurgeAllResearchPlans();

                StructuredDataStructure dataStructure = dsHelper.CreateADataStructure();
                dataStructure.Should().NotBeNull("Failed to meet a precondition: a data strcuture is required.");

                var rp = dsHelper.CreateResearchPlan();
                rp.Should().NotBeNull("Failed to meet a precondition: a research plan is required.");

                var mds = mdm.Repo.Query().First();
                mds.Should().NotBeNull("Failed to meet a precondition: a metadata strcuture is required.");

                var et = etm.Repo.Query().First();
                et.Should().NotBeNull("Failed to meet a precondition: a entity template is required.");

                Dataset dataset = dm.CreateEmptyDataset(dataStructure, rp, mds, et);
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
                etm.Dispose();
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
        public void GetDatasetVersionEffectiveDataTuples_AllDataTuplesFromLatestVersion_ReturnListOfAbstractTuples()
        {
            //Arrange
            DatasetManager datasetManager = new DatasetManager();

            try
            {
                //Act
                DatasetVersion datasetversion = datasetManager.GetDatasetLatestVersion(datasetId);
                var result = datasetManager.GetDatasetVersionEffectiveTuples(datasetversion);

                //Assert
                Assert.That(result.Count(), Is.EqualTo(numberOfTuples));
            }
            finally
            {
                datasetManager.Dispose();
            }
        }

        [Test()]
        public void GetDatasetVersionEffectiveDataTuples_PageOfDataTuplesFromLatestVersion_ReturnListOfAbstractTuples()
        {
            //Arrange
            DatasetManager datasetManager = new DatasetManager();

            int pagesize = (int)(numberOfTuples / 4);
            int page = 2;

            try
            {
                //Act
                DatasetVersion datasetversion = datasetManager.GetDatasetLatestVersion(datasetId);
                var result = datasetManager.GetDatasetVersionEffectiveTuples(datasetversion, page, pagesize);

                //Assert
                Assert.That(result.Count(), Is.EqualTo(pagesize));
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

                dataset = dsHelper.UpdateOneTupleForDataset(dataset, (StructuredDataStructure)dataset.DataStructure, latestDataTupleId, 1,"david", datasetManager);
                datasetManager.CheckInDataset(dataset.Id, "for testing  datatuples with versions", username, ViewCreationBehavior.None);

                //Act
                List<DatasetVersion> datasetversions = datasetManager.GetDatasetVersions(datasetId).OrderBy(d => d.Timestamp).ToList();
                var result = datasetManager.GetDatasetVersionEffectiveTuples(datasetversions.ElementAt(datasetversions.Count - 2)); // get datatuples from the one before the latest

                //Assert
                Assert.That(result.Count(), Is.EqualTo(numberOfTuples));
            }
            finally
            {
                datasetManager.Dispose();
            }
        }

        [Test]
        public void GetDatasetVersionEffectiveDataTuples_CalledOlderVersionWithPaging_ReturnListOfAbstractTuples()
        {
            //Arrange
            DatasetManager datasetManager = null;
            int pageSize = 4;
            int pageNumber = 2;

            try
            {
                datasetManager = new DatasetManager();

                var latestDatasetVersionId = datasetManager.GetDatasetLatestVersionId(datasetId);

                //get latest datatupleid before create a new dataset and data
                using (var uow = this.GetUnitOfWork())
                {
                    var latestDataTuple = uow.GetReadOnlyRepository<DataTuple>().Get().LastOrDefault();
                    var firstDataTuple = uow.GetReadOnlyRepository<DataTuple>().Get().Where(dt => dt.DatasetVersion.Id.Equals(latestDatasetVersionId)).FirstOrDefault();
                    if (latestDataTuple != null) latestDataTupleId = latestDataTuple.Id;
                    if (firstDataTuple != null) firstDataTupleId = firstDataTuple.Id;
                }

                var dataset = datasetManager.GetDataset(datasetId);

                dataset = dsHelper.UpdateOneTupleForDataset(dataset, (StructuredDataStructure)dataset.DataStructure, firstDataTupleId, 1, username, datasetManager);
                datasetManager.CheckInDataset(dataset.Id, "for testing  datatuples with versions", username, ViewCreationBehavior.None);

                dataset = dsHelper.UpdateOneTupleForDataset(dataset, (StructuredDataStructure)dataset.DataStructure, latestDataTupleId, 2, username, datasetManager);
                datasetManager.CheckInDataset(dataset.Id, "for testing  datatuples with versions", username, ViewCreationBehavior.None);

                //Act
                List<DatasetVersion> datasetversions = datasetManager.GetDatasetVersions(datasetId).OrderBy(d => d.Timestamp).ToList();
                var resultAll = datasetManager.GetDatasetVersionEffectiveTuples(datasetversions.ElementAt(datasetversions.Count - 2));
                List<long> comapreIds = resultAll.OrderBy(dt => dt.OrderNo).Skip(pageNumber * pageSize).Take(pageSize).Select(dt => dt.Id).ToList();

                var result = datasetManager.GetDatasetVersionEffectiveTuples(datasetversions.ElementAt(datasetversions.Count - 2), pageNumber, pageSize); // get datatuples from the one before the latest
                var resultIds = result.Select(dt => dt.Id).ToList();

                //Assert
                Assert.That(comapreIds, Is.EquivalentTo(resultIds));
            }
            finally
            {
                datasetManager.Dispose();
            }
        }

        //[Test()]
        //public void GetDatasetVersionEffectiveDataTuples_PageOfDataTuplesFromLatestVersion_ReturnListOfAbstractTuplesWithNumberOfPagesize()
        //{
        //    throw new NotImplementedException();
        //}

        //[Test()]
        //public void GetDatasetVersionEffectiveDataTuples_OrderdDataTuplesFromLatestVersion_ReturnOrderedListOfAbstractTuples()
        //{
        //    throw new NotImplementedException();
        //}

        //[Test()]
        //public void GetDatasetVersionEffectiveDataTuples_FilteredDataTuplesFromLatestVersion_ReturnFilteredListOfAbstractTuples()
        //{
        //    throw new NotImplementedException();
        //}

        //[Test()]
        //public void GetDatasetVersionEffectiveDataTuples_PageOfDataTuplesFromHistoricalVersion_ReturnListOfAbstractTuplesWithNumberOfPagesize()
        //{
        //    throw new NotImplementedException();
        //}

        //[Test()]
        //public void GetDatasetVersionEffectiveDataTuples_OrderdDataTuplesFromHistoricalVersion_ReturnOrderedListOfAbstractTuples()
        //{
        //    throw new NotImplementedException();
        //}

        //[Test()]
        //public void GetDatasetVersionEffectiveDataTuples_FilteredDataTuplesFromHistoricalVersion_ReturnFilteredListOfAbstractTuples()
        //{
        //    throw new NotImplementedException();
        //}
    }
}