using FluentAssertions;
using NUnit.Framework;
using System;

namespace BExIS.IO.Tests
{
    [TestFixture()]
    public class IOHelperTests
    {
        [OneTimeSetUp]
        /// It is called once prior to executing any of the tests in a fixture.
        /// Multiple methods can be marked. Order is not preserved. 
        /// Inheritance is supported, call sequence form the parents
        public void OneTimeSetUp()
        {

        }

        [SetUp]
        /// performs the initial setup for the tests. This runs once per test, NOT per class!
        protected void SetUp()
        {

        }

        [TearDown]
        /// performs the cleanup after each test
        public void TearDown()
        {

        }

        [OneTimeTearDown]
        /// It is called once after executing all the tests in a fixture.
        /// Multiple methods can be marked. Order is not preserved. 
        /// Inheritance is supported, call sequence form the children
        /// Executes only if: counterpart OneTimeSetUp exists and executed successfully.
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
