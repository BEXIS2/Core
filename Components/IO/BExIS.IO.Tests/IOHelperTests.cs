using NUnit.Framework;
using System.IO;
using BExIS.IO;
using Vaiona.Utils.Cfg;
using FluentAssertions;
using System.Collections.Generic;
using System;
using System.Globalization;

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
        public void GetDynamicStorePathTest()
        {
            string path = IoHelper.GetDynamicStorePath(1, 2, "test", ".txt");
            string exceptedResult = @"Datasets\1\DatasetVersions\1_2_test.txt";


            path.Should().BeEquivalentTo(exceptedResult,"because {0} not match with {1}", path, exceptedResult );

        }

        [Test()]
        public void GetDynamicStorePathWidthNoDatasetIdTest()
        {

            Assert.Throws<Exception>(() => IoHelper.GetDynamicStorePath(-1, 2, "test", ".txt"));
  
        }

        [Test()]
        public void GetDynamicStorePathWidthNoDatasetVersionNrTest()
        {

            Assert.Throws<Exception>(() => IoHelper.GetDynamicStorePath(1, -1, "test", ".txt"));

        }

        [Test()]
        public void GetDynamicStorePathWidthNoTitleTest()
        {
            Assert.Throws<Exception>(() => IoHelper.GetDynamicStorePath(1, -1, "", ".txt"));
        }

        [Test()]
        public void GetDynamicStorePathWidthNoExtentionsTest()
        {

            Assert.Throws<Exception>(() => IoHelper.GetDynamicStorePath(1, -1, "title", ""));
        }

    }
}
