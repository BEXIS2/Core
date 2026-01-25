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
using Vaiona.Persistence.Api;

namespace BExIS.Dlm.Tests.Services.Data
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Reliability", "CA2000:Objekte verwerfen, bevor Bereich verloren geht", Justification = "<Ausstehend>")]
    [TestFixture()]
    public class DatasetManager_GetDataTuplesTest
    {
        private TestSetupHelper helper = null;
        private long datasetId = 0;
        private long latestDataTupleId = 0;
        private string username = "david";
        private long numberOfTuples = 10;
        private DatasetHelper dsHelper;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            helper = new TestSetupHelper(WebApiConfig.Register, false);
            using (var dm = new DatasetManager())
            using (var rsm = new ResearchPlanManager())
            using (var mdm = new MetadataStructureManager())
            using (var etm = new EntityTemplateManager())
            {
                dsHelper = new DatasetHelper();

                try
                {
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

                    var et = etm.Repo.Query().First();
                    et.Should().NotBeNull("Failed to meet a precondition: a entity template is required.");

                    Dataset dataset = dm.CreateEmptyDataset(dataStructure, rp, mds, et);
                    datasetId = dataset.Id;

                    // add datatuples
                    dataset = dsHelper.GenerateTuplesForDataset(dataset, dataStructure, numberOfTuples, username);
                    dm.CheckInDataset(dataset.Id, "for testing  datatuples with versions", username, ViewCreationBehavior.None);

                    dm.SyncView(dataset.Id, ViewCreationBehavior.Create | ViewCreationBehavior.Refresh);
                }
                finally
                {
                    dm.CheckInDataset(datasetId, "for testing  datatuples with versions", username, ViewCreationBehavior.None);
                }
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
        public void GetDataTuples_InvalidDatasetVersionId_ReturnEmptyIQueryable()
        {
            DatasetManager datasetManager = new DatasetManager();

            try
            {
                //Act
                var result = datasetManager.GetDataTuples(-1);

                //Assert
                Assert.That(result.Count(), Is.EqualTo(0));
            }
            finally
            {
                datasetManager.Dispose();
            }
        }

        [Test()]
        public void GetDataTuples_WhenCalledForLatestVersionComplete_ReturnIQueryable()
        {
            //Arrange
            DatasetManager datasetManager = new DatasetManager();

            try
            {
                //Act
                DatasetVersion datasetversion = datasetManager.GetDatasetLatestVersion(datasetId);
                var result = datasetManager.GetDataTuples(datasetversion.Id);
                int c = datasetManager.GetDataTuplesCount(datasetversion.Id);
                //Assert
                Assert.That(result.Count(), Is.EqualTo(numberOfTuples));
                Assert.That(c, Is.EqualTo(result.Count()));
            }
            finally
            {
                datasetManager.Dispose();
            }
        }

        [Test()]
        public void GetDataTuples_CallOlderVersionAfterTwoUpdates_ReturnIQueryable()
        {
            List<long> datatupleIds;

            try
            {
                //Arrange
                using (var datasetManager = new DatasetManager())
                {
                    var dataset = datasetManager.GetDataset(datasetId);
                    var datasetVersion = datasetManager.GetDatasetLatestVersion(datasetId);

                    Assert.IsNotNull(dataset);
                    Assert.IsNotNull(datasetVersion);

                    //get latest datatupleid before create a new dataset and data
                    using (var uow = this.GetUnitOfWork())
                    {
                        datatupleIds = uow.GetReadOnlyRepository<DataTuple>().Get().Where(dt => dt.DatasetVersion.Id.Equals(datasetVersion.Id)).Select(dt => dt.Id).ToList();
                    }

                    dataset = dsHelper.UpdateOneTupleForDataset(dataset, (StructuredDataStructure)dataset.DataStructure, datatupleIds[0], 1000, "david",datasetManager);
                    datasetManager.CheckInDataset(dataset.Id, "for testing  datatuples with versions", username, ViewCreationBehavior.None);

                    dataset = dsHelper.UpdateOneTupleForDataset(dataset, (StructuredDataStructure)dataset.DataStructure, datatupleIds[1], 2000, "david", datasetManager);
                    datasetManager.CheckInDataset(dataset.Id, "for testing  datatuples with versions", username, ViewCreationBehavior.None);
                }

                //Act
                using (var datasetManager = new DatasetManager())
                {
                    List<DatasetVersion> datasetversions = datasetManager.GetDatasetVersions(datasetId).OrderBy(d => d.Timestamp).ToList();
                    Assert.That(datasetversions.Count, Is.EqualTo(3));

                    foreach (var dsv in datasetversions)
                    {
                        int c = datasetManager.GetDataTuplesCount(dsv.Id);

                        var result = datasetManager.GetDataTuples(dsv.Id); // get datatuples from the one before the latest
                        int cm = result.Count();
                        Assert.That(c, Is.EqualTo(10));
                        Assert.That(c, Is.EqualTo(result.Count()));
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [Test()]
        public void GetDataTuples_WhenCalledOneOlderVersion_ReturnIQueryableOfAbstractTuples()
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

                dataset = dsHelper.UpdateOneTupleForDataset(dataset, (StructuredDataStructure)dataset.DataStructure, latestDataTupleId, 1, "david", datasetManager);
                datasetManager.CheckInDataset(dataset.Id, "for testing  datatuples with versions", username, ViewCreationBehavior.None);

                //Act
                List<DatasetVersion> datasetversions = datasetManager.GetDatasetVersions(datasetId).OrderBy(d => d.Timestamp).ToList();
                var result = datasetManager.GetDataTuples(datasetversions.ElementAt(datasetversions.Count - 2).Id); // get datatuples from the one before the latest

                //Assert
                Assert.That(result.Count(), Is.EqualTo(10));
            }
            finally
            {
                datasetManager.Dispose();
            }
        }

        //[Test()]
        //public void GetDataTuples_PageOfDataTuplesFromLatestVersion_ReturnListOfAbstractTuplesWithNumberOfPagesize()
        //{
        //    throw new NotImplementedException();
        //}

        //[Test()]
        //public void GetDataTuples_OrderdDataTuplesFromLatestVersion_ReturnOrderedListOfAbstractTuples()
        //{
        //    throw new NotImplementedException();
        //}

        //[Test()]
        //public void GetDataTuples_FilteredDataTuplesFromLatestVersion_ReturnFilteredListOfAbstractTuples()
        //{
        //    throw new NotImplementedException();
        //}

        //[Test()]
        //public void GetDataTuples_PageOfDataTuplesFromHistoricalVersion_ReturnListOfAbstractTuplesWithNumberOfPagesize()
        //{
        //    throw new NotImplementedException();
        //}

        //[Test()]
        //public void GetDataTuples_OrderdDataTuplesFromHistoricalVersion_ReturnOrderedListOfAbstractTuples()
        //{
        //    throw new NotImplementedException();
        //}

        //[Test()]
        //public void GetDataTuples_FilteredDataTuplesFromHistoricalVersion_ReturnFilteredListOfAbstractTuples()
        //{
        //    throw new NotImplementedException();
        //}
    }
}