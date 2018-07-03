using BExIS.App.Testing;
using BExIS.Dlm.Entities.Data;
using BExIS.Dlm.Entities.DataStructure;
using BExIS.Dlm.Services.Administration;
using BExIS.Dlm.Services.Data;
using BExIS.Dlm.Services.DataStructure;
using BExIS.Dlm.Services.MetadataStructure;
using BExIS.Dlm.Tests.Helpers;
using BExIS.Utils.Config;
using BExIS.Utils.Data.Helpers;
using BExIS.Web.Shell;
using BExIS.Web.Shell.Helpers;
using FluentAssertions;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using BExIS.Dlm.Orm.NH.Qurying;
using BExIS.Dlm.Entities.Administration;

namespace BExIS.Dlm.Tests.Services.Data
{    
    public class DatasetManagerTests
    {
        private TestSetupHelper helper = null;
        private StructuredDataStructure dataStructure;
        private ResearchPlan researchPlan;
        DatasetHelper dsHelper = null;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            helper = new TestSetupHelper(WebApiConfig.Register, false);
            dsHelper = new DatasetHelper();
            dsHelper.PurgeAllDatasets();
            dsHelper.PurgeAllDataStructures();

            dataStructure = dsHelper.CreateADataStructure();
            researchPlan = dsHelper.CreateResearchPlan();


        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            dsHelper.PurgeAllDatasets();
            dsHelper.PurgeAllDataStructures();

            helper.Dispose();
        }

        [Test()]
        public void CreateEmptyDatasetTest()
        {
             DatasetManager dm = new DatasetManager();
            var rsm = new ResearchPlanManager();
            var mdm = new MetadataStructureManager();

            dataStructure.Should().NotBeNull("Failed to meet a precondition: a data strcuture is required.");

            var rp = rsm.Repo.Query().First();
            rp.Should().NotBeNull("Failed to meet a precondition: a research plan is required.");

            var mds = mdm.Repo.Query().First();
            mds.Should().NotBeNull("Failed to meet a precondition: a metadata strcuture is required.");

            Dataset dataset = dm.CreateEmptyDataset(dataStructure, rp, mds);

            dataset.Should().NotBeNull();
            dataset.Id.Should().BeGreaterThan(0, "Dataset is not persisted.");
            dataset.LastCheckIOTimestamp.Should().NotBeAfter(DateTime.UtcNow, "The dataset's timestamp is wrong.");
            dataset.DataStructure.Should().NotBeNull("Dataset must have a data structure.");
            dataset.Status.Should().Be(DatasetStatus.CheckedIn, "Dataset must be in CheckedIn status.");

            dm.PurgeDataset(dataset.Id);
        }

        [Test()]
        public void DeleteDatasetTest()
        {
            DatasetManager dm = new DatasetManager();
            var rsm = new ResearchPlanManager();
            var mdm = new MetadataStructureManager();

            dataStructure.Should().NotBeNull("Failed to meet a precondition: a data strcuture is required.");

            var rp = rsm.Repo.Query().First();
            rp.Should().NotBeNull("Failed to meet a precondition: a research plan is required.");

            var mds = mdm.Repo.Query().First();
            mds.Should().NotBeNull("Failed to meet a precondition: a metadata strcuture is required.");

            Dataset dataset = dm.CreateEmptyDataset(dataStructure, rp, mds);
            dm.DeleteDataset(dataset.Id, "Javad", false);

            dataset.Should().NotBeNull();
            dataset.Id.Should().BeGreaterThan(0, "Dataset is not persisted.");
            dataset.LastCheckIOTimestamp.Should().NotBeAfter(DateTime.UtcNow, "The dataset's timestamp is wrong.");
            dataset.DataStructure.Should().NotBeNull("Dataset must have a data structure.");
            dataset.Status.Should().Be(DatasetStatus.Deleted, "Dataset must be in Deleted status.");

            dm.PurgeDataset(dataset.Id);
        }


