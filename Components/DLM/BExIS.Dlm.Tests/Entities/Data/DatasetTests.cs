using NUnit.Framework;
using BExIS.Dlm.Entities.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BExIS.Dlm.Services.Data;
using BExIS.Dlm.Services.DataStructure;
using BExIS.Dlm.Services.Administration;
using BExIS.App.Testing;

namespace BExIS.Dlm.Tests.Entities.Data
{
    [TestFixture()]
    public class DatasetTests
    {
        [OneTimeSetUp]
        /// It is called once prior to executing any of the tests in a fixture.
        /// Multiple methods can be marked. Order is not preserved. 
        /// Inheritance is supported, call sequence form the parents
        public void OneTimeSetUp()
        {
            // because these tests are working on in-memory objects (datasets) only, there is no need to do the test app setup
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
        {}

        [Test()]
        public void InstantiateDatasetTest()
        {
            Dataset dataset = new Dataset();
            Assert.That(dataset, Is.Not.Null);
            Assert.That(dataset.DataStructure, Is.Not.Null, "Dataset must have a data structure.");
            Assert.That(dataset.Status, Is.EqualTo(DatasetStatus.CheckedIn), "Dataset must be in CheckedIn status.");
        }

        [Test]
        public void InstantiateDatasetWithoutDataStructureTest()
        {
            // Test pass means the exception has been thrown and caught properly.
            Assert.Throws(typeof(ArgumentNullException), delegate { new Dataset(null); });
        }
    }
}