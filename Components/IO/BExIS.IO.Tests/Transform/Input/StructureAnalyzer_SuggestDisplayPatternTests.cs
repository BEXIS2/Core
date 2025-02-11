using BExIS.App.Testing;
using BExIS.IO.Transform.Input;
using BExIS.Utils.Config;
using NUnit.Framework;
using System;
using System.Linq;

namespace BExIS.IO.Tests.Transform.Input
{
    public class StructureAnalyzer_SuggestDisplayPatternTests
    {
        private TestSetupHelper helper = null;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            //// because these tests are working on in-memory objects (datasets) only, there is no need to do the test app setup
            //helper = new TestSetupHelper(WebApiConfig.Register, false);
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

        [TestCase("11.12.2024","dd.MM.yyyy")]
        [TestCase("11.12.2024 1:23", "M/d/yyyy h:m")]
        [TestCase("11-12-2024T11:23:12", "yyyy-MM-ddThh:mm:ss")]
        public void SuggestDisplayPattern_valid_DisplayPattern(string input,string displayPattern)
        {
            //Arrange
            StructureAnalyser structureAnalyser = new StructureAnalyser();

            //Act
            var result = structureAnalyser.SuggestDisplayPattern(input);

            //Assert
            Assert.NotNull(result, "result should not be null.");
            Assert.AreEqual(result.DisplayPattern, displayPattern);
        }

        

    }
}