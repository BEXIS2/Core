using System;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NUnit.Framework;
using Assert = NUnit.Framework.Assert;

namespace BExIS.Xml.Helpers.UnitTests
{
    [TestFixture()]
    public class XmlUtility_XDocumentTests
    {
        private XDocument _document;

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
            _document = new XDocument();
            _document.Add(new XElement("root"));

            XElement e1 = new XElement("level1");
            XElement e2 = new XElement("level2");
            XElement e3 = new XElement("level3");

            e2.Add(e3);
            e2.Add(new XAttribute("type", "l2"));

            e1.Add(e2);

            _document.Root.Add(e1);
            _document.Root.Add(new XElement("level1a"));
            _document.Root.Add(new XElement("level1b"));
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

        [Test()]
        public void GetChildren_SourceIsNull_ReturnNull()
        {
            //Arrange
            //Act
            var result = XmlUtility.GetChildren(null);

            //Assert
            Assert.IsNull(result);
        }

        [Test()]
        public void GetChildren_WhenCalledValid_ReturnAEnumerableOfXElements()
        {
            //Arrange
            //Act
            var result = XmlUtility.GetChildren(_document.Root);

            //Assert
            Assert.IsNotNull(result);
            Assert.That(result.Count(), Is.EqualTo(3));
        }

        [Test]
        public void GetAllChildren_SourceIsNull_ReturnNull()
        {
            //Arrange
            //Act
            var result = XmlUtility.GetAllChildren(null);

            //Assert
            Assert.IsNull(result);
        }

        [Test()]
        public void GetAllChildren_WhenCalledValid_ReturnAEnumerableOfXElements()
        {
            //Arrange
            //Act
            var result = XmlUtility.GetAllChildren(_document.Root);

            //Assert
            Assert.IsNotNull(result);
            Assert.That(result.Count(), Is.EqualTo(5));
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase(" ")]
        public void GetXElementByNodeName_NodeNameIsInvalid_ReturnNull(string nodeName)
        {
            //Arrange
            //Act
            var result = XmlUtility.GetXElementByNodeName(nodeName, _document);

            //Assert
            Assert.IsNull(result);
        }

        [Test()]
        public void GetXElementByNodeName_NodeNameNotExist_ReturnEmptyList()
        {
            //Arrange
            //Act
            var result = XmlUtility.GetXElementByNodeName("level4", _document);

            //Assert
            Assert.That(result.Count(), Is.EqualTo(0));
        }

        public void GetXElementByNodeName_DocumentIsNull_ReturnNull()
        {
            //Arrange
            //Act
            var result = XmlUtility.GetXElementByNodeName("level3", null);

            //Assert
            Assert.IsNotNull(result);
            Assert.That(result.Count(), Is.EqualTo(5));
        }

        [Test()]
        public void GetXElementByNodeName_WhenCalledValid_ReturnAEnumerableOfXElements()
        {
            //Arrange
            //Act
            var result = XmlUtility.GetXElementByNodeName("level3", _document);

            //Assert
            Assert.IsNotNull(result);
            Assert.That(result.Count(), Is.EqualTo(1));
            Assert.That(result.FirstOrDefault().Name.LocalName, Is.EqualTo("level3"));
        }

        [Test()]
        public void GetXElementByAttribute_WhenCalledValid_ReturnTheExpectedXElement()
        {
            //Arrange

            //Act
            var result = XmlUtility.GetXElementByAttribute("level2", "type", "l2", _document);

            //Assert
            Assert.That(result, Is.TypeOf<XElement>());
            Assert.That(result.Name.LocalName, Is.EqualTo("level2"));
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
        public void GetXElementByAttribute_StringParametersInvalid_ReturnTheExpectedXElement(string name, string attrName, string attrValue)
        {
            //Arrange
            //Act
            var result = XmlUtility.GetXElementByAttribute(name, attrName, attrValue, _document);

            //Assert
            Assert.IsNull(result);
        }
    }
}