        [Test()]
        public void CreateDatasetVersionTest()
        {
            long numberOfTuples = 1000;
            var dm = new DatasetManager();
            var rsm = new ResearchPlanManager();
            var mdm = new MetadataStructureManager();

            try
            {
                dataStructure.Should().NotBeNull("Failed to meet a precondition: a data strcuture is required.");

                var rp = rsm.Repo.Query().First();
                rp.Should().NotBeNull("Failed to meet a precondition: a research plan is required.");

                var mds = mdm.Repo.Query().First();
                mds.Should().NotBeNull("Failed to meet a precondition: a metadata strcuture is required.");

                Dataset dataset = dm.CreateEmptyDataset(dataStructure, rp, mds);
                dataset = dsHelper.GenerateTuplesForDataset(dataset, dataStructure, numberOfTuples);
                dataset.Should().NotBeNull("The dataset tuple generation has failed!");

                dm.CheckInDataset(dataset.Id, "for testing purposes 2", "Javad", ViewCreationBehavior.None);
                //dm.SyncView(ds.Id, ViewCreationBehavior.Create);
                //dm.SyncView(ds.Id, ViewCreationBehavior.Refresh);
                dm.SyncView(dataset.Id, ViewCreationBehavior.Create | ViewCreationBehavior.Refresh);

                dataset.Id.Should().BeGreaterThan(0, "Dataset was not persisted.");
                dataset.LastCheckIOTimestamp.Should().NotBeAfter(DateTime.UtcNow, "The dataset's timestamp is wrong.");
                dataset.DataStructure.Should().NotBeNull("Dataset must have a data structure.");
                dataset.Status.Should().Be(DatasetStatus.CheckedIn, "Dataset must be in the CheckedIn status.");
                dm.GetDatasetLatestVersionEffectiveTupleCount(dataset.Id).Should().Be(numberOfTuples);

                dm.DatasetVersionRepo.Evict();
                dm.DataTupleRepo.Evict();
                dm.DatasetRepo.Evict();
                dm.PurgeDataset(dataset.Id, true);
            }
            finally
            {
                dm.Dispose();
                rsm.Dispose();
                mdm.Dispose();
            }
        }

        [Test()]
        public void CreateAndExpressionForQueryingTest()
        {
            string var1Name = "var" + dataStructure.Variables.First().Id;
            string var2Name = "var" + dataStructure.Variables.Skip(1).First().Id;

            FilterExpression fex = BinaryFilterExpression
                .And(
                    new FilterNumberItemExpression()
                    {
                        Field = new Field() { DataType = BExIS.Dlm.Orm.NH.Qurying.DataType.Ineteger, Name = var1Name }
                        ,
                        Operator = NumberOperator.Operation.GreaterThan
                        ,
                        Value = 12
                    }
                    ,
                    new FilterStringItemExpression()
                    {
                        Field = new Field() { DataType = BExIS.Dlm.Orm.NH.Qurying.DataType.String, Name =  var2Name}
                            ,
                        Operator = StringOperator.Operation.EndsWith
                            ,
                        Value = "Test"
                    }
                );

            fex.ToSQL().Should().Be($"(({var1Name}) > (12)) AND (({var2Name}) LIKE ('%Test'))");

            // this is to show how to apply a NOT operator on any other expression.
            // It can be applied on Numeric, String, Date, and any other type of expression
            FilterExpression notFex = UnaryFilterExpression.Not(fex);
            notFex.ToSQL().Should().Be($"NOT ((({var1Name}) > (12)) AND (({var2Name}) LIKE ('%Test')))");
            notFex.ToSQL().Should().Be($"NOT ({fex.ToSQL()})"); 

            OrderByExpression orderByExpr = new OrderByExpression(
                                                    new List<OrderItemExpression>() {
                                                        new OrderItemExpression(var1Name),
                                                        new OrderItemExpression(var2Name, SortDirection.Descending)
                                                    });
            orderByExpr.ToSQL().Should().Be($"{var1Name} ASC, {var2Name} DESC");

            // create a dataset and test the filter, sorting, and projectgion
            long numberOfTuples = 100;
            var dm = new DatasetManager();
            var rsm = new ResearchPlanManager();
            var mdm = new MetadataStructureManager();

            try
            {
                dataStructure.Should().NotBeNull("Failed to meet a precondition: a data strcuture is required.");

                var rp = rsm.Repo.Query().First();
                rp.Should().NotBeNull("Failed to meet a precondition: a research plan is required.");

                var mds = mdm.Repo.Query().First();
                mds.Should().NotBeNull("Failed to meet a precondition: a metadata strcuture is required.");

                Dataset dataset = dm.CreateEmptyDataset(dataStructure, rp, mds);
                dataset = dsHelper.GenerateTuplesForDataset(dataset, dataStructure, numberOfTuples);
                dataset.Should().NotBeNull("The dataset tuple generation has failed!");

                dm.CheckInDataset(dataset.Id, "for testing purposes 2", "Javad", ViewCreationBehavior.None);
                dm.SyncView(dataset.Id, ViewCreationBehavior.Create | ViewCreationBehavior.Refresh);

                dataset.Id.Should().BeGreaterThan(0, "Dataset was not persisted.");
                dataset.LastCheckIOTimestamp.Should().NotBeAfter(DateTime.UtcNow, "The dataset's timestamp is wrong.");
                dataset.DataStructure.Should().NotBeNull("Dataset must have a data structure.");
                dataset.Status.Should().Be(DatasetStatus.CheckedIn, "Dataset must be in the CheckedIn status.");
                dm.GetDatasetLatestVersionEffectiveTupleCount(dataset.Id).Should().Be(numberOfTuples);

                // pass this filter to get a subset of dataset X
                var dst = dm.GetLatestDatasetVersionTuples(dataset.Id, fex, null, null, 1, 10);
                dst.Should().NotBeNull();
                dst.Rows.Count.Should().BeLessOrEqualTo(10);

                dm.DatasetVersionRepo.Evict();
                dm.DataTupleRepo.Evict();
                dm.DatasetRepo.Evict();
                dm.PurgeDataset(dataset.Id, true);
            }
            finally
            {
                dm.Dispose();
                rsm.Dispose();
                mdm.Dispose();
            }

        }

