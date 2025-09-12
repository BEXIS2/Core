using BExIS.IO.Transform.Input;
using NUnit.Framework;
using System;
using System.Collections.Generic;

namespace BExIS.IO.Tests.Transform.Input
{
    public class StructureAnalyserTest
    {
        private List<string> rows = new List<string>();
        private List<string> rowWithMissingValues = new List<string>();
        private List<string> missingValueList = new List<string>() { "na" };

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            rows = generateTestRows(100, ';');
            rowWithMissingValues = generateTestRows(100, ';', missingValueList);
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

        #region delimeter

        [TestCase("a,b,c", "2.23,2.34,2", TextSeperator.comma)]
        [TestCase("v1;v2;v3", "2.23;a,b,c,d,e,f;2", TextSeperator.semicolon)]
        [TestCase("v1\tv2\tv3", "2.23\t2.34\t2", TextSeperator.tab)]
        public void SuggestDelimeter_Valid_ResultTextSeperator(string rowA, string rowB, TextSeperator textSeperator)
        {
            //Arrange
            StructureAnalyser structureAnalyser = new StructureAnalyser();

            //Act
            var result = structureAnalyser.SuggestDelimeter(rowA, rowB);

            //Assert
            Assert.NotNull(result, "result should not be null.");
            Assert.AreEqual(result, textSeperator, "textseperator not expected");
        }

        [TestCase("a,\"b\",c", "2.23,\"2.34,2.24\",2", TextSeperator.comma, TextMarker.doubleQuotes)]
        [TestCase("a,'b',c", "2.23,'2.34,2.24',2", TextSeperator.comma, TextMarker.quotes)]
        public void SuggestDelimeter_DelimeterInTextMarkers_ResultTextSeperator(string rowA, string rowB, TextSeperator textSeperator, TextMarker textMarker)
        {
            //Arrange
            StructureAnalyser structureAnalyser = new StructureAnalyser();

            //Act
            var result = structureAnalyser.SuggestDelimeter(rowA, rowB, textMarker);

            //Assert
            Assert.NotNull(result, "result should not be null.");
            Assert.AreEqual(result, textSeperator, "textseperator not expected");
        }

        [TestCase("a,\"b\",c", "2.23,\"a;b;c\",2", TextSeperator.comma, TextMarker.doubleQuotes)]
        public void SuggestDelimeter_CharInTextMarkersEqualToDelimeter_ResultTextSeperator(string rowA, string rowB, TextSeperator textSeperator, TextMarker textMarker)
        {
            //Arrange
            StructureAnalyser structureAnalyser = new StructureAnalyser();

            //Act
            var result = structureAnalyser.SuggestDelimeter(rowA, rowB, TextMarker.doubleQuotes);

            //Assert
            Assert.NotNull(result, "result should not be null.");
            Assert.AreEqual(result, textSeperator, "textseperator not expected");
        }

        [Test()]
        public void SuggestDelimeter_EmptyRows_ArgumentNullException()
        {
            //Arrange
            StructureAnalyser structureAnalyser = new StructureAnalyser();

            //Act
            var result = Assert.Throws<ArgumentNullException>(() => structureAnalyser.SuggestDelimeter("", ""));

            //Assert
            Assert.AreEqual(result.ParamName, "rowA");
        }

        [TestCase("a,b,c", "2.23,2.342")]
        [TestCase("v1;v2;v3", "2.23;")]
        [TestCase("v1\tv2", "2.23\t2.34\t2")]
        public void SuggestDelimeter_NoPossibleTextSeperator_Exception(string rowA, string rowB)
        {
            //Arrange
            StructureAnalyser structureAnalyser = new StructureAnalyser();

            //Act
            var result = Assert.Throws<Exception>(() => structureAnalyser.SuggestDelimeter(rowA, rowB));

            //Assert
            Assert.AreEqual(result.Message, "the guessing of the operator came to no result.");
        }

        #endregion delimeter

        #region decimal

        [TestCase("a,b,c", "2.23,2.34,2", TextSeperator.comma, DecimalCharacter.point)]
        [TestCase("v1;v2;v3", "2.23;a,b,c,d,e,f;2", TextSeperator.semicolon, DecimalCharacter.point)]
        [TestCase("v1\tv2\tv3", "2.23\t2.34\t2", TextSeperator.tab, DecimalCharacter.point)]
        [TestCase("v1\tv2\tv3", "2.23\t2.34\t2", TextSeperator.tab, DecimalCharacter.point)]
        public void SuggestDecimal_Valid_ResultDecimalCharacter(string rowA, string rowB, TextSeperator textSeperator, DecimalCharacter decimalCharacter)
        {
            //Arrange
            StructureAnalyser structureAnalyser = new StructureAnalyser();

            //Act
            var result = structureAnalyser.SuggestDecimal(rowA, rowB, textSeperator);

            //Assert
            Assert.NotNull(result, "result should not be null.");
            Assert.AreEqual(result, decimalCharacter, "decimal character not expected");
        }

