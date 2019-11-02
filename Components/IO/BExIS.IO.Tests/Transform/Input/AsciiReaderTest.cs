using BExIS.App.Testing;
using BExIS.Dlm.Entities.DataStructure;
using BExIS.Dlm.Tests.Helpers;
using BExIS.IO.Transform.Input;
using BExIS.Utils.Config;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BExIS.IO.Tests.Transform.Input
{
    [TestFixture()]
    public class AsciiReaderTest
    {
        private DatasetHelper dsHelper = null;
        private TestSetupHelper helper = null;
        private StructuredDataStructure dataStructure;

        [OneTimeSetUp]
        /// It is called once prior to executing any of the tests in a fixture.
        /// Multiple methods can be marked. Order is not preserved.
        /// Inheritance is supported, call sequence form the parents
        public void OneTimeSetUp()
        {
            // because these tests are working on in-memory objects (datasets) only, there is no need to do the test app setup
            helper = new TestSetupHelper(WebApiConfig.Register, false);
            dsHelper = new DatasetHelper();
            dsHelper.PurgeAllDatasets();
            dsHelper.PurgeAllDataStructures();

            dataStructure = dsHelper.CreateADataStructure();
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

        [Test]
        public void textMarkerHandling_SeperatorInQuotes_ReturnExpectedListOfStrings()
        {
            //Arrange
            string row = "'V1';'V2';'V3;V4'";
            List<string> expectedOutcome = new List<string> { "V1", "V2", "V3;V4" };

            AsciiFileReaderInfo info = new AsciiFileReaderInfo();
            info.Seperator = TextSeperator.semicolon;

            AsciiReader reader = new AsciiReader(new StructuredDataStructure(), info);

            //Act

            List<string> values = reader.TextMarkerHandling(row,
                AsciiFileReaderInfo.GetSeperator(TextSeperator.semicolon),
                AsciiFileReaderInfo.GetTextMarker(TextMarker.quotes));

            //Assert

            Assert.That(values.Count, Is.EqualTo(3));
            Assert.That(values, Is.EquivalentTo(expectedOutcome));
        }

        [Test]
        public void textMarkerHandling_NotTextMarkerInRow_ReturnExpectedListOfStrings()
        {
            //Arrange
            string row = "V1;V2;V3;V4";
            List<string> expectedOutcome = new List<string> { "V1", "V2", "V3", "V4" };

            AsciiFileReaderInfo info = new AsciiFileReaderInfo();
            info.Seperator = TextSeperator.semicolon;

            AsciiReader reader = new AsciiReader(new StructuredDataStructure(), info);

            //Act

            List<string> values = reader.TextMarkerHandling(row,
                AsciiFileReaderInfo.GetSeperator(TextSeperator.semicolon),
                AsciiFileReaderInfo.GetTextMarker(TextMarker.quotes));

            //Assert

            Assert.That(values.Count, Is.EqualTo(expectedOutcome.Count));
            Assert.That(values, Is.EquivalentTo(expectedOutcome));
        }

        [Test]
        public void rowToList_RowAsQuotesAndSeperatorInQuotes_ReturnExpectedListOfStrings()
        {
            //Arrange
            string row = "'V1';'V2';'V3;V4'";
            List<string> expectedOutcome = new List<string> { "V1", "V2", "V3;V4" };

            AsciiFileReaderInfo info = new AsciiFileReaderInfo();
            info.Seperator = TextSeperator.semicolon;

            AsciiReader reader = new AsciiReader(new StructuredDataStructure(), info);

            //Act

            List<string> values = reader.rowToList(row,
                AsciiFileReaderInfo.GetSeperator(TextSeperator.semicolon));

            //Assert

            Assert.That(values.Count, Is.EqualTo(expectedOutcome.Count));
            Assert.That(values, Is.EquivalentTo(expectedOutcome));
        }
    }
}