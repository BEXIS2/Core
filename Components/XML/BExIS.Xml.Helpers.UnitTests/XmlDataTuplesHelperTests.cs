using BExIS.Dlm.Entities.Data;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using Assert = NUnit.Framework.Assert;

namespace BExIS.Xml.Helpers.UnitTests
{
    [TestFixture()]
    public class XmlDataTuplesHelperTests
    {
        private List<VariableValue> _variableValues;
        private XmlDataTupleHelper _xmlDataTupleHelper;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            // because these tests are working on in-memory objects (datasets) only, there is no need to do the test app setup
        }

        [SetUp]
        protected void SetUp()
        {
            _variableValues = new List<VariableValue>();

            VariableValue vv = new VariableValue();
            vv.VariableId = 1;
            vv.Value = 1;

            VariableValue vv2 = new VariableValue();
            vv2.VariableId = 2;
            vv2.Value = 2;

            VariableValue vv3 = new VariableValue();
            vv3.VariableId = 3;
            vv3.Value = true;

            VariableValue vv4 = new VariableValue();
            vv4.VariableId = 4;
            vv4.Value = DateTime.Now;

            _variableValues.Add(vv);
            _variableValues.Add(vv2);
            _variableValues.Add(vv3);
            _variableValues.Add(vv4);
        }

        [TearDown]
        public void TearDown()
        {
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
        }

        [Test()]
        public void Convert_ValidCall_ReturnXmlDocument()
        {
            //Arrange
            _xmlDataTupleHelper = new XmlDataTupleHelper();

            //Act
            var result = _xmlDataTupleHelper.Convert(_variableValues);

            //Assert
            Assert.IsNotNull(result);
        }

        [Test()]
        public void Convert_ValidCall_ReturnListOfVariableValues()
        {
            //Arrange
            _xmlDataTupleHelper = new XmlDataTupleHelper();
            var dataAsXml = _xmlDataTupleHelper.Convert(_variableValues);

            //Act
            var result = _xmlDataTupleHelper.Convert(dataAsXml);

            //Assert
            Assert.IsNotNull(result);
        }
    }
}