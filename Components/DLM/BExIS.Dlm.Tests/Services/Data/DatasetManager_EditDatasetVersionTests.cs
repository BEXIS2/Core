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
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Reliability", "CA2000:Objekte verwerfen, bevor Bereich verloren geht", Justification = "<Ausstehend>")]

    [TestFixture()]
    public class DatasetManager_EditDatasetVersionTests
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

                Dataset dataset = dm.CreateEmptyDataset(dataStructure, rp, mds);
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
        public void EditDatasetVersion_DeleteADataTuple_ReturnUpdatedVersion()
        {
            DatasetManager datasetManager = new DatasetManager();

            try
            {
                //Arrange
                DatasetVersion latest = datasetManager.GetDatasetLatestVersion(datasetId);

                int before = datasetManager.GetDataTuplesCount(latest.Id);
                var tuple = datasetManager.GetDataTuples(latest.Id).LastOrDefault();

                //Act
                if (datasetManager.IsDatasetCheckedOutFor(datasetId, "David") || datasetManager.CheckOutDataset(datasetId, "David"))
                {
                    DatasetVersion workingCopy = datasetManager.GetDatasetWorkingCopy(datasetId);


                    List<DataTuple> deleteTuples = new List<DataTuple>();
                    deleteTuples.Add(tuple as DataTuple);

                    workingCopy = datasetManager.EditDatasetVersion(workingCopy, null, null, deleteTuples.Select(d=>d.Id).ToList());

                    datasetManager.CheckInDataset(datasetId, "delete one datatuple for testing", username, ViewCreationBehavior.None);

                }

                latest = datasetManager.GetDatasetLatestVersion(datasetId);

                int after = datasetManager.GetDataTuplesCount(latest.Id);


                //Assert
                Assert.That(before, Is.GreaterThan(after));


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



        [Test()]
        public void EditDatasetVersion_DeleteADataTupleAfterUpdate_ReturnUpdatedVersion()
        {
            Dataset dataset;
            DatasetVersion latest;
            using (DatasetManager datasetManager = new DatasetManager())
            {
                //Arrange
                dataset = datasetManager.GetDataset(datasetId);
                latest = datasetManager.GetDatasetLatestVersion(datasetId);

                //update the dataset
                dataset = dsHelper.UpdateAnyTupleForDataset(dataset, dataset.DataStructure as StructuredDataStructure, datasetManager);
                datasetManager.CheckInDataset(datasetId, "for testing  update all datatuple", username, ViewCreationBehavior.None);
            }

            using (DatasetManager datasetManager = new DatasetManager())
            {
                try
                {

                    latest = datasetManager.GetDatasetLatestVersion(datasetId);

                    int before = datasetManager.GetDataTuplesCount(latest.Id);
                    var tuple = datasetManager.GetDataTuples(latest.Id).LastOrDefault();

                    //Act
                    if (datasetManager.IsDatasetCheckedOutFor(datasetId, "David") || datasetManager.CheckOutDataset(datasetId, "David"))
                    {
                        DatasetVersion workingCopy = datasetManager.GetDatasetWorkingCopy(datasetId);


                        List<AbstractTuple> deleteTuples = new List<AbstractTuple>();
                        deleteTuples.Add(tuple);

                        workingCopy = datasetManager.EditDatasetVersion(workingCopy, null, null, deleteTuples.Select(d => d.Id).ToList());

                        datasetManager.CheckInDataset(datasetId, "delete one datatuple for testing", username, ViewCreationBehavior.None);

                    }

                    latest = datasetManager.GetDatasetLatestVersion(datasetId);

                    int after = datasetManager.GetDataTuplesCount(latest.Id);


                    //Assert
                    Assert.That(before, Is.GreaterThan(after));


                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
        }
    }
}