using BExIS.App.Testing;
using BExIS.Dlm.Entities.Data;
using BExIS.Dlm.Entities.DataStructure;
using BExIS.Dlm.Services.Administration;
using BExIS.Dlm.Services.Data;
using BExIS.Dlm.Services.MetadataStructure;
using BExIS.Dlm.Tests.Helpers;
using BExIS.Security.Entities.Versions;
using BExIS.Utils.Config;
using BExIS.Utils.NH.Querying;
using FluentAssertions;
using NUnit.Framework;
using System;
using System.Collections.Generic;
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

        [SetUp]
        public void SetUp()
        {
            var dsHelper = new DatasetHelper();
            //dsHelper.PurgeAllDatasets();
            //dsHelper.PurgeAllDataStructures();
            //dsHelper.PurgeAllResearchPlans();
        }

        [TearDown]
        public void TearDown()
        {
            var dsHelper = new DatasetHelper();
            dsHelper.PurgeAllDatasets();
            dsHelper.PurgeAllDataStructures();
            dsHelper.PurgeAllResearchPlans();
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            helper.Dispose();
        }

        [Test()]
        public void CreateEmptyDatasetTest()
        {
            var dm = new DatasetManager();
            var rsm = new ResearchPlanManager();
            var mdm = new MetadataStructureManager();
            var etm = new EntityTemplateManager();
            try
            {
                var dsHelper = new DatasetHelper();
                StructuredDataStructure dataStructure = dsHelper.CreateADataStructure();
                dataStructure.Should().NotBeNull("Failed to meet a precondition: a data strcuture is required.");

                var rp = dsHelper.CreateResearchPlan();
                rp.Should().NotBeNull("Failed to meet a precondition: a research plan is required.");

                var mds = mdm.Repo.Query().First();
                mds.Should().NotBeNull("Failed to meet a precondition: a metadata strcuture is required.");

                var et = etm.Repo.Query().First();
                et.Should().NotBeNull("Failed to meet a precondition: a entity template is required.");

                Dataset dataset = dm.CreateEmptyDataset(dataStructure, rp, mds, et);

                dataset.Should().NotBeNull();
                dataset.Id.Should().BeGreaterThan(0, "Dataset is not persisted.");
                dataset.LastCheckIOTimestamp.Should().NotBeAfter(DateTime.UtcNow, "The dataset's timestamp is wrong.");
                dataset.DataStructure.Should().NotBeNull("Dataset must have a data structure.");
                dataset.Status.Should().Be(DatasetStatus.CheckedIn, "Dataset must be in CheckedIn status.");

                dm.PurgeDataset(dataset.Id);
                dsHelper.PurgeAllDataStructures();
            }
            finally
            {
                dm?.Dispose();
                rsm?.Dispose();
                mdm?.Dispose();
                etm?.Dispose();
            }
        }

        [Test()]
        public void DeleteDatasetTest()
        {
            var dm = new DatasetManager();
            var rsm = new ResearchPlanManager();
            var mdm = new MetadataStructureManager();
            var etm = new EntityTemplateManager();

            try
            {
                var dsHelper = new DatasetHelper();
                StructuredDataStructure dataStructure = dsHelper.CreateADataStructure();
                dataStructure.Should().NotBeNull("Failed to meet a precondition: a data strcuture is required.");

                var rp = dsHelper.CreateResearchPlan();
                rp.Should().NotBeNull("Failed to meet a precondition: a research plan is required.");

                var mds = mdm.Repo.Query().First();
                mds.Should().NotBeNull("Failed to meet a precondition: a metadata strcuture is required.");

                var et = etm.Repo.Query().First();
                et.Should().NotBeNull("Failed to meet a precondition: a entity template is required.");

                Dataset dataset = dm.CreateEmptyDataset(dataStructure, rp, mds, et);
                dm.DeleteDataset(dataset.Id, "Javad", false);

                dataset.Should().NotBeNull();
                dataset.Id.Should().BeGreaterThan(0, "Dataset is not persisted.");
                dataset.LastCheckIOTimestamp.Should().NotBeAfter(DateTime.UtcNow, "The dataset's timestamp is wrong.");
                dataset.DataStructure.Should().NotBeNull("Dataset must have a data structure.");
                dataset.Status.Should().Be(DatasetStatus.Deleted, "Dataset must be in Deleted status.");

                dsHelper.PurgeAllDatasets();
                dsHelper.PurgeAllDataStructures();
            }
            finally
            {
                dm.Dispose();
                rsm.Dispose();
                mdm.Dispose();
                etm.Dispose();
            }
        }

        [Test()]
        public void UndoDeleteDatasetTest()
        {
            var dm = new DatasetManager();
            var rsm = new ResearchPlanManager();
            var mdm = new MetadataStructureManager();
            var etm = new EntityTemplateManager();

            var dsHelper = new DatasetHelper();
            long id = 0;
            try
            {
                // Arrange
                StructuredDataStructure dataStructure = dsHelper.CreateADataStructure();
                dataStructure.Should().NotBeNull("Failed to meet a precondition: a data strcuture is required.");

                var rp = dsHelper.CreateResearchPlan();
                rp.Should().NotBeNull("Failed to meet a precondition: a research plan is required.");

                var mds = mdm.Repo.Query().First();
                mds.Should().NotBeNull("Failed to meet a precondition: a metadata strcuture is required.");

                var et = etm.Repo.Query().First();
                et.Should().NotBeNull("Failed to meet a precondition: a entity template is required.");

                Dataset dataset = dm.CreateEmptyDataset(dataStructure, rp, mds, et);
                id = dataset.Id;

                dsHelper.GenerateTuplesForDataset(dataset, dataStructure, 1000, "david test");
                dm.CheckInDataset(dataset.Id, "for testing  datatuples with versions", "david test", ViewCreationBehavior.None);

                using (var datasetmanager = new DatasetManager())
                {
                    datasetmanager.DeleteDataset(dataset.Id, "David", false);

                    dataset.Should().NotBeNull();
                    dataset.Id.Should().BeGreaterThan(0, "Dataset is not persisted.");
                    dataset.LastCheckIOTimestamp.Should().NotBeAfter(DateTime.UtcNow, "The dataset's timestamp is wrong.");
                    dataset.DataStructure.Should().NotBeNull("Dataset must have a data structure.");
                    dataset.Status.Should().Be(DatasetStatus.Deleted, "Dataset must be in Deleted status.");
                    dm.CheckInDataset(dataset.Id, "for testing  datatuples with versions", "david test", ViewCreationBehavior.None);
                }

                using (var datasetmanager = new DatasetManager())
                {
                    dataset = datasetmanager.GetDataset(id);
                    var deletedVersionId = dataset.Versions.Last().Id;

                    // Act
                    datasetmanager.UndoDeleteDataset(dataset.Id, "David test", true);
                    var c = datasetmanager.GetDatasetLatestVersionEffectiveTupleCount(dataset);

                    //Assert
                    dataset = datasetmanager.GetDataset(id);
                    dataset.Should().NotBeNull();
                    dataset.Id.Should().BeGreaterThan(0, "Dataset is not persisted.");
                    dataset.LastCheckIOTimestamp.Should().NotBeAfter(DateTime.UtcNow, "The dataset's timestamp is wrong.");
                    dataset.DataStructure.Should().NotBeNull("Dataset must have a data structure.");
                    dataset.Status.Should().Be(DatasetStatus.CheckedIn, "Dataset must be in Deleted status.");

                    var deletedVersion = datasetmanager.DatasetVersionRepo.Get(deletedVersionId);
                    deletedVersion.Should().BeNull("Deleted version must be null.");

                    Assert.That(c.Equals(1000), "version has not same tuples");
                }
            }
            finally
            {
                dsHelper.PurgeAllDatasets();
                dsHelper.PurgeAllDataStructures();

                dm.Dispose();
                rsm.Dispose();
                mdm.Dispose();
                etm.Dispose();
            }
        }

        [Test()]
        public void CreateDatasetVersionTest()
        {
            long numberOfTuples = 10;
            var dm = new DatasetManager();
            var rsm = new ResearchPlanManager();
            var mdm = new MetadataStructureManager();
            var etm = new EntityTemplateManager();

            try
            {
                var dsHelper = new DatasetHelper();
                StructuredDataStructure dataStructure = dsHelper.CreateADataStructure();
                dataStructure.Should().NotBeNull("Failed to meet a precondition: a data strcuture is required.");

                var rp = dsHelper.CreateResearchPlan();
                rp.Should().NotBeNull("Failed to meet a precondition: a research plan is required.");

                var mds = mdm.Repo.Query().First();
                mds.Should().NotBeNull("Failed to meet a precondition: a metadata strcuture is required.");

                var et = etm.Repo.Query().First();
                et.Should().NotBeNull("Failed to meet a precondition: a entity template is required.");

                Dataset dataset = dm.CreateEmptyDataset(dataStructure, rp, mds, et);
                dataset = dsHelper.GenerateTuplesForDataset(dataset, dataStructure, numberOfTuples, "Javad");
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

                dsHelper.PurgeAllDataStructures();
            }
            finally
            {
                dm.Dispose();
                rsm.Dispose();
                mdm.Dispose();
                etm.Dispose();
            }
        }

        [Test()]
        public void CreateAndExpressionForQueryingTest()
        {
            var dsHelper = new DatasetHelper();
            StructuredDataStructure dataStructure = dsHelper.CreateADataStructure();
            dataStructure.Should().NotBeNull("Failed to meet a precondition: a data strcuture is required.");

            string var1Name = "var" + dataStructure.Variables.First().Id;
            string var2Name = "var" + dataStructure.Variables.Skip(1).First().Id;

            FilterExpression fex = BinaryFilterExpression
                .And(
                    new FilterNumberItemExpression()
                    {
                        Field = new Field() { DataType = Utils.NH.Querying.DataType.Ineteger, Name = var1Name }
                        ,
                        Operator = NumberOperator.Operation.GreaterThan
                        ,
                        Value = 12
                    }
                    ,
                    new FilterStringItemExpression()
                    {
                        Field = new Field() { DataType = Utils.NH.Querying.DataType.String, Name = var2Name }
                            ,
                        Operator = StringOperator.Operation.EndsWith
                            ,
                        Value = "Test"
                    }
                );

            fex.ToSQL().Should().Be($"(({var1Name}) > (12)) AND (({var2Name}) ILIKE ('%Test'))");

            // this is to show how to apply a NOT operator on any other expression.
            // It can be applied on Numeric, String, Date, and any other type of expression
            FilterExpression notFex = UnaryFilterExpression.Not(fex);
            notFex.ToSQL().Should().Be($"NOT ((({var1Name}) > (12)) AND (({var2Name}) ILIKE ('%Test')))");
            notFex.ToSQL().Should().Be($"NOT ({fex.ToSQL()})");

            OrderByExpression orderByExpr = new OrderByExpression(
                                                    new List<OrderItemExpression>() {
                                                        new OrderItemExpression(var1Name),
                                                        new OrderItemExpression(var2Name, SortDirection.Descending)
                                                    });
            orderByExpr.ToSQL().Should().Be($"{var1Name} ASC, {var2Name} DESC");

            // create a dataset and test the filter, sorting, and projectgion
            long numberOfTuples = 10;
            var dm = new DatasetManager();
            var rsm = new ResearchPlanManager();
            var mdm = new MetadataStructureManager();
            var etm = new EntityTemplateManager();

            try
            {
                dataStructure.Should().NotBeNull("Failed to meet a precondition: a data strcuture is required.");

                var rp = dsHelper.CreateResearchPlan();
                rp.Should().NotBeNull("Failed to meet a precondition: a research plan is required.");

                var mds = mdm.Repo.Query().First();
                mds.Should().NotBeNull("Failed to meet a precondition: a metadata strcuture is required.");

                var et = etm.Repo.Query().First();
                et.Should().NotBeNull("Failed to meet a precondition: a entity template is required.");

                Dataset dataset = dm.CreateEmptyDataset(dataStructure, rp, mds, et);

                dataset = dsHelper.GenerateTuplesForDataset(dataset, dataStructure, numberOfTuples, "Javad");
                dataset.Should().NotBeNull("The dataset tuple generation has failed!");

                dm.CheckInDataset(dataset.Id, "for testing purposes 2", "Javad", ViewCreationBehavior.None);
                dm.SyncView(dataset.Id, ViewCreationBehavior.Create | ViewCreationBehavior.Refresh);

                dataset.Id.Should().BeGreaterThan(0, "Dataset was not persisted.");
                dataset.LastCheckIOTimestamp.Should().NotBeAfter(DateTime.UtcNow, "The dataset's timestamp is wrong.");
                dataset.DataStructure.Should().NotBeNull("Dataset must have a data structure.");
                dataset.Status.Should().Be(DatasetStatus.CheckedIn, "Dataset must be in the CheckedIn status.");
                dm.GetDatasetLatestVersionEffectiveTupleCount(dataset.Id).Should().Be(numberOfTuples);

                // pass this filter to get a subset of dataset X
                var dst = dm.GetLatestDatasetVersionTuples(dataset.Id, fex, null, null, "", 1, 10);
                dst.Should().NotBeNull();
                dst.Rows.Count.Should().BeLessOrEqualTo(10);

                dm.DatasetVersionRepo.Evict();
                dm.DataTupleRepo.Evict();
                dm.DatasetRepo.Evict();
                dm.PurgeDataset(dataset.Id, true);

                dsHelper.PurgeAllDataStructures();
            }
            finally
            {
                dm.Dispose();
                rsm.Dispose();
                mdm.Dispose();
                etm.Dispose();
            }
        }

        [Test()]
        public void ProjectExpressionTest()
        {
            var dsHelper = new DatasetHelper();
            StructuredDataStructure dataStructure = dsHelper.CreateADataStructure();
            dataStructure.Should().NotBeNull("Failed to meet a precondition: a data strcuture is required.");

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
            var etm = new EntityTemplateManager();

            try
            {
                dataStructure.Should().NotBeNull("Failed to meet a precondition: a data strcuture is required.");

                var rp = dsHelper.CreateResearchPlan();
                rp.Should().NotBeNull("Failed to meet a precondition: a research plan is required.");

                var mds = mdm.Repo.Query().First();
                mds.Should().NotBeNull("Failed to meet a precondition: a metadata strcuture is required.");
                var et = etm.Repo.Query().First();
                et.Should().NotBeNull("Failed to meet a precondition: a entity template is required.");

                Dataset dataset = dm.CreateEmptyDataset(dataStructure, rp, mds, et);
                dataset = dsHelper.GenerateTuplesForDataset(dataset, dataStructure, numberOfTuples, "Javad");
                dataset.Should().NotBeNull("The dataset tuple generation has failed!");

                dm.CheckInDataset(dataset.Id, "for testing purposes 2", "Javad", ViewCreationBehavior.None);
                dm.SyncView(dataset.Id, ViewCreationBehavior.Create | ViewCreationBehavior.Refresh);

                dataset.Id.Should().BeGreaterThan(0, "Dataset was not persisted.");
                dataset.LastCheckIOTimestamp.Should().NotBeAfter(DateTime.UtcNow, "The dataset's timestamp is wrong.");
                dataset.DataStructure.Should().NotBeNull("Dataset must have a data structure.");
                dataset.Status.Should().Be(DatasetStatus.CheckedIn, "Dataset must be in the CheckedIn status.");
                dm.GetDatasetLatestVersionEffectiveTupleCount(dataset.Id).Should().Be(numberOfTuples);

                // pass this filter to get a subset of dataset X
                var dst = dm.GetLatestDatasetVersionTuples(dataset.Id, null, null, projectionExpression, "", 1, 3);
                dst.Should().NotBeNull();
                dst.Rows.Count.Should().BeLessOrEqualTo(3);
                dst.Columns.Count.Should().BeLessOrEqualTo(3, "Projection failed, wrong number of columns");

                dm.DatasetVersionRepo.Evict();
                dm.DataTupleRepo.Evict();
                dm.DatasetRepo.Evict();
                dm.PurgeDataset(dataset.Id, true);
                dsHelper.PurgeAllDataStructures();
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
                etm.Dispose();
            }
        }

        [Test()]
        public void CheckedIn_NoErrors_StatusShouldCheckInAfterUpdate()
        {
            using (var dm = new DatasetManager())
            using (var rsm = new ResearchPlanManager())
            using (var mdm = new MetadataStructureManager())
            using (var etm = new EntityTemplateManager())
            {
                //Arrange
                var dsHelper = new DatasetHelper();
                StructuredDataStructure dataStructure = dsHelper.CreateADataStructure();
                dataStructure.Should().NotBeNull("Failed to meet a precondition: a data strcuture is required.");

                var rp = dsHelper.CreateResearchPlan();
                rp.Should().NotBeNull("Failed to meet a precondition: a research plan is required.");

                var mds = mdm.Repo.Query().First();
                mds.Should().NotBeNull("Failed to meet a precondition: a metadata strcuture is required.");

                var et = etm.Repo.Query().First();
                et.Should().NotBeNull("Failed to meet a precondition: a entity template is required.");

                Dataset dataset = dm.CreateEmptyDataset(dataStructure, rp, mds, et);

                // Act
                if (dm.IsDatasetCheckedOutFor(dataset.Id, "David") || dm.CheckOutDataset(dataset.Id, "David"))
                {
                    var workingCopy = dm.GetDatasetWorkingCopy(dataset.Id);
                    dm.EditDatasetVersion(workingCopy, null, null, null);

                    dm.CheckInDataset(dataset.Id, "no update on data tuples", "David", ViewCreationBehavior.None);
                }

                // Assert
                dataset.Status.Should().Be(DatasetStatus.CheckedIn, "Dataset must be in CheckedIn status.");

                dm.PurgeDataset(dataset.Id);
                dsHelper.PurgeAllDataStructures();
            }
        }

        [Test()]
        public void CheckedOut_NoErrors_StatusShouldCheckOutAfterUpdate()
        {
            using (var dm = new DatasetManager())
            using (var rsm = new ResearchPlanManager())
            using (var mdm = new MetadataStructureManager())
            using (var etm = new EntityTemplateManager())
            {
                //Arrange
                var dsHelper = new DatasetHelper();
                StructuredDataStructure dataStructure = dsHelper.CreateADataStructure();
                dataStructure.Should().NotBeNull("Failed to meet a precondition: a data strcuture is required.");

                var rp = dsHelper.CreateResearchPlan();
                rp.Should().NotBeNull("Failed to meet a precondition: a research plan is required.");

                var mds = mdm.Repo.Query().First();
                mds.Should().NotBeNull("Failed to meet a precondition: a metadata strcuture is required.");

                var et = etm.Repo.Query().First();
                et.Should().NotBeNull("Failed to meet a precondition: a entity template is required.");

                Dataset dataset = dm.CreateEmptyDataset(dataStructure, rp, mds, et);

                // Act
                if (dm.IsDatasetCheckedOutFor(dataset.Id, "David") || dm.CheckOutDataset(dataset.Id, "David"))
                {
                    // Assert
                    dataset.Status.Should().Be(DatasetStatus.CheckedOut, "Dataset must be in CheckedOut status.");
                }

                dm.PurgeDataset(dataset.Id);
                dsHelper.PurgeAllDataStructures();
            }
        }

        [Test()]
        public void UndoCheckout_NoErrors_StatusShouldCheckIn()
        {
            using (var dm = new DatasetManager())
            using (var rsm = new ResearchPlanManager())
            using (var mdm = new MetadataStructureManager())
            using (var etm = new EntityTemplateManager())
            {
                try
                {
                    //Arrange
                    var dsHelper = new DatasetHelper();
                    StructuredDataStructure dataStructure = dsHelper.CreateADataStructure();
                    dataStructure.Should().NotBeNull("Failed to meet a precondition: a data strcuture is required.");

                    var rp = dsHelper.CreateResearchPlan();
                    rp.Should().NotBeNull("Failed to meet a precondition: a research plan is required.");

                    var mds = mdm.Repo.Query().First();
                    mds.Should().NotBeNull("Failed to meet a precondition: a metadata strcuture is required.");

                    var et = etm.Repo.Query().First();
                    et.Should().NotBeNull("Failed to meet a precondition: a entity template is required.");

                    Dataset dataset = dm.CreateEmptyDataset(dataStructure, rp, mds, et);

                    // Act
                    if (dm.IsDatasetCheckedOutFor(dataset.Id, "David") || dm.CheckOutDataset(dataset.Id, "David"))
                    {
                        var workingCopy = dm.GetDatasetWorkingCopy(dataset.Id);
                        dm.EditDatasetVersion(workingCopy, null, null, null);

                        dm.CheckInDataset(dataset.Id, "no update on data tuples", "David", ViewCreationBehavior.None);
                    }

                    long count = dm.GetDatasetVersionCount(dataset.Id);

                    // Act
                    if (dm.IsDatasetCheckedOutFor(dataset.Id, "David") || dm.CheckOutDataset(dataset.Id, "David"))
                    {
                        var workingCopy = dm.GetDatasetWorkingCopy(dataset.Id);
                        dm.EditDatasetVersion(workingCopy, null, null, null);
                        //dm.CheckInDataset(dataset.Id, "no update on data tuples", "David", ViewCreationBehavior.None);

                        dm.UndoCheckoutDataset(dataset.Id, "David", ViewCreationBehavior.None);
                    }

                    long countAfterUndo = dm.GetDatasetVersionCount(dataset.Id);
                    var lastestVersion = dm.GetDatasetLatestVersion(dataset.Id);

                    // Assert
                    dataset.Status.Should().Be(DatasetStatus.CheckedIn, "Dataset must be in CheckedIn status.");
                    lastestVersion.Status.Should().Be(DatasetVersionStatus.CheckedIn, "Dataset version must be in CheckedIn status.");
                    Assert.That(count, Is.EqualTo(countAfterUndo));

                    dm.PurgeDataset(dataset.Id);
                    dsHelper.PurgeAllDataStructures();
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
        }

        [Test()]
        public void UpdateSingleValueInMetadata_valid_updatedValue()
        {
            using (var dm = new DatasetManager())
            using (var rsm = new ResearchPlanManager())
            using (var mdm = new MetadataStructureManager())
            using (var etm = new EntityTemplateManager())
            {
                try
                {
                    //Arrange
                    var dsHelper = new DatasetHelper();

                    var dataset = dsHelper.CreateDatasetWithMetadata();
     
                    var version = dm.GetDatasetLatestVersion(dataset.Id);
                    // Act
                    //string xpath = "Metadata/Basic/BasicType/alternateIdentifier/alternateIdentifierType";
                    string xpath = "Metadata/Basic/BasicType/DatasetGUID/DatasetGUIDType";
                    string value = "new doi"+DateTime.Now.ToString();

                    dm.UpdateSingleValueInMetadata(version.Id, xpath, value);

                    //assert
                    version = dm.GetDatasetLatestVersion(dataset.Id);
                    string valueFromDb = dm.GetMetadataValueFromDatasetVersion(version.Id, xpath);
                    Assert.AreEqual(value, valueFromDb);

                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
        }

        [Test()]
        public void UpdateValueInMetadata_valid_updatedValue()
        {
            using (var dm = new DatasetManager())
            using (var rsm = new ResearchPlanManager())
            using (var mdm = new MetadataStructureManager())
            using (var etm = new EntityTemplateManager())
            {
                try
                {
                    //Arrange
                    var dsHelper = new DatasetHelper();

                    var dataset = dsHelper.CreateDatasetWithMetadata();

                    var version = dm.GetDatasetLatestVersion(dataset.Id);
                    // Act
                    //string xpath = "Metadata/Basic/BasicType/alternateIdentifier/alternateIdentifierType";
                    string xpath = "Metadata/Basic/BasicType/DatasetGUID/DatasetGUIDType";
                    string value = "new doi" + DateTime.Now.ToString();
                    string value2 = "new doi new";

                    dm.UpdateSingleValueInMetadata(version.Id, xpath, value);

                    version = dm.GetDatasetLatestVersion(dataset.Id);
                    dm.UpdateValueInMetadata(version.Id, xpath, value2);

                    //assert
                    version = dm.GetDatasetLatestVersion(dataset.Id);

                    var l = version.Metadata.SelectNodes(xpath);
                    Assert.AreEqual(l.Count, 2);


                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
        }

        [Test()]
        public void UpdateValueInMetadata_lastValueEmpty_updatedValueFromLast()
        {
            using (var dm = new DatasetManager())
            using (var rsm = new ResearchPlanManager())
            using (var mdm = new MetadataStructureManager())
            using (var etm = new EntityTemplateManager())
            {
                try
                {
                    //Arrange
                    var dsHelper = new DatasetHelper();

                    var dataset = dsHelper.CreateDatasetWithMetadata();

                    var version = dm.GetDatasetLatestVersion(dataset.Id);
                    // Act
                    //string xpath = "Metadata/Basic/BasicType/alternateIdentifier/alternateIdentifierType";
                    string xpath = "Metadata/Basic/BasicType/DatasetGUID/DatasetGUIDType";
                    string value = "";
                    string value2 = "new doi new";

                    dm.UpdateSingleValueInMetadata(version.Id, xpath, value);

                    version = dm.GetDatasetLatestVersion(dataset.Id); // grab new version

                    dm.UpdateValueInMetadata(version.Id, xpath, value2);

                    //assert
                    version = dm.GetDatasetLatestVersion(dataset.Id);

                    var l = version.Metadata.SelectNodes(xpath);
                    Assert.AreEqual(l.Count, 1);

                    string valueFromDb = dm.GetMetadataValueFromDatasetVersion(version.Id, xpath);
                    Assert.AreEqual(value2, valueFromDb);

                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
        }
    }
}