        [TestCase("a;b;c", "100,000,002.23;2.34;2", TextSeperator.semicolon, DecimalCharacter.point)]
        [TestCase("a\tb\tc", "100,2\t\"der hut ist...\"\t2.409,8", TextSeperator.tab, DecimalCharacter.comma)]
        [TestCase("a\tb\tc", "1,100.2\t\"der hut ist...\"\t2,400.8", TextSeperator.tab, DecimalCharacter.point)]
        [TestCase("a\tb\tc", "1.100,2\t\"der hut ist...\"\t2.400,8", TextSeperator.tab, DecimalCharacter.comma)]
        public void SuggestDecimal_DecimalWithThousandSpot_ResultDecimalCharacter(string rowA, string rowB, TextSeperator textSeperator, DecimalCharacter decimalCharacter)
        {
            //Arrange
            StructureAnalyser structureAnalyser = new StructureAnalyser();

            //Act
            var result = structureAnalyser.SuggestDecimal(rowA, rowB, textSeperator);

            //Assert
            Assert.NotNull(result, "result should not be null.");
            Assert.AreEqual(result, decimalCharacter, "decimal character not expected");
        }

        [Test()]
        public void SuggestDecimal_EmptyRows_ArgumentNullException()
        {
            //Arrange
            StructureAnalyser structureAnalyser = new StructureAnalyser();

            //Act
            var result = Assert.Throws<ArgumentNullException>(() => structureAnalyser.SuggestDecimal("", "", TextSeperator.tab));

            //Assert
            Assert.AreEqual(result.ParamName, "rowA");
        }

        #endregion decimal

        #region textmarker

        [TestCase("'a',b,'c'", "2.23,2.34,'2'", TextMarker.quotes)]
        public void SuggestTextMarker_Valid_ResultTextSeperator(string rowA, string rowB, TextMarker expect)
        {
            //Arrange
            StructureAnalyser structureAnalyser = new StructureAnalyser();

            //Act
            var result = structureAnalyser.SuggestTextMarker(rowA, rowB);

            //Assert
            Assert.NotNull(result, "result should not be null.");
            Assert.AreEqual(expect, result, "text marker not expected");
        }

        [TestCase("'a',b,'c'", "2.23,'text with \\' and more chars','2'", TextMarker.quotes)]
        public void SuggestTextMarker_QuotesWithBackslash_ResultTextSeperator(string rowA, string rowB, TextMarker expect)
        {
            //Arrange
            StructureAnalyser structureAnalyser = new StructureAnalyser();

            //Act
            var result = structureAnalyser.SuggestTextMarker(rowA, rowB);

            //Assert
            Assert.NotNull(result, "result should not be null.");
            Assert.AreEqual(expect, result, "text marker not expected");
        }

        [TestCase("a,b,c", "2.23,text with and more chars,2", TextMarker.none)]
        public void SuggestTextMarker_noTextMarker_ResultTextSeperator(string rowA, string rowB, TextMarker expect)
        {
            //Arrange
            StructureAnalyser structureAnalyser = new StructureAnalyser();

            //Act
            var result = structureAnalyser.SuggestTextMarker(rowA, rowB);

            //Assert
            Assert.NotNull(result, "result should not be null.");
            Assert.AreEqual(expect, result, "text marker not expected");
        }

        [TestCase(34,TextMarker.doubleQuotes, '"')]
        [TestCase(39,TextMarker.quotes, '\'')]
        [TestCase(0,TextMarker.none, '\0')]
        public void GetTextMarker_basedOnInteger_ResultTextSeperator(int tMarker, TextMarker expect, char expectedChar)
        {
            //Arrange
            AsciiFileReaderInfo asciiFileReaderInfoHelper = new AsciiFileReaderInfo();

            //Act
            var result = AsciiFileReaderInfo.GetTextMarker(tMarker);
            var charResult = AsciiFileReaderInfo.GetTextMarker(result);



            //Assert
            Assert.NotNull(result, "result should not be null.");
            Assert.AreEqual(expect, result, "text marker not expected");
            Assert.AreEqual(expectedChar, charResult, "char not expected");
        }

        [Test()]
        public void SuggestTextMarker_EmptyRows_ArgumentNullException()
        {
            //Arrange
            StructureAnalyser structureAnalyser = new StructureAnalyser();

            //Act
            var result = Assert.Throws<ArgumentNullException>(() => structureAnalyser.SuggestTextMarker("", ""));

            //Assert
            Assert.AreEqual(result.ParamName, "rowA");
        }

        #endregion textmarker

        #region systemtypes

