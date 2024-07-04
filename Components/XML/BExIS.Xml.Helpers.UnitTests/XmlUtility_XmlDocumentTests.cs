using NUnit.Framework;
using System;
using System.Xml;
using Assert = NUnit.Framework.Assert;

namespace BExIS.Xml.Helpers.UnitTests
{
    [TestFixture()]
    public class XmlUtility_XmlDocumentTests
    {
        private XmlDocument _document;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            // because these tests are working on in-memory objects (datasets) only, there is no need to do the test app setup
        }

        [SetUp]
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
        public void TearDown()
        {
        }

        [OneTimeTearDown]
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

        [Test()]
        public void GenerateNodeFromXpath_XpathIsSame_Xpath()
        {
            //Arrange
            string xpath = "root/a/b/c";
            //Act
            var result = XmlUtility.GenerateNodeFromXPath(_document, null, xpath);
            string resultXPath = XmlUtility.GetXPathToNode(result);
            //Assert
            Assert.That(resultXPath, Is.EqualTo(xpath));
        }

        [Test()]
        public void GenerateNodeFromXpath_XpathIsEmpty_ReturnParent()
        {
            //Arrange
            //Act
            var result = XmlUtility.GenerateNodeFromXPath(_document, _document.DocumentElement, string.Empty);

            //Assert
            Assert.That(result, Is.EqualTo(_document.DocumentElement));
        }

        [Test()]
        public void GenerateNodeFromXpath_ParentIsInvalid_ReturnNull()
        {
            //Arrange
            string xpath = "/test/function";
            //Act
            var result = XmlUtility.GenerateNodeFromXPath(_document, null, xpath);

            //Assert
            Assert.IsNull(result);
        }

        [Test()]
        public void GenerateNodeFromXpath_DocIsInvalid_ReturnNull()
        {
            //Arrange
            string xpath = "/test/function";
            //Act
            var result = XmlUtility.GenerateNodeFromXPath(null, _document.DocumentElement, xpath);

            //Assert
            Assert.IsNull(result);
        }

        [Test()]
        public void GenerateNodeFromXpath_ParentNotBelongToDoc_ReturnNull()
        {
            //Arrange
            XmlDocument tmp = new XmlDocument();
            tmp.AppendChild(tmp.CreateElement("otherRoot"));

            string xpath = "root/level1/levelA";

            //Act
            //Assert
            Assert.That(() => XmlUtility.GenerateNodeFromXPath(_document, tmp.DocumentElement, xpath), Throws.Exception);
        }

        [Test()]
        public void GenerateNodeFromXpath_WhenCalledValid_ReturnParentWithNewChildrens()
        {
            //Arrange
            string xpath = "levelA/levelB";
            //Act
            var result = XmlUtility.GenerateNodeFromXPath(_document, _document.DocumentElement, xpath);

            //Assert
            Assert.That(result.Name, Is.EqualTo("levelB"));
            Assert.That(result.ParentNode.Name, Is.EqualTo("levelA"));
            Assert.That(result.ParentNode.ParentNode.Name, Is.EqualTo("root"));
            Assert.That(_document.DocumentElement.ChildNodes.Count, Is.EqualTo(2));
        }

        [TestCase(null, "AttrValue")]
        [TestCase("", "AttrValue")]
        [TestCase(" ", "AttrValue")]
        public void AddAttribute_AttributeParametersInvalid_ReturnNull(string name, string value)
        {
            //Arrange
            //Act
            var result = XmlUtility.AddAttribute(_document.DocumentElement, name, value, _document);

            //Assert
            Assert.IsNull(result);
        }

        [Test()]
        public void AddAttribute_NodeIsInvalid_ReturnNull()
        {
            //Arrange
            string name = "AttrName";
            string value = "AttrValue";
            //Act
            var result = XmlUtility.AddAttribute(_document.DocumentElement, name, value, null);

            //Assert
            Assert.IsNull(result);
        }

        [Test()]
        public void AddAttribute_DocIsInvalid_ReturnNull()
        {
            //Arrange
            string name = "AttrName";
            string value = "AttrValue";

            //Act
            var result = XmlUtility.AddAttribute(_document.DocumentElement, name, value, null);

            //Assert
            Assert.IsNull(result);
        }

        [Test()]
        public void Addttribute_NodeNotBelongToDoc_ReturnNull()
        {
            //Arrange
            XmlDocument tmp = new XmlDocument();
            tmp.AppendChild(tmp.CreateElement("otherRoot"));

            string name = "AttrName";
            string value = "AttrValue";

            //Act
            //Assert
            Assert.That(() => XmlUtility.AddAttribute(_document.DocumentElement, name, value, tmp), Throws.Exception);
        }

        [Test()]
        public void AddAttribute_WhenCalledValid_ReturnParentWithNewChildrens()
        {
            //Arrange
            string name = "AttrName";
            string value = "AttrValue";

            //Act
            var result = XmlUtility.AddAttribute(_document.DocumentElement, name, value, _document);

            //Assert
            Assert.That(result.Attributes.Count, Is.EqualTo(1));
            Assert.That(result.Attributes[0].Name, Is.EqualTo(name));
            Assert.That(result.Attributes[0].Value, Is.EqualTo(value));
        }

        [Test()]
        public void FindXPath_NodeAsAttribute_ReturnXPath()
        {
            //Arrange
            string xpath = "/root[1]/level1[1]/level2[1]/level3[1]/@type";
            XmlNode node = _document.SelectSingleNode(xpath);

            //Act
            var result = XmlUtility.FindXPath(node);

            //Assert
            Assert.AreEqual(xpath, result);
        }

        [Test()]
        public void FindXPath_NodeAsElement_ReturnXPath()
        {
            //Arrange
            string xpath = "/root[1]/level1[1]/level2[1]/level3[1]";
            XmlNode node = _document.SelectSingleNode(xpath);

            //Act
            var result = XmlUtility.FindXPath(node);

            //Assert
            Assert.AreEqual(xpath, result);
        }

        [Test()]
        public void FindXPath_NodeAsDocument_ReturnXPath()
        {
            //Act
            var result = XmlUtility.FindXPath(_document);

            //Assert
            Assert.That(result, Is.EqualTo(""));
        }

        [Test()]
        public void FindXPath_NodeAsInvalidType_ReturnArgumentException()
        {
            //Arrange
            XmlNode node = _document.CreateEntityReference("xyz");

            //Act
            //Assert
            Assert.That(() => XmlUtility.FindXPath(node), Throws.ArgumentException);
        }

        [Test()]
        public void FindXPath_NodeAsNotValid_ReturnArgumentException()
        {
            //Arrange
            XmlNode node = _document.CreateEntityReference("xyz");

            //Act
            //Assert
            Assert.That(() => XmlUtility.FindXPath(null), Throws.ArgumentException);
        }
    }
}