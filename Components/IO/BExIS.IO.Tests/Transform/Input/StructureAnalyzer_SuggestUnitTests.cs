using BExIS.App.Testing;
using BExIS.IO.Transform.Input;
using BExIS.Utils.Config;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BExIS.IO.Tests.Transform.Input
{
    public class StructureAnalyzer_SuggestUnitTests
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

        [TestCase("nmol/g",3)]
        [TestCase("nmol-g",3)] // no excact but near to mol/kg
        [TestCase("nmlo/g",3)] // gramma mistake but near to mol/kg
        [TestCase("micromol per gram", 4)]
        [TestCase("none", 1)]
        [TestCase("m^2", 5)]
        [TestCase("µmol", 4)]
        public void SuggestUnit_AbbrOrNameAsInputDatatypeEmpty_ReturnUnit(string input, long id)
        {

            //Arrange
            StructureAnalyser structureAnalyser = new StructureAnalyser();

            //Act
            var result = structureAnalyser.SuggestUnit(input,input,"");

            //Assert
            Assert.NotNull(result, "result should not be null.");
            Assert.AreEqual(id, result.FirstOrDefault().Id);

        }

        [TestCase("double", 1, 62)] // datatype, id, count
        [TestCase("string", 1, 1)] // datatype, id, count
        public void SuggestUnit_InputIsEmptyButDatatypeExist_ReturnUnit(string dataType, long firstId, int count)
        {

            //Arrange
            StructureAnalyser structureAnalyser = new StructureAnalyser();

            //Act
            var result = structureAnalyser.SuggestUnit("","", dataType);

            //Assert
            Assert.NotNull(result, "result should not be null.");
            Assert.AreEqual(count,result.Count, "result list is not correct");
            Assert.AreEqual(firstId, result.OrderBy(d=>d.Id).FirstOrDefault().Id, "first entry is not correct");

        }

    }
}