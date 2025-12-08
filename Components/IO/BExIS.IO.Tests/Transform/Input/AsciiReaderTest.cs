using BExIS.App.Testing;
using BExIS.Dlm.Entities.DataStructure;
using BExIS.IO.Tests.Helper;
using BExIS.IO.Tests.Helpers;
using BExIS.IO.Transform.Input;
using BExIS.IO.Transform.Validation.Exceptions;
using BExIS.Utils.Config;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
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
        public void rowToList_RowAsOneVariablewithQuotesAndSeperatorInQuotes_ReturnExpectedListOfStrings()
        {
            //Arrange
            string row = "V1,V2,'V3,V4'";
            List<string> expectedOutcome = new List<string> { "V1", "V2", "V3,V4" };

            AsciiFileReaderInfo info = new AsciiFileReaderInfo();
            info.Seperator = TextSeperator.comma;

            AsciiReader reader = new AsciiReader(new StructuredDataStructure(), info);

            //Act

            List<string> values = reader.rowToList(row,
                AsciiFileReaderInfo.GetSeperator(TextSeperator.comma));

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
            Encoding encoding = Encoding.UTF8;
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

        [TestCase(10)]
        public void Count_FilexExist_ReturnCount(long numberOfRows)
        {
            DataGeneratorHelper dgh = new DataGeneratorHelper();
            var errors = new List<Error>();
            var testData = dgh.GenerateRowsWithRandomValuesBasedOnDatastructure(dataStructure, ",", numberOfRows, true);
            IEnumerable<string> vairableNames = dataStructure.Variables.Select(v => v.Label);

            //generate file to read
            Encoding encoding = Encoding.UTF8;
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
            Assert.AreEqual(numberOfRows, count, "Number of rows is not correct");
        }

        [Test]
        public void Count_FileNotExist_Count()
        {
            // Act
            var result = Assert.Throws<FileNotFoundException>(() => AsciiReader.Count("c:/data/notexist.txt"));

            // Assert
            Assert.AreEqual(result.Message, "file not found");
        }

        [Test]
        public void Count_FileNameIsEmpty_ArgumentNullException()
        {
            // Act
            var result = Assert.Throws<ArgumentNullException>(() => AsciiReader.Skipped(""));

            // Assert
            Assert.AreEqual(result.ParamName, "fileName");
        }

        [Test]
        public void Skipped_Valid_Count()
        {
            // Arrange
            List<string> rows = new List<string>();
            rows.Add("");
            rows.Add("");
            rows.Add("");
            rows.Add("abcd");
            rows.Add("");
            rows.Add("abcd2");

            //generate file to read
            Encoding encoding = Encoding.UTF8;
            string path = Path.Combine(AppConfiguration.DataPath, "test_skipped.txt");
            if (File.Exists(path))
            {
                File.Delete(path);
            }

            using (StreamWriter sw = new StreamWriter(path))
            {
                foreach (var r in rows)
                {
                    sw.WriteLine(r);
                }
            }

            // Act
            int skipped = AsciiReader.Skipped(path);

            //Assert
            Assert.AreEqual(skipped, 3, "the skipped lines are less or more than expected");
        }

        [Test]
        public void Skipped_FileNotExist_FileNotFoundException()
        {
            // Act
            var result = Assert.Throws<FileNotFoundException>(() => AsciiReader.Skipped("c:/data/notexist.txt"));

            // Assert
            Assert.AreEqual(result.Message, "file not found");
        }

        [Test]
        public void Skipped_FileNameIsEmpty_ArgumentNullException()
        {
            // Act
            var result = Assert.Throws<ArgumentNullException>(() => AsciiReader.Skipped(""));

            // Assert
            Assert.AreEqual(result.ParamName, "fileName");
        }

        [TestCase(100, 2)]
        [TestCase(1000, 100)]
        public void GetRandowRows_FileExist_ReturnListOfStrings(int total, int selection)
        {
            DataGeneratorHelper dgh = new DataGeneratorHelper();
            var errors = new List<Error>();
            var testData = dgh.GenerateRowsWithRandomValuesBasedOnDatastructure(dataStructure, ",", total, true);
            IEnumerable<string> vairableNames = dataStructure.Variables.Select(v => v.Label);
            //generate file to read
            Encoding encoding = Encoding.UTF8;
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
            var result = AsciiReader.GetRandowRows(path, total, selection);

            // Assert
            Assert.AreEqual(result.Count, selection, "Number of rows is not correct");
        }

        [Test()]
        public void GetRandowRows_DataStartValid_ReturnListOfStrings()
        {
            //Arrange

            string header = "v1,v2,v3";
            string unit = "kg,cm,none";
            string description = "a,b,c,d";
            var data = new List<string>() { "0,01,02", "1,11,12", "2,21,22", "3,31,32" };

            //generate file to read
            Encoding encoding = Encoding.UTF8;
            string path = Path.Combine(AppConfiguration.DataPath, "test_getrows_indexlistempty.txt");
            if (File.Exists(path))
            {
                File.Delete(path);
            }

            using (StreamWriter sw = new StreamWriter(path))
            {
                sw.WriteLine("");
                sw.WriteLine("");
                sw.WriteLine("");
                sw.WriteLine(header);
                sw.WriteLine(unit);
                sw.WriteLine(description);
                data.ForEach(x => sw.WriteLine(x));
            }

            int startdata = 4; // get from user selection
            var total = AsciiReader.Count(path); //10
            var skipped = AsciiReader.Skipped(path);//3
            var dataCount = total - skipped - startdata + 1; // 4

            Assert.AreEqual(total, 10);
            Assert.AreEqual(skipped, 3);
            Assert.AreEqual(dataCount, 4);

            // Act
            var result = AsciiReader.GetRandowRows(path, total, dataCount, startdata);

            // Assert
            Assert.NotNull(result);
            Assert.AreEqual(result.Count, 4, "more or less rows then expected");
        }

        [Test()]
        public void GetRandowRows_DataHasEmptyRows_ReturnListOfStrings()
        {
            //Arrange

            string header = "v1,v2,v3";
            string unit = "kg,cm,none";
            string description = "a,b,c,d";
            var data = new List<string>() { "0,01,02", "1,11,12", "", "2,21,22", "3,31,32" };

            //generate file to read
            Encoding encoding = Encoding.UTF8;
            string path = Path.Combine(AppConfiguration.DataPath, "test_getrows_indexlistempty.txt");
            if (File.Exists(path))
            {
                File.Delete(path);
            }

            using (StreamWriter sw = new StreamWriter(path))
            {
                sw.WriteLine("");
                sw.WriteLine("");
                sw.WriteLine("");
                sw.WriteLine(header);
                sw.WriteLine(unit);
                sw.WriteLine(description);
                data.ForEach(x => sw.WriteLine(x));
            }

            var total = AsciiReader.Count(path); //11
            var skipped = AsciiReader.Skipped(path);//3
            int startdata = 4; // get from user selection
            var dataCount = total - startdata - skipped + 1; // 4

            Assert.AreEqual(total, 11);
            Assert.AreEqual(skipped, 3);
            Assert.AreEqual(dataCount, 5);

            // Act
            var result = AsciiReader.GetRandowRows(path, total, dataCount, startdata + skipped);

            // Assert
            Assert.NotNull(result);
            Assert.AreEqual(result.Count, 4, "more or less rows then expected");
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

        [Test()]
        public void GetRows__FileNameEmpty_ArgumentNullException()
        {
            // Act
            var result = Assert.Throws<ArgumentNullException>(() => AsciiReader.GetRows("", Encoding.UTF8));

            // Assert
            Assert.AreEqual(result.ParamName, "fileName");
        }

        [Test()]
        public void GetRows__FileNotExist_FileNotFoundException()
        {
            // Act
            var result = Assert.Throws<FileNotFoundException>(() => AsciiReader.GetRows("c:/data/notexist.txt", Encoding.UTF8));

            // Assert
            Assert.AreEqual(result.Message, "file not found");
        }

        [Test()]
        public void GetRows_FileNameEmpty_ArgumentNullException()
        {
            // Act
            var result = Assert.Throws<ArgumentNullException>(() => AsciiReader.GetRows("", Encoding.UTF8));

            // Assert
            Assert.AreEqual(result.ParamName, "fileName");
        }

        [Test()]
        public void GetRows_FileNotExist_FileNotFoundException()
        {
            // Act
            var result = Assert.Throws<FileNotFoundException>(() => AsciiReader.GetRows("c:/data/notexist.txt", Encoding.UTF8, new List<int>() { 0 }, null));

            // Assert
            Assert.AreEqual(result.Message, "file not found");
        }

        [Test()]
        public void GetRows_IndexListIsEmpty_ArgumentNullException()
        {
            //Arrange
            //generate file to read
            Encoding encoding = Encoding.UTF8;
            string path = Path.Combine(AppConfiguration.DataPath, "test_getrows_indexlistempty.txt");
            if (File.Exists(path))
            {
                File.Delete(path);
            }

            using (StreamWriter sw = new StreamWriter(path))
            {
                sw.WriteLine("test");
            }

            // Act
            var result = Assert.Throws<ArgumentNullException>(() => AsciiReader.GetRows(path, null, null));

            // Assert
            Assert.AreEqual(result.ParamName, "fileName");
        }

        [Test()]
        public void GetRows_GetRowsFromIndexList_ListOfRows()
        {
            //Arrange
            var data = new List<string>() { "0", "1", "2", "3" };
            var wantedRows = new List<int>() { 1, 3 };

            //generate file to read
            Encoding encoding = Encoding.UTF8;
            string path = Path.Combine(AppConfiguration.DataPath, "test_getrows_indexlistempty.txt");
            if (File.Exists(path))
            {
                File.Delete(path);
            }

            using (StreamWriter sw = new StreamWriter(path))
            {
                data.ForEach(x => sw.WriteLine(x));
            }

            // Act
            var result = AsciiReader.GetRows(path, Encoding.UTF8, wantedRows);

            // Assert
            Assert.NotNull(result);
            Assert.AreEqual(result.Count, wantedRows.Count, "more or less rows then expected");
            Assert.That(result[0], Is.EqualTo("1"));
            Assert.That(result[1], Is.EqualTo("3"));
        }

        [Test()]
        public void GetRows_GetSubsetOfRowsFromIndexList_ListOfRows()
        {
            //Arrange
            var data = new List<string>() { "0,01,02", "1,10,11", "2,20,21", "3,30,31" };
            var wantedRows = new List<int>() { 1, 3 };
            var activeCells = new List<bool>() { false, true, false };

            //generate file to read
            Encoding encoding = Encoding.UTF8;
            string path = Path.Combine(AppConfiguration.DataPath, "test_getsubsetrows_indexlistempty.txt");
            if (File.Exists(path))
            {
                File.Delete(path);
            }

            using (StreamWriter sw = new StreamWriter(path))
            {
                data.ForEach(x => sw.WriteLine(x));
            }

            // Act
            var result = AsciiReader.GetRows(path, Encoding.UTF8, wantedRows, activeCells, TextSeperator.comma);

            // Assert
            Assert.NotNull(result);
            Assert.AreEqual(result.Count, wantedRows.Count, "more or less rows then expected");
            Assert.That(result[0], Is.EqualTo("10"));
            Assert.That(result[1], Is.EqualTo("30"));
        }
    }
}