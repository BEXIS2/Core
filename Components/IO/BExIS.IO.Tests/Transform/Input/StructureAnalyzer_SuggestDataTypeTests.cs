using BExIS.App.Testing;
using BExIS.IO.Transform.Input;
using BExIS.Utils.Config;
using NUnit.Framework;
using System;
using System.Linq;

namespace BExIS.IO.Tests.Transform.Input
{
    public class StructureAnalyzer_SuggestDataTypeTests
    {
        private TestSetupHelper helper = null;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            // because these tests are working on in-memory objects (datasets) only, there is no need to do the test app setup
            helper = new TestSetupHelper(WebApiConfig.Register, false);
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

        [Test]
        public void SuggestDataType_String_String()
        {
            //Arrange
            StructureAnalyser structureAnalyser = new StructureAnalyser();

            //Act
            var result = structureAnalyser.SuggestDataType(typeof(string).Name).FirstOrDefault();

            //Assert
            Assert.NotNull(result, "result should not be null.");
            Assert.NotNull(result.Name, "string");
        }

        [Test]
        public void SuggestDataType_Int16_number()
        {
            //Arrange
            StructureAnalyser structureAnalyser = new StructureAnalyser();

            //Act
            var result = structureAnalyser.SuggestDataType(typeof(Int16).Name).FirstOrDefault();

            //Assert
            Assert.NotNull(result, "result should not be null.");
            Assert.NotNull(result.Name, "Integer");
        }

        [Test]
        public void SuggestDataType_Int32_integer()
        {
            //Arrange
            StructureAnalyser structureAnalyser = new StructureAnalyser();

            //Act
            var result = structureAnalyser.SuggestDataType(typeof(Int32).Name).FirstOrDefault();

            //Assert
            Assert.NotNull(result, "result should not be null.");
            Assert.NotNull(result.Name, "Integer");
        }

        [Test]
        public void SuggestDataType_Boolean_bool()
        {
            //Arrange
            StructureAnalyser structureAnalyser = new StructureAnalyser();

            //Act
            var result = structureAnalyser.SuggestDataType(typeof(Boolean).Name).FirstOrDefault();

            //Assert
            Assert.NotNull(result, "result should not be null.");
            Assert.NotNull(result.Name, "bool");
        }

        [Test]
        public void SuggestDataType_Decimal_Decimal()
        {
            //Arrange
            StructureAnalyser structureAnalyser = new StructureAnalyser();

            //Act
            var result = structureAnalyser.SuggestDataType(typeof(decimal).Name).FirstOrDefault();

            //Assert
            Assert.NotNull(result, "result should not be null.");
            Assert.NotNull(result.Name, "Floating Point Number");
        }

        [Test]
        public void SuggestDataType_Double_Double()
        {
            //Arrange
            StructureAnalyser structureAnalyser = new StructureAnalyser();

            //Act
            var result = structureAnalyser.SuggestDataType(typeof(double).Name).FirstOrDefault();

            //Assert
            Assert.NotNull(result, "result should not be null.");
            Assert.NotNull(result.Name, "Floating Point Number");
        }

        [Test()]
        public void SuggestDataType_EmptySystemType_ArgumentNullException()
        {
            //Arrange
            StructureAnalyser structureAnalyser = new StructureAnalyser();

            //Act
            var result = Assert.Throws<ArgumentNullException>(() => structureAnalyser.SuggestDataType(""));

            //Assert
            Assert.AreEqual(result.ParamName, "systemType");
        }

        [Test()]
        public void SuggestDataType_TimeWithValue_Time()
        {
            //Arrange
            StructureAnalyser structureAnalyser = new StructureAnalyser();

            //Act
            var result = structureAnalyser.SuggestDataType(typeof(DateTime).Name, "12:39:32");

            //Assert
            Assert.NotNull(result, "result should not be null.");
        }

    }
}