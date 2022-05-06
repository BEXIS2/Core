﻿using BExIS.App.Testing;
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

        [TestCase("mol/kg",4)]
        [TestCase("mol-kg",4)] // no excact but near to mol/kg
        [TestCase("mlo/kg",4)] // gramma mistake but near to mol/kg
        [TestCase("mole per kilogram", 4)]
        [TestCase("none", 1)]
        [TestCase("m^2", 8)]
        [TestCase("µmol", 54)]
        public void SuggestUnit_AbbrOrNameAsInputDatatypeEmpty_ReturnUnit(string input, long id)
        {

            //Arrange
            StructureAnalyser structureAnalyser = new StructureAnalyser();

            //Act
            var result = structureAnalyser.SuggestUnit(input,"");

            //Assert
            Assert.NotNull(result, "result should not be null.");
            Assert.AreEqual(id, result.FirstOrDefault().Id);

        }

        [TestCase("double", 2, 125)] // datatype, id, count
        [TestCase("string", 1, 3)] // datatype, id, count
        public void SuggestUnit_InputIsEmptyButDatatypeExist_ReturnUnit(string dataType, long firstId, int count)
        {

            //Arrange
            StructureAnalyser structureAnalyser = new StructureAnalyser();

            //Act
            var result = structureAnalyser.SuggestUnit("", dataType);

            //Assert
            Assert.NotNull(result, "result should not be null.");
            Assert.AreEqual(count,result.Count, "result list is not correct");
            Assert.AreEqual(firstId, result.OrderBy(d=>d.Id).FirstOrDefault().Id, "first entry is not correct");

        }

    }
}