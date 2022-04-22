using BExIS.App.Testing;
using BExIS.Dlm.Entities.DataStructure;
using BExIS.Dlm.Tests.Helpers;
using BExIS.IO.Tests.Helper;
using BExIS.IO.Transform.Input;
using BExIS.IO.Transform.Validation.DSValidation;
using BExIS.IO.Transform.Validation.Exceptions;
using BExIS.Utils;
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

        [TestCase(100)]
        [TestCase(1000)]
        [TestCase(10000)]
        [TestCase(100000)]
        public void Count_FilexExist_ReturnCount(long numberOfRows)
        {
            DataGeneratorHelper dgh = new DataGeneratorHelper();
            var errors = new List<Error>();
            var testData = dgh.GenerateRowsWithRandomValuesBasedOnDatastructure(dataStructure, ",", numberOfRows, true);
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

            // Act
            var count = AsciiReader.Count(path);
            numberOfRows++; // add header

            // Assert
            Assert.AreEqual(numberOfRows, count,"Number of rows is not correct");

        }

        [TestCase(100,2)]
        [TestCase(1000,100)]
        [TestCase(10000,100)]
        [TestCase(100000,100)]
        public void GetRandowRows_FileExist_ReturnListOfStrings(int total, int selection)
        {
            DataGeneratorHelper dgh = new DataGeneratorHelper();
            var errors = new List<Error>();
            var testData = dgh.GenerateRowsWithRandomValuesBasedOnDatastructure(dataStructure, ",", total, true);
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

            // Act
            var result = AsciiReader.GetRandowRows(path,total,selection);

            // Assert
            Assert.AreEqual(result.Count, selection, "Number of rows is not correct");

        }

        [Test()]
        public void GetRandowRows_FileNotFoundExist_FileNotFoundException()
        {
            // Act

            var result = Assert.Throws<FileNotFoundException>(() => AsciiReader.GetRandowRows("c:/data/notexist.txt", 100, 1));

            // Assert
            Assert.AreEqual(result.Message, "file not found");
        }

        [Test()]
        public void GetRandowRows_FileNameIsEmpty_ArgumentNullException()
        {
            // Act

            var result = Assert.Throws<ArgumentNullException>(() => AsciiReader.GetRandowRows("", 100, 1));
            
            // Assert
            Assert.AreEqual(result.ParamName, "fileName");
            Assert.AreEqual(result.Message, "fileName not exist\r\nParametername: fileName");
        }

        [Test()]
        public void GetRandowRows_TotalIsZero_Exception()
        {
            // Act
            var result = Assert.Throws<Exception>(() => AsciiReader.GetRandowRows("filepath", 0, 1));

            // Assert
            Assert.AreEqual(result.Message, "total can not be 0");


        }

        [Test()]
        public void GetRandowRows_SelectionIsZero_Exception()
        {
            // Act
            var result = Assert.Throws<Exception>(() => AsciiReader.GetRandowRows("filepath", 100, 0));

            // Assert
            Assert.AreEqual(result.Message, "selection can not be 0");
        }

        [Test()]
        public void GetRandowRows_SelectionGreaterThenTotal_Exception()
        {
            // Act
            var result = Assert.Throws<Exception>(() => AsciiReader.GetRandowRows("filepath", 1, 2));

            // Assert
            Assert.AreEqual(result.Message, "total must be greater then selection");
        }


    }
}