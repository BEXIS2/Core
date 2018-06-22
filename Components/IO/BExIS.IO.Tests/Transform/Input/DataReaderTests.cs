using NUnit.Framework;
using System.IO;
using BExIS.IO;
using Vaiona.Utils.Cfg;
using FluentAssertions;
using Moq;
using BExIS.App.Testing;
using BExIS.Dlm.Tests.Helpers;
using BExIS.Utils.Config;
using BExIS.Dlm.Entities.DataStructure;
using System.Collections.Generic;
using BExIS.IO.Transform.Input;
using BExIS.Dlm.Entities.Data;
using BExIS.IO.Transform.Validation.DSValidation;
using System.Linq;

namespace BExIS.IO.Tests.Transform.Input
{

    public class DataReaderTests
    {
        private TestSetupHelper helper = null;
        private StructuredDataStructure dataStructure;

        DatasetHelper dsHelper = null;

        [OneTimeSetUp]
        /// It is called once prior to executing any of the tests in a fixture.
        /// Multiple methods can be marked. Order is not preserved. 
        /// Inheritance is supported, call sequence form the parents
        public void OneTimeSetUp()
        {
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

        [TestCase("1|test|2.2|true|02.02.2018")]
        public void ReadRowTest(string rowString)
        {
            //preperation
            List<string> row = new List<string>(rowString.Split('|'));

            //Mock IOUtility
            var ioUtilityMock = new Mock<IOUtility>();
            ioUtilityMock.Setup(i => i.ConvertDateToCulture("2018")).Returns("2018");

            //prepare the variables
            DataReader reader = new AsciiReader(dataStructure,new AsciiFileReaderInfo(), ioUtilityMock.Object);
            IEnumerable<string> vairableNames = dataStructure.Variables.Select(v => v.Label);
            reader.SetSubmitedVariableIdentifiers(vairableNames.ToList());

            //test
            DataTuple dt = reader.ReadRow(new List<string>(row), 1);

            //asserts
            dt.Should().NotBeNull();
            dt.VariableValues.Count.Should().Equals(row.Count);

        }
    }
}
