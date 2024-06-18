using BExIS.App.Testing;
using BExIS.Dlm.Entities.Data;
using BExIS.Dlm.Entities.DataStructure;
using BExIS.Dlm.Services.Administration;
using BExIS.Dlm.Services.Data;
using BExIS.Dlm.Services.MetadataStructure;
using BExIS.Utils.Config;
using BExIS.Utils.Tests.Data.Helpers;
using BExIS.Utils.Upload;
using FluentAssertions;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BExIS.Utils.Data.Tests
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Reliability", "CA2000:Objekte verwerfen, bevor Bereich verloren geht", Justification = "<Ausstehend>")]
    [TestFixture()]
    public class UploadHelperTests
    {
        private TestSetupHelper helper = null;
        private long datasetId = 0;
        private long latestDataTupleId = 0;
        private string username = "David";
        private long numberOfTuples = 100;
        private DatasetHelper dsHelper;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            helper = new TestSetupHelper(WebApiConfig.Register, false);
            dsHelper = new DatasetHelper();

            using (var dm = new DatasetManager())
            using (var rsm = new ResearchPlanManager())
            using (var mdm = new MetadataStructureManager())
            using (var etm = new EntityTemplateManager())
            {
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
                dataset = dsHelper.GenerateTuplesWithRandomValuesForDataset(dataset, dataStructure, numberOfTuples, username);
                dm.CheckInDataset(dataset.Id, "for testing  datatuples with versions", username, ViewCreationBehavior.None);

                dm.SyncView(dataset.Id, ViewCreationBehavior.Create | ViewCreationBehavior.Refresh);

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

        [TestCase(0)]// primary key as int
        [TestCase(2)]// primary key as double
        public void GetSplitDatatuples_AllDataTuplesEdited_SameNumberOfDatatuples(int primaryKeyIndex)
        {
            Dataset dataset;
            DatasetVersion latest;
            List<DataTuple> incoming = new List<DataTuple>();
            int count = 0;
            int expectedCount = 0;
            List<long> datatupleFromDatabaseIds = new List<long>();

            using (DatasetManager datasetManager = new DatasetManager())
            {
                //Arrange
                dataset = datasetManager.GetDataset(datasetId);
                latest = datasetManager.GetDatasetLatestVersion(datasetId);
                datatupleFromDatabaseIds = datasetManager.GetDatasetVersionEffectiveTupleIds(latest);

                //get updated tuples as incoming datatuples
                incoming = dsHelper.GetUpdatedDatatuples(latest, dataset.DataStructure as StructuredDataStructure, datasetManager);

                //because of updateing all datatuples the incoming number is should be equal then the existing one
                expectedCount = incoming.Count;
            }

            try
            {
                List<long> primaryKeys = new List<long>();

                //get primarykey ids
                // var 1 = int = 1
                // var 2 = string = 2
                // var 3 = double = 3
                // var 4 = boolean = 4
                // var 5 = datetime = 5
                List<long> varIds = ((StructuredDataStructure)dataset.DataStructure).Variables.Select(v => v.Id).ToList();

                primaryKeys.Add(varIds.ElementAt(primaryKeyIndex));

                //Act
                Dictionary<string, List<DataTuple>> splittedDatatuples = new Dictionary<string, List<DataTuple>>();
                UploadHelper uploadhelper = new UploadHelper();
                splittedDatatuples = uploadhelper.GetSplitDatatuples(incoming, primaryKeys, null, ref datatupleFromDatabaseIds);

                //Assert
                int newCount = splittedDatatuples["new"].Count;
                int editCount = splittedDatatuples["edit"].Count;

                Assert.That(newCount, Is.EqualTo(0));
                Assert.That(editCount, Is.EqualTo(expectedCount));
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [Test]
        public void GetSplitDatatuples_CombinedPrimaryKeyOneUpdatedDataTuple_ExpectedNumberOfEditDatatuples()
        {
            Dataset dataset;
            DatasetVersion latest;
            List<DataTuple> incoming = new List<DataTuple>();
            int count = 0;
            int expectedCount = 0;
            List<long> datatupleFromDatabaseIds = new List<long>();

            //setup the primary key conbination
            List<int> primaryKeysIndex = new List<int> { 0, 2 };
            List<long> primaryKeyIds = new List<long>();

            using (DatasetManager datasetManager = new DatasetManager())
            {
                //Arrange
                dataset = datasetManager.GetDataset(datasetId);
                latest = datasetManager.GetDatasetLatestVersion(datasetId);
                datatupleFromDatabaseIds = datasetManager.GetDatasetVersionEffectiveTupleIds(latest);

                foreach (var dtid in datatupleFromDatabaseIds)
                {
                    var datatuple = datasetManager.DataTupleRepo.Get(dtid);
                    datatuple.Materialize();
                    incoming.Add(datatuple);
                }

                //updated last datatuple in text
                dsHelper.GetUpdatedDatatuple(incoming.Last(), 1);

                //get varids of primary key combination
                List<long> allVarIds = ((StructuredDataStructure)dataset.DataStructure).Variables.Select(v => v.Id).ToList();

                foreach (int pkIndex in primaryKeysIndex)
                {
                    primaryKeyIds.Add(allVarIds.ElementAt(pkIndex));
                }

                //Act
                Dictionary<string, List<DataTuple>> splittedDatatuples = new Dictionary<string, List<DataTuple>>();
                UploadHelper uploadhelper = new UploadHelper();
                splittedDatatuples = uploadhelper.GetSplitDatatuples(incoming, primaryKeyIds, null, ref datatupleFromDatabaseIds);

                //Assert
                Assert.That(splittedDatatuples["new"].Count, Is.EqualTo(0));
                Assert.That(splittedDatatuples["edit"].Count, Is.EqualTo(1));
            }
        }
    }
}