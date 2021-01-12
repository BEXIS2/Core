using BExIS.App.Testing;
using BExIS.Dlm.Entities.DataStructure;
using BExIS.Dlm.Tests.Helpers;
using BExIS.IO.Tests.Helper;
using BExIS.IO.Transform.Input;
using BExIS.IO.Transform.Validation.DSValidation;
using BExIS.IO.Transform.Validation.Exceptions;
using BExIS.Utils.Config;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vaiona.Utils.Cfg;

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

        [Test]
        public void ValidateRow_runNotValid_LimitErrors()
        {

            //Arrange

            DataGeneratorHelper dgh = new DataGeneratorHelper();
            var errors = new List<Error>();
            var testData = dgh.GenerateRowsWithRandomValuesBasedOnDatastructureWithErrors(dataStructure, ",", 1000000, true);
            IEnumerable<string> vairableNames = dataStructure.Variables.Select(v => v.Label);
            //generate file to read
            Encoding encoding = Encoding.Default;
            string path = Path.Combine(AppConfiguration.DataPath, "testdataforvalidation.txt");
            if (File.Exists(path))
            {
                File.Delete(path);
            }

            using (StreamWriter sw = new StreamWriter(path))
            {
                string header = string.Join(",", vairableNames.ToArray());
                sw.WriteLine(header);

                foreach (var r in testData)
                {
                    sw.WriteLine(r);
                }
            }

            //Mock IOUtility
            var ioUtilityMock = new Mock<IOUtility>();
            ioUtilityMock.Setup(i => i.ConvertDateToCulture("2018")).Returns("2018");
            try
            {
                AsciiFileReaderInfo afr = new AsciiFileReaderInfo();
                afr.TextMarker = TextMarker.doubleQuotes;
                afr.Seperator = TextSeperator.comma;

                DataReader reader = new AsciiReader(dataStructure, afr, ioUtilityMock.Object);

                var asciireader = (AsciiReader)reader;
                //Act
                var row = new List<string>();

                using (Stream stream = reader.Open(path))
                {
                    asciireader.ValidateFile(stream, "", 1);
                }


                //Assert
                Assert.That(asciireader.ErrorMessages.Count, Is.EqualTo(1000));
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}