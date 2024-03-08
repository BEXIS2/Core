using BExIS.App.Testing;
using BExIS.Dlm.Entities.Data;
using BExIS.Dlm.Entities.DataStructure;
using BExIS.Dlm.Services.Data;
using BExIS.IO.Tests.Helpers;
using BExIS.IO.Tests.Helper;
using BExIS.IO.Transform.Input;
using BExIS.IO.Transform.Validation.DSValidation;
using BExIS.IO.Transform.Validation.Exceptions;
using BExIS.Utils.Config;
using FluentAssertions;
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
    public class DataReaderTests
    {
        private TestSetupHelper helper = null;
        private StructuredDataStructure dataStructure;

        private DatasetHelper dsHelper = null;

        private static int repeat = 0;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            // set repeat
            repeat = 10000;

            // seedata
            // 1. datastructure

            helper = new TestSetupHelper(WebApiConfig.Register, false);
            dsHelper = new DatasetHelper();
            dsHelper.PurgeAllDatasets();
            dsHelper.PurgeAllDataStructures();

            dataStructure = dsHelper.CreateADataStructure();

            // 2. list<string> as row
            /*
             *  var intType = dataTypeManager.Create("Integer", "Integer", TypeCode.Int32);
                var strType = dataTypeManager.Create("String", "String", TypeCode.String);
                var doubleType = dataTypeManager.Create("Double", "Double", TypeCode.Double);
                var boolType = dataTypeManager.Create("Bool", "Bool", TypeCode.Boolean);
                var dateTimeType = dataTypeManager.Create("DateTime", "DateTime", TypeCode.DateTime);
            */
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
            var dsHelper = new DatasetHelper();

            dsHelper.PurgeAllDatasets();
            dsHelper.PurgeAllDataStructures();
            dsHelper.PurgeAllResearchPlans();
            helper.Dispose();
        }

        #region Read Row

        [TestCase("1|test|2.2|true|02.02.2018")]
        [Repeat(4)]
        public void ReadRow_ValidRowTest_DataTupleIsValid(string rowString)
        {
            //preperation
            List<string> row = new List<string>(rowString.Split('|'));

            //Mock IOUtility -> ConvertDateToCulture
            var ioUtilityMock = new Mock<IOUtility>();
            ioUtilityMock.Setup(i => i.ConvertDateToCulture("2018")).Returns("2018");

            //Mock datasetManager -> CreateVariableValue
            var datasetManagerMock = new Mock<DatasetManager>();
            datasetManagerMock.Setup(d => d.CreateVariableValue("", "", DateTime.Now, DateTime.Now, new ObtainingMethod(), 1, new List<ParameterValue>())).Returns(
                new VariableValue()
                {
                    Value = "",
                    Note = "",
                    SamplingTime = DateTime.Now,
                    ResultTime = DateTime.Now,
                    ObtainingMethod = new ObtainingMethod(),
                    VariableId = 1,
                    ParameterValues = new List<ParameterValue>()
                }
            );

            //prepare the variables
            DataReader reader = new AsciiReader(dataStructure, new AsciiFileReaderInfo(), ioUtilityMock.Object);
            IEnumerable<string> vairableNames = dataStructure.Variables.Select(v => v.Label);
            List<VariableIdentifier> variableIdentifiers = reader.SetSubmitedVariableIdentifiers(vairableNames.ToList());

            List<Error> errors = reader.ValidateComparisonWithDatatsructure(variableIdentifiers);

            errors.Should().BeNull();

            //test
            DataTuple dt = reader.ReadRow(new List<string>(row), 1);

            //asserts
            dt.Should().NotBeNull();
            dt.VariableValues.Count.Should().Equals(row.Count);
        }

        [TestCase("1|test|2.2|true")]
        [Repeat(4)]
        public void ReadRowLessValuesTest(string rowString)
        {
            //preperation
            List<string> row = new List<string>(rowString.Split('|'));

            //Mock IOUtility
            var ioUtilityMock = new Mock<IOUtility>();
            ioUtilityMock.Setup(i => i.ConvertDateToCulture("2018")).Returns("2018");

            //prepare the variables
            DataReader reader = new AsciiReader(dataStructure, new AsciiFileReaderInfo(), ioUtilityMock.Object);
            IEnumerable<string> vairableNames = dataStructure.Variables.Select(v => v.Label);
            reader.SetSubmitedVariableIdentifiers(vairableNames.ToList());

            Assert.Throws<Exception>(() => reader.ReadRow(new List<string>(row), 1));
        }

        [TestCase("1|test|2.2|true|28.09.2018|1|2")]
        [Repeat(4)]
        public void ReadRowMoreValuesTest(string rowString)
        {
            //preperation
            List<string> row = new List<string>(rowString.Split('|'));

            //Mock IOUtility
            var ioUtilityMock = new Mock<IOUtility>();
            ioUtilityMock.Setup(i => i.ConvertDateToCulture("2018")).Returns("2018");

            //prepare the variables
            DataReader reader = new AsciiReader(dataStructure, new AsciiFileReaderInfo(), ioUtilityMock.Object);
            IEnumerable<string> vairableNames = dataStructure.Variables.Select(v => v.Label);
            reader.SetSubmitedVariableIdentifiers(vairableNames.ToList());

            //asserts
            Assert.Throws<Exception>(() => reader.ReadRow(new List<string>(row), 1));
        }

        [Test]
        [Repeat(4)]
        public void ReadRow_RowIsNullTest_DataTupleIsNull()
        {
            //Mock IOUtility
            var ioUtilityMock = new Mock<IOUtility>();
            ioUtilityMock.Setup(i => i.ConvertDateToCulture("2018")).Returns("2018");

            //prepare the variables
            DataReader reader = new AsciiReader(dataStructure, new AsciiFileReaderInfo(), ioUtilityMock.Object);
            IEnumerable<string> vairableNames = dataStructure.Variables.Select(v => v.Label);
            reader.SetSubmitedVariableIdentifiers(vairableNames.ToList());

            //test
            DataTuple dt = reader.ReadRow(null, 1);

            //asserts
            dt.Should().BeNull();
        }

        [TestCase("")]
        [Repeat(4)]
        public void ReadRow_EmptyRowTest_DataTupleIsNull(string rowString)
        {
            //preperation
            List<string> row = new List<string>(rowString.Split('|'));

            //Mock IOUtility
            var ioUtilityMock = new Mock<IOUtility>();
            ioUtilityMock.Setup(i => i.ConvertDateToCulture("2018")).Returns("2018");

            //prepare the variables
            DataReader reader = new AsciiReader(dataStructure, new AsciiFileReaderInfo(), ioUtilityMock.Object);
            IEnumerable<string> vairableNames = dataStructure.Variables.Select(v => v.Label);
            reader.SetSubmitedVariableIdentifiers(vairableNames.ToList());

            //test
            DataTuple dt = reader.ReadRow(new List<string>(row), 1);

            //asserts
            dt.Should().BeNull();
        }

        [TestCase(" 1 | test | 2.2 | true | 02.02.2018")]
        [Repeat(4)]
        public void ReadRow_textWithWhitspaceAtBeginningandEnd_WithspaceRemoved(string rowString)
        {
            //preperation
            List<string> row = new List<string>(rowString.Split('|'));

            //Mock IOUtility -> ConvertDateToCulture
            var ioUtilityMock = new Mock<IOUtility>();
            ioUtilityMock.Setup(i => i.ConvertDateToCulture("2018")).Returns("2018");

            //Mock datasetManager -> CreateVariableValue
            var datasetManagerMock = new Mock<DatasetManager>();
            datasetManagerMock.Setup(d => d.CreateVariableValue("", "", DateTime.Now, DateTime.Now, new ObtainingMethod(), 1, new List<ParameterValue>())).Returns(
                new VariableValue()
                {
                    Value = "",
                    Note = "",
                    SamplingTime = DateTime.Now,
                    ResultTime = DateTime.Now,
                    ObtainingMethod = new ObtainingMethod(),
                    VariableId = 1,
                    ParameterValues = new List<ParameterValue>()
                }
            );

            //prepare the variables
            DataReader reader = new AsciiReader(dataStructure, new AsciiFileReaderInfo(), ioUtilityMock.Object);
            IEnumerable<string> vairableNames = dataStructure.Variables.Select(v => v.Label);
            List<VariableIdentifier> variableIdentifiers = reader.SetSubmitedVariableIdentifiers(vairableNames.ToList());

            List<Error> errors = reader.ValidateComparisonWithDatatsructure(variableIdentifiers);

            errors.Should().BeNull();

            //test
            DataTuple dt = reader.ReadRow(new List<string>(row), 1);

            var v1 = dt.VariableValues[0].Value.ToString();
            var v2 = dt.VariableValues[1].Value.ToString();
            var v3 = dt.VariableValues[2].Value.ToString();
            var v4 = dt.VariableValues[3].Value.ToString();

            Assert.That(v1, Is.EqualTo("1"));
            Assert.That(v2, Is.EqualTo("test"));
            Assert.That(v3, Is.EqualTo("2.2"));
            Assert.That(v4, Is.EqualTo("true"));

            //Assert.Throws<Exception>(() => reader.ReadRow(new List<string>(row), 1));
        }

        #endregion Read Row

        #region Validate Row

        //ToDo check for Mocks in the ValidateRow Function
        [TestCase("1|test|2.2|true|02.02.2018")]
        [Repeat(4)]
        public void ValidateRow_ValidRowTest_NoErrors(string rowString)
        {
            //preperation
            List<string> row = new List<string>(rowString.Split('|'));

            //Mock IOUtility
            var ioUtilityMock = new Mock<IOUtility>();
            ioUtilityMock.Setup(i => i.ConvertDateToCulture("2018")).Returns("2018");

            //prepare the variables
            DataReader reader = new AsciiReader(dataStructure, new AsciiFileReaderInfo(), ioUtilityMock.Object);
            IEnumerable<string> vairableNames = dataStructure.Variables.Select(v => v.Label);
            reader.SetSubmitedVariableIdentifiers(vairableNames.ToList());

            //test
            List<Error> errors = reader.ValidateRow(new List<string>(row), 1);

            //asserts
            errors.Should().NotBeNull();
            errors.Count.Should().Equals(0);
        }

        [TestCase("1|test|2.2|true")]
        [Repeat(4)]
        public void ValidateRowLessValuesTest(string rowString)
        {
            //preperation
            List<string> row = new List<string>(rowString.Split('|'));

            //Mock IOUtility
            var ioUtilityMock = new Mock<IOUtility>();
            ioUtilityMock.Setup(i => i.ConvertDateToCulture("2018")).Returns("2018");

            //prepare the variables
            DataReader reader = new AsciiReader(dataStructure, new AsciiFileReaderInfo(), ioUtilityMock.Object);
            IEnumerable<string> vairableNames = dataStructure.Variables.Select(v => v.Label);
            reader.SetSubmitedVariableIdentifiers(vairableNames.ToList());

            //test
            List<Error> errors = reader.ValidateRow(new List<string>(row), 1);

            //asserts
            errors.Should().NotBeNull();
            errors.Count.Should().Equals(1);
            errors.ElementAt(0).ToString().Should().ContainEquivalentOf("Number of Values different as number of variables");
        }

        [TestCase("1|test|2.2|true|28.09.2018|1|2")]
        [Repeat(4)]
        public void ValidateRowMoreValuesTest(string rowString)
        {
            //preperation
            List<string> row = new List<string>(rowString.Split('|'));

            //Mock IOUtility
            var ioUtilityMock = new Mock<IOUtility>();
            ioUtilityMock.Setup(i => i.ConvertDateToCulture("2018")).Returns("2018");

            //prepare the variables
            DataReader reader = new AsciiReader(dataStructure, new AsciiFileReaderInfo(), ioUtilityMock.Object);
            IEnumerable<string> vairableNames = dataStructure.Variables.Select(v => v.Label);
            reader.SetSubmitedVariableIdentifiers(vairableNames.ToList());

            //test
            List<Error> errors = reader.ValidateRow(new List<string>(row), 1);

            //asserts
            errors.Should().NotBeNull();
            errors.Count.Should().Equals(1);
            errors.ElementAt(0).ToString().Should().ContainEquivalentOf("Number of Values different as number of variables");
        }

        //ToDo check for Mocks in the ValidateRow Function
        [TestCase("var1UT|var2UT|var3UT|var4UT|var5UT")]
        [Repeat(4)]
        public void ValidateComparisonWithDatatstructureTest(string variableRowString)
        {
            //preperation
            List<string> variableRow = new List<string>(variableRowString.Split('|'));

            //prepare the variables
            DataReader reader = new AsciiReader(dataStructure, new AsciiFileReaderInfo());
            IEnumerable<string> vairableNames = dataStructure.Variables.Select(v => v.Label);
            List<VariableIdentifier> variableIdentifiers = reader.SetSubmitedVariableIdentifiers(vairableNames.ToList());

            //test
            List<Error> errors = reader.ValidateComparisonWithDatatsructure(variableIdentifiers);

            //asserts
            errors.Should().BeNull();
        }

        [TestCase("var1UT|var2UT|var3UT|var4UT")]
        [Repeat(4)]
        public void ValidateComparisonWithDatatstructureLessVariablesTest(string variableRowString)
        {
            //preperation
            List<string> variableRow = new List<string>(variableRowString.Split('|'));

            //prepare the variables
            DataReader reader = new AsciiReader(dataStructure, new AsciiFileReaderInfo());
            List<VariableIdentifier> variableIdentifiers = reader.SetSubmitedVariableIdentifiers(variableRow);

            //test
            List<Error> errors = reader.ValidateComparisonWithDatatsructure(variableIdentifiers);

            //asserts
            errors.Should().NotBeNull();
            errors.Count.Should().Equals(1);
        }

        [TestCase("var1UT|var2UT|var3UT|var4UT|var5UT|var6UT")]
        [Repeat(4)]
        public void ValidateComparisonWithDatatstructureMoreVariablesTest(string variableRowString)
        {
            //preperation
            List<string> variableRow = new List<string>(variableRowString.Split('|'));

            //prepare the variables
            DataReader reader = new AsciiReader(dataStructure, new AsciiFileReaderInfo());
            List<VariableIdentifier> variableIdentifiers = reader.SetSubmitedVariableIdentifiers(variableRow.ToList());

            //test
            List<Error> errors = reader.ValidateComparisonWithDatatsructure(variableIdentifiers);

            //asserts
            errors.Should().NotBeNull();
            errors.Count.Should().Equals(1);
        }

        [TestCase("var1UT|var2UT|var3UT|var4UT|XYZTSRZRDZ|var6UT")]
        [Repeat(4)]
        public void ValidateComparisonWithDatatstructureVariablNotExistTest(string variableRowString)
        {
            //preperation
            List<string> variableRow = new List<string>(variableRowString.Split('|'));

            //prepare the variables
            DataReader reader = new AsciiReader(dataStructure, new AsciiFileReaderInfo());
            List<VariableIdentifier> variableIdentifiers = reader.SetSubmitedVariableIdentifiers(variableRow.ToList());

            //test
            List<Error> errors = reader.ValidateComparisonWithDatatsructure(variableIdentifiers);

            //asserts
            errors.Should().NotBeNull();
            errors.Count.Should().Equals(1);
        }

        [Test]
        [Repeat(4)]
        public void ValidateComparisonWithDatatstructureNullTest()
        {
            //prepare the variables
            DataReader reader = new AsciiReader(dataStructure, new AsciiFileReaderInfo());

            //test
            List<Error> errors = reader.ValidateComparisonWithDatatsructure(null);

            //asserts
            errors.Should().NotBeNull();
            errors.Count.Should().Equals(1);
        }

        [Test]
        public void ValidateRow_runValid_noErrors()
        {
            //Arrange

            DataGeneratorHelper dgh = new DataGeneratorHelper();
            var errors = new List<Error>();
            var testData = dgh.GenerateRowsWithRandomValuesBasedOnDatastructure(dataStructure, ",", 1000, true);

            //generate file to read
            Encoding encoding = Encoding.UTF8;
            string path = Path.Combine(AppConfiguration.DataPath, "testdataforvalidation.txt");
            if (File.Exists(path))
            {
                File.Delete(path);
            }

            using (StreamWriter sw = new StreamWriter(path))
            {
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

                DataReader reader = new AsciiReader(dataStructure, new AsciiFileReaderInfo(), ioUtilityMock.Object);
                IEnumerable<string> vairableNames = dataStructure.Variables.Select(v => v.Label);
                List<VariableIdentifier> variableIdentifiers = reader.SetSubmitedVariableIdentifiers(vairableNames.ToList());
                reader.ValidateComparisonWithDatatsructure(variableIdentifiers);

                var asciireader = (AsciiReader)reader;
                //Act
                var row = new List<string>();

                using (StreamReader streamReader = new StreamReader(path, encoding))
                {
                    string line;
                    int index = 1;
                    char seperator = AsciiFileReaderInfo.GetSeperator(afr.Seperator);

                    while ((line = streamReader.ReadLine()) != null)
                    {
                        row = asciireader.rowToList(line, ',');
                        errors = asciireader.ValidateRow(row, index);

                        index++;
                    }
                }

                //Assert
                Assert.That(errors.Count, Is.EqualTo(0));
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #endregion Validate Row
    }
}