        [Test()]
        public void ProjectExpressionTest()
        {
            string var1Name = "var" + dataStructure.Variables.First().Id;
            string var3Name = "var" + dataStructure.Variables.Skip(2).First().Id;

            //create prjection expression
            ProjectionExpression projectionExpression = new ProjectionExpression();

            projectionExpression.Items.Add(new ProjectionItemExpression() { FieldName = var1Name });
            projectionExpression.Items.Add(new ProjectionItemExpression() { FieldName = var3Name });


            // create a dataset and test the filter, sorting, and projectgion
            long numberOfTuples = 10;
            var dm = new DatasetManager();
            var rsm = new ResearchPlanManager();
            var mdm = new MetadataStructureManager();

            try
            {
                dataStructure.Should().NotBeNull("Failed to meet a precondition: a data strcuture is required.");

                var rp = rsm.Repo.Query().First();
                rp.Should().NotBeNull("Failed to meet a precondition: a research plan is required.");

                var mds = mdm.Repo.Query().First();
                mds.Should().NotBeNull("Failed to meet a precondition: a metadata strcuture is required.");

                Dataset dataset = dm.CreateEmptyDataset(dataStructure, rp, mds);
                dataset = dsHelper.GenerateTuplesForDataset(dataset, dataStructure, numberOfTuples);
                dataset.Should().NotBeNull("The dataset tuple generation has failed!");

                dm.CheckInDataset(dataset.Id, "for testing purposes 2", "Javad", ViewCreationBehavior.None);
                dm.SyncView(dataset.Id, ViewCreationBehavior.Create | ViewCreationBehavior.Refresh);

                dataset.Id.Should().BeGreaterThan(0, "Dataset was not persisted.");
                dataset.LastCheckIOTimestamp.Should().NotBeAfter(DateTime.UtcNow, "The dataset's timestamp is wrong.");
                dataset.DataStructure.Should().NotBeNull("Dataset must have a data structure.");
                dataset.Status.Should().Be(DatasetStatus.CheckedIn, "Dataset must be in the CheckedIn status.");
                dm.GetDatasetLatestVersionEffectiveTupleCount(dataset.Id).Should().Be(numberOfTuples);

                // pass this filter to get a subset of dataset X
                var dst = dm.GetLatestDatasetVersionTuples(dataset.Id, null, null, projectionExpression, 1, 3);
                dst.Should().NotBeNull();
                dst.Rows.Count.Should().BeLessOrEqualTo(3);
                dst.Columns.Count.Should().BeLessOrEqualTo(3, "Projection fails, false number of columns");

                dm.DatasetVersionRepo.Evict();
                dm.DataTupleRepo.Evict();
                dm.DatasetRepo.Evict();
                dm.PurgeDataset(dataset.Id, true);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                dm.Dispose();
                rsm.Dispose();
                mdm.Dispose();
            }

        }

        
    }

}