        [TestCase(1)]
        public void SuggestSystemTypes_Valid_ResultWithCorrectTypes(int n)
        {
            //Arrange
            StructureAnalyser structureAnalyser = new StructureAnalyser();

            //Act
            var result = structureAnalyser.SuggestSystemTypes(rows.GetRange(0, n), TextSeperator.semicolon, DecimalCharacter.comma, new List<string>());

            //Assert
            Assert.NotNull(result);

            var v1 = result[0];
            Assert.That(v1.Equals(typeof(Int64)));

            var v2 = result[1];
            Assert.That(v2.Equals(typeof(String)));

            var v3 = result[2];
            Assert.That(v3.Equals(typeof(Double)));

            var v4 = result[3];
            Assert.That(v4.Equals(typeof(Boolean)));

            var v5 = result[4];
            Assert.That(v5.Equals(typeof(DateTime)));

            var v6 = result[5];
            Assert.That(v6.Equals(typeof(Int64)));
        }

        [Test]
        public void SuggestSystemTypes_WithTestData_ResultWithCorrectTypes()
        {
            List<string> rows = new List<string>();
            rows.Add("1;1039;Villiger;09/19/22");
            rows.Add("1;1039;villager;09/19/22");
            rows.Add("1;1324;villager;09/19/22");
            rows.Add("1;1155;villager;09/19/22");
            rows.Add("1;1344;villager;09/19/22");
            rows.Add("1;1380;villager;09/19/22");
            rows.Add("1;1072;villager;09/19/22");
            rows.Add("1;1151;villager;09/19/22");
            rows.Add("1;1070;villager;09/19/22");
            rows.Add("1;1069;villager;09/19/22");
            rows.Add("1;1173;villager;09/19/22");
            rows.Add("1;1143;villager;09/19/22");

            //Arrange
            StructureAnalyser structureAnalyser = new StructureAnalyser();

            //Act
            var result = structureAnalyser.SuggestSystemTypes(rows, TextSeperator.semicolon, DecimalCharacter.comma, new List<string>());

            //Assert
            Assert.NotNull(result);

            var v1 = result[0];
            Assert.That(v1.Equals(typeof(Int64)));

            var v2 = result[1];
            Assert.That(v2.Equals(typeof(Int64)));

            var v3 = result[2];
            Assert.That(v3.Equals(typeof(String)));

            var v4 = result[3];
            Assert.That(v4.Equals(typeof(DateTime)));
        }

        public void SuggestSystemTypes_ValidDateTypes_ResultWithCorrectTypes(int n)
        {
            //Arrange
            StructureAnalyser structureAnalyser = new StructureAnalyser();

            List<string> dateValues = new List<string>();
            dateValues.Add("2022-12-24"); // yyyy-MM-dd

            //Act
            var result = structureAnalyser.SuggestSystemTypes(rows.GetRange(0, n), TextSeperator.semicolon, DecimalCharacter.comma, new List<string>());

            //Assert
            Assert.NotNull(result);
        }

        [Test]
        public void SuggestSystemTypes_ValidWithMissingValues_ResultWithCorrectTypes()
        {
            //Arrange
            StructureAnalyser structureAnalyser = new StructureAnalyser();

            //Act
            var result = structureAnalyser.SuggestSystemTypes(rowWithMissingValues, TextSeperator.semicolon, DecimalCharacter.comma, missingValueList);

            //Assert
            Assert.NotNull(result);

            var v1 = result[0];
            Assert.That(v1.Equals(typeof(Int64)), "is not Int64");

            var v2 = result[1];
            Assert.That(v2.Equals(typeof(String)), "is not String");

            var v3 = result[2];
            Assert.That(v3.Equals(typeof(Double)), "is not Double");

            var v4 = result[3];
            Assert.That(v4.Equals(typeof(Boolean)), "is not Boolean");

            var v5 = result[4];
            Assert.That(v5.Equals(typeof(DateTime)), "is not DateTime");

            var v6 = result[5];
            Assert.That(v6.Equals(typeof(Int64)), "is not Int64");
        }

        #endregion systemtypes

        private List<string> generateTestRows(int number, char seperator, List<string> missingValues = null)
        {
            List<string> rows = new List<string>();

            var r = new Random();

            if (missingValues != null)
            {
                foreach (var missingValue in missingValues)
                {
                    string row = missingValue + seperator;
                    row += missingValue + seperator;
                    row += missingValue + seperator;
                    row += missingValue + seperator;
                    row += missingValue + seperator;
                    row += missingValue;

                    rows.Add(row);
                }
            }

            for (int i = 0; i < number; i++)
            {
                string row = r.Next().ToString();

                row += seperator.ToString() + "Test";
                row += seperator.ToString() + r.NextDouble().ToString();
                row += seperator.ToString() + true.ToString();
                row += seperator.ToString() + DateTime.Now.ToString();
                row += seperator.ToString() + -3;

                rows.Add(row);
            }



            return rows;
        }


    }
}