using FluentAssertions;
using NUnit.Framework;
using System;

namespace BExIS.IO.Tests
{
    [TestFixture()]
    public class IOHelperTests
    {
        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
        }

        [SetUp]
        protected void SetUp()
        {
        }

        [TearDown]
        public void TearDown()
        {
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
        }

        [Test()]
        public void GetDynamicStorePathValuesTest(
            [Range(1, 2)] long datasetId, [Range(1, 2)] long datasetVersionOrderNr)
        {
            string path = IoHelper.GetDynamicStorePath(datasetId, datasetVersionOrderNr, "title", ".txt");

            path.Should().NotBeNull("Because path is not null.");
        }

        [TestCase(1, 2, "test", ".txt", ExpectedResult = @"Datasets\1\DatasetVersions\1_2_test.txt")]
        public string GetDynamicStorePathResultTest(long datasetId, long datasetVersionOrderNr, string title, string extention)
        {
            return IoHelper.GetDynamicStorePath(datasetId, datasetVersionOrderNr, title, extention);
        }

        [TestCase(-1, 1, "title", ".txt")]
        [TestCase(1, -1, "title", ".txt")]
        [TestCase(1, 1, "", ".txt")]
        [TestCase(1, 1, "title", "")]
        public void GetDynamicStorePathWidthExceptionTest(long datasetId, long datasetVersionOrderNr, string title, string extention)
        {
            Assert.Throws<Exception>(() => IoHelper.GetDynamicStorePath(datasetId, datasetVersionOrderNr, title, extention));
        }
    }
}