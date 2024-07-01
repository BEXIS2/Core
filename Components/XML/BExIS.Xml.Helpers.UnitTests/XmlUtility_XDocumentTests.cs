using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Assert = NUnit.Framework.Assert;

namespace BExIS.Xml.Helpers.UnitTests
{
    [TestFixture()]
    public class XmlUtility_XDocumentTests
    {
        private XDocument _document;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            // because these tests are working on in-memory objects (datasets) only, there is no need to do the test app setup
        }

        [SetUp]
        protected void SetUp()
        {
            _document = new XDocument();
            _document.Add(new XElement("root"));

            XElement e1 = new XElement("level1");
            XElement e2 = new XElement("level2");
            XElement e3 = new XElement("level3");

            // add format attribute
            e1.Add(new XAttribute("format", "l"));
            e2.Add(new XAttribute("format", "l"));
            e3.Add(new XAttribute("format", "l"));

            e1.Add(new XAttribute("type", "l1"));
            e2.Add(new XAttribute("type", "l2"));
            e3.Add(new XAttribute("type", "l3"));

            e2.Add(e3);
            e1.Add(e2);

            // create rooms
            XElement r1 = new XElement("room");
            XElement r2 = new XElement("room");
            XElement r3 = new XElement("room");

            // add nr attribute
            r1.Add(new XAttribute("nr", "l"));
            r2.Add(new XAttribute("nr", "2"));
            r3.Add(new XAttribute("nr", "3"));

            // window account attr
            r1.Add(new XAttribute("windows", "2"));
            r2.Add(new XAttribute("windows", "2"));
            r3.Add(new XAttribute("windows", "2"));

            r1.Add(new XAttribute("doors", "1"));
            r2.Add(new XAttribute("doors", "1"));
            r3.Add(new XAttribute("doors", "1"));

            r3.Add(new XAttribute("lamp", "1"));

            // add r to e3
            e3.Add(r1);
            e3.Add(r2);
            e3.Add(r3);

            _document.Root.Add(e1);
            _document.Root.Add(new XElement("level1a"));
            _document.Root.Add(new XElement("level1b"));
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
            Assert.That(result.Count(), Is.EqualTo(8));
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

        [Test()]
        public void GetXElementByNodeName_DocumentIsNull_ReturnNull()
        {
            //Arrange
            //Act
            var result = XmlUtility.GetXElementByNodeName("level3", null);

            //Assert
            Assert.IsNull(result);
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
        public void GetXElementByAttribute_StringParametersInvalid_ReturnNull(string name, string attrName, string attrValue)
        {
            //Arrange
            //Act
            var result = XmlUtility.GetXElementByAttribute(name, attrName, attrValue, _document);

            //Assert
            Assert.IsNull(result);
        }

        [Test()]
        public void GetXElementsByAttribute_WhenCalledValid_ReturnExpectedXElements()
        {
            //Arrange

            //Act
            var result = XmlUtility.GetXElementsByAttribute("room", "windows", "2", _document);

            //Assert
            Assert.That(result.Count, Is.EqualTo(3));
        }

        [Test()]
        public void GetXElementsByAttribute_AttributeNotExistButNodeAndValue_ReturnEmptyList()
        {
            //Arrange

            //Act
            var result = XmlUtility.GetXElementsByAttribute("room", "notexist", "2", _document);

            //Assert
            Assert.That(result.Count, Is.EqualTo(0));
        }

        [Test()]
        public void GetXElementsByAttribute_AttributeValueNotExistButNodeAndAttribute_ReturnEmptyList()
        {
            //Arrange

            //Act
            var result = XmlUtility.GetXElementsByAttribute("room", "window", "3", _document);

            //Assert
            Assert.That(result.Count, Is.EqualTo(0));
        }

        [TestCase(null, "windows", "2")]
        [TestCase("", "windows", "2")]
        [TestCase(" ", "windows", "22")]
        [TestCase("room", null, "2")]
        [TestCase("room", "", "2")]
        [TestCase("room", " ", "2")]
        [TestCase("room", "windows", null)]
        [TestCase("room", "windows", "")]
        [TestCase("room", "windows", " ")]
        public void GetXElementsByAttribute_StringParametersInvalid_ReturnNull(string name, string attrName, string attrValue)
        {
            //Arrange
            //Act
            var result = XmlUtility.GetXElementsByAttribute(name, attrName, attrValue, _document);

            //Assert
            Assert.IsNull(result);
        }

        [Test()]
        public void GetXElementsByAttribute_WhenCallValid_ReturnExpectedXElements()
        {
            //Arrange
            //Act
            var result = XmlUtility.GetXElementsByAttribute("format", _document);

            //Assert
            Assert.IsNotNull(result);
            Assert.That(result.Count(), Is.EqualTo(3));
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase(" ")]
        public void GetXElementsByAttribute_StringParametersInvalid_ReturnNull(string attrName)
        {
            //Arrange
            //Act
            var result = XmlUtility.GetXElementsByAttribute(attrName, _document);

            //Assert
            Assert.IsNull(result);
        }

        [Test()]
        public void GetXElementsByAttribute_AttributeNotExist_ReturnEmptyList()
        {
            //Arrange
            //Act
            var result = XmlUtility.GetXElementsByAttribute("NotExistingAttributeName", _document);

            //Assert
            Assert.IsNotNull(result);
            Assert.That(result.Count(), Is.EqualTo(0));
        }

        [TestCase(null, "1")]
        [TestCase("", "1")]
        [TestCase(" ", "1")]
        [TestCase("format", null)]
        [TestCase("format", "")]
        [TestCase("format", " ")]
        public void GetXElementsByAttribute_StringParametersInvalid_ReturnNull(string attrName, string attrValue)
        {
            //Arrange
            //Act
            var result = XmlUtility.GetXElementsByAttribute(attrName, attrValue, _document);

            //Assert
            Assert.IsNull(result);
        }

        [Test()]
        public void GetXElementsByAttribute_AttributeNotExistButValue_ReturnEmptyList()
        {
            //Arrange
            //Act
            var result = XmlUtility.GetXElementsByAttribute("NotExistingAttributeName", "2", _document);

            //Assert
            Assert.IsNotNull(result);
            Assert.That(result.Count(), Is.EqualTo(0));
        }

        [Test()]
        public void GetXElementsByAttribute_AttributeExistButNotValue_ReturnEmptyList()
        {
            //Arrange
            //Act
            var result = XmlUtility.GetXElementsByAttribute("format", "3", _document);

            //Assert
            Assert.IsNotNull(result);
            Assert.That(result.Count(), Is.EqualTo(0));
        }

        [Test()]
        public void GetXElementsByAttribute_WhenCallValidWithTwoParameters_ReturnExpectedXElements()
        {
            //Arrange
            //Act
            var result = XmlUtility.GetXElementsByAttribute("format", "l", _document);

            //Assert
            Assert.IsNotNull(result);
            Assert.That(result.Count(), Is.EqualTo(3));
        }

        [Test()]
        public void GetXElementByAttribute_WhenCallValidWithManyAttributes_ReturnExpectedXElement()
        {
            //Arrange
            var dic = new Dictionary<string, string>();
            dic.Add("nr", "2");
            dic.Add("windows", "2");
            string nodeName = "room";
            //Act
            var result = XmlUtility.GetXElementByAttribute(nodeName, dic, _document.Root);

            //Assert
            Assert.IsNotNull(result);
            Assert.That(result.Name.LocalName, Is.EqualTo(nodeName));
        }

        [Test()]
        public void GetXElementsByAttribute_WhenCallValidWithManyAttributes_ReturnExpectedXElement()
        {
            //Arrange
            var dic = new Dictionary<string, string>();
            dic.Add("doors", "1");
            dic.Add("windows", "2");
            string nodeName = "room";
            //Act
            var result = XmlUtility.GetXElementsByAttribute(nodeName, dic, _document.Root);

            //Assert
            Assert.IsNotNull(result);
            Assert.That(result.Count, Is.EqualTo(3));
        }

        [Test()]
        public void GetXElementsByAttribute_ExistingNodeTypeButDicWithNotValidAttributes_ReturnEmptyList()
        {
            //Arrange
            var dic = new Dictionary<string, string>();
            dic.Add("notexistingattr", "1"); // not existing attr
            dic.Add("windows", "2"); //existing attr
            string nodeName = "room";
            //Act
            var result = XmlUtility.GetXElementsByAttribute(nodeName, dic, _document.Root);

            //Assert
            Assert.IsNotNull(result);
            Assert.That(result.Count, Is.EqualTo(0));
        }
    }
}