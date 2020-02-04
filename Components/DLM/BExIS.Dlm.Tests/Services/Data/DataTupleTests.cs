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
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using Vaiona.Persistence.Api;

namespace BExIS.Dlm.Tests.Services.Data
{
    [TestFixture()]
    public class DataTupleTests
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
                long numberOfTuples = 10;

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
        public void TransformDatatupleToJson()
        {

            //Arrange
            DatasetManager datasetManager = new DatasetManager();

            try
            {
                //Act
                DatasetVersion datasetversion = datasetManager.GetDatasetLatestVersion(datasetId);
                var result = datasetManager.GetDatasetVersionEffectiveTuples(datasetversion);

                foreach (var dt in result)
                {
                    dt.Materialize();

                    dt.Dematerialize();
                }

                var newDts = result.Cast<DataTuple>();

                foreach (var dt in newDts)
                {
                    //dt.Materialize2();

                    //dt.Dematerialize2(); // convert variablevalues 1 to json

                }


                //Assert
                Assert.That(result.Count(), Is.EqualTo(10));

                foreach (var dt in newDts)
                {
                    for (int i = 0; i < dt.VariableValues.Count; i++)
                    {
                        var vv1 = dt.VariableValues.ElementAt(i);
                        //var vv2 = dt.VariableValues2.ElementAt(i);

                        //Assert.That(vv1.DataAttribute.Id , Is.EqualTo(vv2.DataAttribute.Id));
                        //Assert.That(vv1.Note, Is.EqualTo(vv2.Note));
                        //Assert.That(vv1.ObtainingMethod, Is.EqualTo(vv2.ObtainingMethod));
                        //Assert.That(vv1.ParameterValues, Is.EqualTo(vv2.ParameterValues));
                        //Assert.That(vv1.ResultTime, Is.EqualTo(vv2.ResultTime));
                        //Assert.That(vv1.SamplingTime, Is.EqualTo(vv2.SamplingTime));
                        //Assert.That(vv1.Tuple.Id, Is.EqualTo(vv2.Tuple.Id));
                        //Assert.That(vv1.Value, Is.EqualTo(vv2.Value));
                        //Assert.That(vv1.Variable.Id, Is.EqualTo(vv2.Variable.Id));
                        //Assert.That(vv1.VariableId, Is.EqualTo(vv2.VariableId));
                    }
                }
            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {
                datasetManager.Dispose();
            }

        }

    }
}
