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
        private List<string> missingValueList = new List<string>() { "na"};

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            rows = generateTestRows(1000, ';');
            rowWithMissingValues = generateTestRows(1000, ';', missingValueList);
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

        [TestCase("a,b,c","2.23,2.34,2",TextSeperator.comma)]
        [TestCase("v1;v2;v3","2.23;a,b,c,d,e,f;2",TextSeperator.semicolon)]
        [TestCase("v1\tv2\tv3","2.23\t2.34\t2",TextSeperator.tab)]
        public void SuggestDelimeter_Valid_ResultTextSeperator(string rowA, string rowB , TextSeperator textSeperator)
        {

            //Arrange
            StructureAnalyser structureAnalyser = new StructureAnalyser();

            //Act
            var result = structureAnalyser.SuggestDelimter(rowA, rowB);

            //Assert
            Assert.NotNull(result, "result should not be null.");
            Assert.AreEqual(result,textSeperator,"textseperator not expected");
        }

        [Test()]
        public void SuggestDelimeter_EmptyRows_ArgumentNullException()
        {

            //Arrange
            StructureAnalyser structureAnalyser = new StructureAnalyser();

            //Act
            var result = Assert.Throws<ArgumentNullException> (()=> structureAnalyser.SuggestDelimter("", ""));

            //Assert
            Assert.AreEqual(result.Message, "row has no content to suggest.\r\nParametername: rowA");
        }

        [TestCase("a,b,c", "2.23,2.342")]
        [TestCase("v1;v2;v3", "2.23;")]
        [TestCase("v1\tv2", "2.23\t2.34\t2")]
        public void SuggestDelimeter_NoPossibleTextSeperator_Exception(string rowA, string rowB)
        {

            //Arrange
            StructureAnalyser structureAnalyser = new StructureAnalyser();

            //Act
            var result = Assert.Throws<Exception>(() => structureAnalyser.SuggestDelimter(rowA, rowB));


            //Assert
            Assert.AreEqual(result.Message, "the guessing of the operator came to no result.");
        }


        [Test]
        public void SuggestDataTypes_Valid_ResultWithCorrectTypes()
        {
            //Arrange
            StructureAnalyser structureAnalyser = new StructureAnalyser();

            //Act
            var result = structureAnalyser.SuggestDataTypes(rows, TextSeperator.semicolon, DecimalCharacter.comma, new List<string>());

            //Assert
            Assert.NotNull(result);

            var v1 = result[0];
            Assert.That(v1.Equals(typeof(UInt32)));
        
            var v2 = result[1];
            Assert.That(v2.Equals(typeof(String)));

            var v3 = result[2];
            Assert.That(v3.Equals(typeof(Decimal)));

            var v4 = result[3];
            Assert.That(v4.Equals(typeof(Boolean)));

            var v5 = result[4];
            Assert.That(v5.Equals(typeof(DateTime)));

            var v6 = result[5];
            Assert.That(v6.Equals(typeof(Int64)));

        }

        [Test]
        public void SuggestDataTypes_ValidWithMissingValues_ResultWithCorrectTypes()
        {
            //Arrange
            StructureAnalyser structureAnalyser = new StructureAnalyser();

            //Act
            var result = structureAnalyser.SuggestDataTypes(rowWithMissingValues, TextSeperator.semicolon, DecimalCharacter.comma, missingValueList);

            //Assert
            Assert.NotNull(result);

            var v1 = result[0];
            Assert.That(v1.Equals(typeof(UInt32)), "is not UInt32");

            var v2 = result[1];
            Assert.That(v2.Equals(typeof(String)), "is not String");

            var v3 = result[2];
            Assert.That(v3.Equals(typeof(Decimal)), "is not Decimal");

            var v4 = result[3];
            Assert.That(v4.Equals(typeof(Boolean)), "is not Boolean");

            var v5 = result[4];
            Assert.That(v5.Equals(typeof(DateTime)), "is not DateTime");

            var v6 = result[5];
            Assert.That(v6.Equals(typeof(Int64)), "is not Int64");

        }

        private List<string> generateTestRows(int number, char seperator, List<string> missingValues=null)
        {
            List<string> rows = new List<string>();

            var r = new Random();


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

            if (missingValues!=null)
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

            return rows;
        }

    }
}