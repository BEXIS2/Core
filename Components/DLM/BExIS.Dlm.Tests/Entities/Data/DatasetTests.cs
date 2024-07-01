using BExIS.Dlm.Entities.Data;
using FluentAssertions;
using NUnit.Framework;
using System;

namespace BExIS.Dlm.Tests.Entities.Data
{
    [TestFixture()]
    public class DatasetTests
    {
        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            // because these tests are working on in-memory objects (datasets) only, there is no need to do the test app setup
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
        { }

        [Test()]
        public void InstantiateDatasetTest()
        {
            Dataset dataset = new Dataset();
            dataset.Should().NotBeNull(); // Assert.That(dataset, Is.Not.Null);
            dataset.DataStructure.Should().NotBeNull("Dataset must have a data structure."); // Assert.That(dataset.DataStructure, Is.Not.Null, "Dataset must have a data structure.");
            dataset.Status.Should().Be(DatasetStatus.CheckedIn, "Dataset must be in CheckedIn status."); // Assert.That(dataset.Status, Is.EqualTo(DatasetStatus.CheckedIn), "Dataset must be in CheckedIn status.");
        }

        [Test]
        public void InstantiateDatasetWithoutDataStructureAndEntityTemplateTest()
        {
            // Test pass means the exception has been thrown and caught properly.
            // Assert.Throws(typeof(ArgumentNullException), delegate { new Dataset(null); });

            Action action = () => new Dataset(null, null);
            action.Should().Throw<ArgumentNullException>()
                //.WithInnerException<ArgumentException>()
                .WithMessage("*without*entity*template.*");
        }
    }
}