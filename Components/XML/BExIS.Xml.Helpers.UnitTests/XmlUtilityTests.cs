using System;
using System.Xml;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NUnit.Framework;
using Assert = NUnit.Framework.Assert;

namespace BExIS.Xml.Helpers.UnitTests
{
    [TestFixture()]
    public class XmlUtilityTests
    {
        private XmlDocument _document;

        [OneTimeSetUp]
        /// It is called once prior to executing any of the tests in a fixture.
        /// Multiple methods can be marked. Order is not preserved.
        /// Inheritance is supported, call sequence form the parents
        public void OneTimeSetUp()
        {
            // because these tests are working on in-memory objects (datasets) only, there is no need to do the test app setup
        }

        [SetUp]
        /// performs the initial setup for the tests. This runs once per test, NOT per class!
        protected void SetUp()
        {
            _document = new XmlDocument();

            var root = _document.CreateElement("root");
            var level1 = _document.CreateElement("level1");
            level1.SetAttribute("type", "l1");

            var level2 = _document.CreateElement("level2");
            level2.SetAttribute("type", "l2");

            var level3 = _document.CreateElement("level3");
            level3.SetAttribute("type", "l3");

            level2.AppendChild(level3);
            level1.AppendChild(level2);
            root.AppendChild(level1);

            _document.AppendChild(root);
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
        { }

        [Test()]
        public void GetXPathToNode_NodeIsNull_ReturnEmptyString()
        {
            var result = XmlUtility.GetXPathToNode(null);

            Assert.That(result, Is.EqualTo(String.Empty));
        }

        [Test()]
        public void GetXPathToNode_NodeIsRoot_ReturnRootNameAsXPath()
        {
            var result = XmlUtility.GetXPathToNode(_document.DocumentElement);

            Assert.That(result, Is.EqualTo("root"));
        }

        [Test()]
        public void GetXPathToNode_NodeIsChild_ReturnXPathFromNodeAsString()
        {
            var result = XmlUtility.GetXPathToNode(_document.DocumentElement.FirstChild);

            Assert.That(result, Is.EqualTo("root/level1"));
        }

        [Test()]
        public void GetDirectXPathToNode_NodeIsNull_ReturnEmptyString()
        {
            var result = XmlUtility.GetDirectXPathToNode(null);

            Assert.That(result, Is.EqualTo(String.Empty));
        }

        [Test()]
        public void GetDirectXPathToNode_NodeIsRoot_ReturnRootNameAsXPath()
        {
            var result = XmlUtility.GetDirectXPathToNode(_document.DocumentElement);

            Assert.That(result, Is.EqualTo("root"));
        }

        [Test()]
        public void GetDirectXPathToNode_NodeIsChild_ReturnXPathFromNodeAsString()
        {
            var result = XmlUtility.GetDirectXPathToNode(_document.DocumentElement.FirstChild);

            Assert.That(result, Is.EqualTo("root/level1[1]"));
        }

        [Test()]
        public void ExistAsChild_ChildNotExist_ReturnFalse()
        {
            var result = XmlUtility.ExistAsChild(_document.DocumentElement, null);

            Assert.IsFalse(result);
        }

        [Test()]
        public void ExistAsChild_ParentNotExist_ReturnFalse()
        {
            var result = XmlUtility.ExistAsChild(null, _document.DocumentElement.FirstChild);

            Assert.IsFalse(result);
        }

        [Test()]
        public void ExistAsChild_ParentNodeHasNoChildNode_ReturnFalse()
        {
            var result = XmlUtility.ExistAsChild(_document.DocumentElement.FirstChild, _document.DocumentElement.FirstChild);

            Assert.IsFalse(result);
        }

        [Test()]
        public void ExistAsChild_NodeIsNotAChild_ReturnFalse()
        {
            XmlDocument _tmp = new XmlDocument();
            _tmp.LoadXml("<example><childexample></childexample></example>");

            var result = XmlUtility.ExistAsChild(_document.DocumentElement, _tmp.DocumentElement.FirstChild);

            Assert.IsFalse(result);
        }

        [Test()]
        public void ExistAsChild_NodeIsChild_ReturnTrue()
        {
            var result = XmlUtility.ExistAsChild(_document.DocumentElement, _document.DocumentElement.FirstChild);

            Assert.IsTrue(result);
        }

        [Test()]
        public void GetXmlNodeByName_ParentIsNull_ReturnNull()
        {
            var result = XmlUtility.GetXmlNodeByName(null, "test");

            Assert.IsNull(result);
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase(" ")]
        public void GetXmlNodeByName_NameIsNullOrEmpty_ReturnNull(string name)
        {
            var result = XmlUtility.GetXmlNodeByName(_document.DocumentElement, name);

            Assert.IsNull(result);
        }

        [Test()]
        public void GetXmlNodeByName_ChildExist_ReturnXmlNode()
        {
            var result = XmlUtility.GetXmlNodeByName(_document.DocumentElement, "level1");

            Assert.IsInstanceOf<XmlNode>(result);
            Assert.That(result.Name, Is.EqualTo("level1"));
        }

        [Test()]
        public void GetXmlNodeByName_ChildExistRecursive_ReturnXmlNode()
        {
            var result = XmlUtility.GetXmlNodeByName(_document.DocumentElement, "level2");

            Assert.IsInstanceOf<XmlNode>(result);
            Assert.That(result.Name, Is.EqualTo("level2"));
        }

        [Test()]
        public void GetXmlNodeByName_ChildExistRecursiveButRecursiveIsFalse_ReturnFalse()
        {
            var result = XmlUtility.GetXmlNodeByName(_document.DocumentElement, "level2", false);

            Assert.IsNull(result);
        }

        [TestCase(null, "type", "l2")]
        [TestCase("", "type", "l2")]
        [TestCase(" ", "type", "l2")]
        [TestCase("level2", null, "l2")]
        [TestCase("level2", "", "l2")]
        [TestCase("level2", " ", "l2")]
        [TestCase("level2", "type", null)]
        [TestCase("level2", "type", "")]
        [TestCase("level2", "type", " ")]
        public void GetXmlNodeByAttribute_StringParameterIsNotValid_ReturnNull(string name, string attrName, string attrValue)
        {
            var result = XmlUtility.GetXmlNodeByAttribute(_document.DocumentElement, name, attrName, attrValue);

            Assert.IsNull(result);
        }

        [Test()]
        public void GetXmlNodeByAttribute_ParentNodeIsNull_ReturnNull()
        {
            var result = XmlUtility.GetXmlNodeByAttribute(null, "level2", "type", "l2");

            Assert.IsNull(result);
        }

        [Test()]
        public void GetXmlNodeByAttribute_NodeWithAttributeExist_ReturnXmlNode()
        {
            var result = XmlUtility.GetXmlNodeByAttribute(_document.DocumentElement, "level2", "type", "l2");

            Assert.IsInstanceOf<XmlNode>(result);
            Assert.That(result.Name, Is.EqualTo("level2"));
        }

        [Test()]
        public void GetXmlNodeByAttribute_AttributeNotExist_ReturnFalse()
        {
            var result = XmlUtility.GetXmlNodeByAttribute(_document.DocumentElement, "level2", "notExistingAttrName", "l2");

            Assert.IsNull(result);
        }

        [Test()]
        public void GetXmlNodeByAttribute_ValueOfAttributeNotExist_ReturnFalse()
        {
            var result = XmlUtility.GetXmlNodeByAttribute(_document.DocumentElement, "level2", "type", "notExistingAttrValue");

            Assert.IsNull(result);
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase(" ")]
        public void CreateNode_NodeNameIsNotValid_ReturnFalse(string name)
        {
            var result = XmlUtility.CreateNode(name, _document);

            Assert.IsNull(result);
        }

        [Test()]
        public void CreateNode_DocumentNotExist_ReturnFalse()
        {
            var result = XmlUtility.CreateNode("test", null);

            Assert.IsNull(result);
        }

        [Test()]
        public void CreateNode_WasCalled_ReturnXmlNode()
        {
            var result = XmlUtility.CreateNode("test", _document);

            Assert.IsInstanceOf<XmlNode>(result);
            Assert.That(result.Name, Is.EqualTo("test"));
        }
    }
}