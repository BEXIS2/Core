using BExIS.App.Testing;
using BExIS.Utils.Config;
using BEXIS.JSON.Helpers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using Assert = NUnit.Framework.Assert;

namespace BExIS.Xml.Helpers.UnitTests
{
    [TestFixture()]
    public class XmlMetadataConverterTests
    {
        private XDocument _document;
        private TestSetupHelper helper = null;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            // because these tests are working on in-memory objects (datasets) only, there is no need to do the test app setup
            helper = new TestSetupHelper(WebApiConfig.Register, false);
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

        [Test()]
        public void ConvertTo_XmlToJson_ReturnJson()
        {
            //Arrange
            XmlMetadataConverter xmlMetadataHelper = new XmlMetadataConverter();

            var metadata = "ConvertTo_XmlToJson2.xml";
            XmlDocument xmlDocument = new XmlDocument();
            string path = AppDomain.CurrentDomain.BaseDirectory;
            string metadataPath = Path.Combine(path, metadata);

            xmlDocument.Load(metadataPath);

            var metadataStructureConverter = new MetadataStructureConverter();
            var schema = metadataStructureConverter.ConvertToJsonSchema(1);

            //Act
            JObject result = xmlMetadataHelper.ConvertTo(xmlDocument);
            JObject result2 = xmlMetadataHelper.ConvertTo(xmlDocument, true);

            bool isvalid = result.IsValid(schema);

            var json = JsonConvert.SerializeObject(result);
            var json2 = JsonConvert.SerializeObject(result2);

            //Assert
            Assert.IsNotNull(json);
            Assert.IsTrue(isvalid);
        }

        [Test()]
        public void ConvertTo_JsonToXml_ReturnXml()
        {
            //Arrange
            XmlMetadataConverter xmlMetadataHelper = new XmlMetadataConverter();

            var metadataInput = "ConvertTo_XmlToJson.json";
            var metadataOriginalXMl = "ConvertTo_XmlToJson.xml";
            string path = AppDomain.CurrentDomain.BaseDirectory;
            string metadataInputPath = Path.Combine(path, metadataInput);
            string metadataOriginalXMlPath = Path.Combine(path, metadataOriginalXMl);

            XmlDocument metadataOriginal = new XmlDocument();
            metadataOriginal.Load(metadataOriginalXMlPath);

            using (StreamReader r = new StreamReader(metadataInputPath))
            {
                XmlMetadataConverter metadataConverter = new XmlMetadataConverter();

                string json = r.ReadToEnd();
                JObject metadataInputJson = JObject.Parse(json);

                // Act
                XmlDocument metadataOut = metadataConverter.ConvertTo(metadataInputJson);

                //Assert
                Assert.IsNotNull(metadataOut);

                // check content
                string aText = metadataOriginal.InnerText;
                string bText = metadataOut.InnerText;

                Assert.AreEqual(aText, bText, "the content between output and original xml is not equal");

                // check elements
                var a = XmlUtility.ToXDocument(metadataOut);
                var b = XmlUtility.ToXDocument(metadataOriginal);

                var aElements = XmlUtility.GetAllChildren(a.Root);
                var bElements = XmlUtility.GetAllChildren(b.Root);

                Assert.That(aElements.Count, Is.EqualTo(bElements.Count()), string.Format("number of elements a {0} is different then b {1}", aElements.Count(), bElements.Count()));

                if (aElements.Count() == bElements.Count())
                {
                    for (int i = 0; i < aElements.Count(); i++)
                    {
                        var aChild = aElements.ElementAt(i);
                        var bChild = bElements.ElementAt(i);

                        Assert.That(aChild.Name, Is.EqualTo(bChild.Name), string.Format("child element {0} is not equal to {1}", aChild.Name, bChild.Name));

                        // check attributes
                        if (aChild.Attributes().Count() > 0)
                        {
                            Assert.That(aChild.Attributes().Count(), Is.EqualTo(bChild.Attributes().Count()), string.Format("number of attributes a {0} is different then b {1}", aChild.Name, bChild.Name));

                            for (int j = 0; j < aChild.Attributes().Count(); j++)
                            {
                                var aAttr = aChild.Attributes().ElementAt(j);
                                var bAttr = bChild.Attributes().ElementAt(j);

                                Assert.That(aAttr.Name, Is.EqualTo(bAttr.Name), string.Format("child attr {0} is not equal to {1}", aChild.Name, bChild.Name));
                                Assert.That(aAttr.Value, Is.EqualTo(bAttr.Value), string.Format("value of the attr {0} - {1} is not equal to {2} - {3}", aChild.Name, aChild.Value, bChild.Name, bChild.Value));
                            }
                        }
                    }
                }
            }
        }

        [Test()]
        public void ConvertTo_XmlToJsonToXml_ReturnXml()
        {
            //Arrange
            XmlMetadataConverter xmlMetadataHelper = new XmlMetadataConverter();

            var metadataOriginalXMl = "ConvertTo_XmlToJson.xml";
            string path = AppDomain.CurrentDomain.BaseDirectory;
            string metadataOriginalXMlPath = Path.Combine(path, metadataOriginalXMl);

            XmlDocument metadataOriginal = new XmlDocument();
            metadataOriginal.Load(metadataOriginalXMlPath);

            // Act
            XmlMetadataConverter metadataConverter = new XmlMetadataConverter();

            JObject metadataAsJson = metadataConverter.ConvertTo(metadataOriginal);
            XmlDocument metadataOut = metadataConverter.ConvertTo(metadataAsJson);

            //Assert
            Assert.IsNotNull(metadataOut);

            // check content
            string aText = metadataOut.InnerText;
            string bText = metadataOriginal.InnerText;

            Assert.AreEqual(aText, bText, "the content between output and original xml is not equal");

            // check elements
            var a = XmlUtility.ToXDocument(metadataOut);
            var b = XmlUtility.ToXDocument(metadataOriginal);

            var aElements = XmlUtility.GetAllChildren(a.Root);
            var bElements = XmlUtility.GetAllChildren(b.Root);

            Assert.That(aElements.Count, Is.EqualTo(bElements.Count()), string.Format("number of elements a {0} is different then b {1}", aElements.Count(), bElements.Count()));

            if (aElements.Count() == bElements.Count())
            {
                for (int i = 0; i < aElements.Count(); i++)
                {
                    var aChild = aElements.ElementAt(i);
                    var bChild = bElements.ElementAt(i);

                    Assert.That(aChild.Name, Is.EqualTo(bChild.Name), string.Format("child element {0} is not equal to {1}", aChild.Name, bChild.Name));

                    // check attributes
                    if (aChild.Attributes().Count() > 0)
                    {
                        Assert.That(aChild.Attributes().Count(), Is.EqualTo(bChild.Attributes().Count()), string.Format("number of attributes a {0} is different then b {1}", aChild.Name, bChild.Name));

                        for (int j = 0; j < aChild.Attributes().Count(); j++)
                        {
                            var aAttr = aChild.Attributes().ElementAt(j);
                            var bAttr = bChild.Attributes().ElementAt(j);

                            Assert.That(aAttr.Name, Is.EqualTo(bAttr.Name), string.Format("child attr {0} is not equal to {1}", aChild.Name, bChild.Name));
                            Assert.That(aAttr.Value, Is.EqualTo(bAttr.Value), string.Format("value of the attr {0} - {1} is not equal to {2} - {3}", aChild.Name, aChild.Value, bChild.Name, bChild.Value));
                        }
                    }
                }
            }
        }

        [Test()]
        public void ConvertTo_XmlToJsonWithArrays_ReturnXml()
        {
            //Arrange
            XmlMetadataConverter xmlMetadataHelper = new XmlMetadataConverter();

            var metadataOriginalXMl = "ConvertTo_JsonToXml.xml";
            string path = AppDomain.CurrentDomain.BaseDirectory;
            string metadataOriginalXMlPath = Path.Combine(path, metadataOriginalXMl);

            XmlDocument metadataOriginal = new XmlDocument();
            metadataOriginal.Load(metadataOriginalXMlPath);

            // Act
            XmlMetadataConverter metadataConverter = new XmlMetadataConverter();

            JObject metadataAsJson = metadataConverter.ConvertTo(metadataOriginal);
            XmlDocument metadataOut = metadataConverter.ConvertTo(metadataAsJson);

            //Assert
            Assert.IsNotNull(metadataOut);

            // check content
            string aText = metadataOut.InnerText;
            string bText = metadataOriginal.InnerText;

            Assert.AreEqual(aText, bText, "the content between output and original xml is not equal");

            // check elements
            var a = XmlUtility.ToXDocument(metadataOut);
            var b = XmlUtility.ToXDocument(metadataOriginal);

            var aElements = XmlUtility.GetAllChildren(a.Root);
            var bElements = XmlUtility.GetAllChildren(b.Root);

            Assert.That(aElements.Count, Is.EqualTo(bElements.Count()), string.Format("number of elements a {0} is different then b {1}", aElements.Count(), bElements.Count()));

            if (aElements.Count() == bElements.Count())
            {
                for (int i = 0; i < aElements.Count(); i++)
                {
                    var aChild = aElements.ElementAt(i);
                    var bChild = bElements.ElementAt(i);

                    Assert.That(aChild.Name, Is.EqualTo(bChild.Name), string.Format("child element {0} is not equal to {1}", aChild.Name, bChild.Name));

                    // check attributes
                    if (aChild.Attributes().Count() > 0)
                    {
                        Assert.That(aChild.Attributes().Count(), Is.EqualTo(bChild.Attributes().Count()), string.Format("number of attributes a {0} is different then b {1}", aChild.Name, bChild.Name));

                        for (int j = 0; j < aChild.Attributes().Count(); j++)
                        {
                            var aAttr = aChild.Attributes().ElementAt(j);
                            var bAttr = bChild.Attributes().ElementAt(j);

                            Assert.That(aAttr.Name, Is.EqualTo(bAttr.Name), string.Format("child attr {0} is not equal to {1}", aChild.Name, bChild.Name));
                            Assert.That(aAttr.Value, Is.EqualTo(bAttr.Value), string.Format("value of the attr {0} - {1} is not equal to {2} - {3}", aChild.Name, aChild.Value, bChild.Name, bChild.Value));
                        }
                    }
                }
            }
        }

        [Test()]
        public void ConvertTo_DocumentIsNull_ThrowArgumentNullException()
        {
            XmlMetadataConverter xmlMetadataHelper = new XmlMetadataConverter();
            Assert.That(() => xmlMetadataHelper.ConvertTo(null), Throws.ArgumentNullException);
        }

        [Test()]
        public void HasValidStructure_ValidDocument_ReturnTrueAnd0Errors()
        {
            // Arrange
            var metadataInput = "ConvertTo_XmlToJson.json";
            string path = AppDomain.CurrentDomain.BaseDirectory;
            string metadataInputPath = Path.Combine(path, metadataInput);

            using (StreamReader r = new StreamReader(metadataInputPath, Encoding.UTF8, true))
            {
                XmlMetadataConverter metadataConverter = new XmlMetadataConverter();

                string json = r.ReadToEnd();
                JObject metadataInputJson = JObject.Parse(json);

                //Act
                XmlMetadataConverter xmlMetadataConverter = new XmlMetadataConverter();
                List<string> errors = new List<string>();
                var valid = xmlMetadataConverter.HasValidStructure(metadataInputJson, 1, out errors);

                //Assert
                Assert.IsTrue(valid, "The result of the function should be true, but is false");
                Assert.That(errors.Count, Is.EqualTo(0), "The number of errors should be 0");
            }
        }

        [Test()]
        public void HasValidStructure_JObjectIsNull_ThrowArgumentNullException()
        {
            // Arrange
            XmlMetadataConverter xmlMetadataHelper = new XmlMetadataConverter();
            List<String> errors = new List<string>();

            //Act & Assert
            Assert.That(() => xmlMetadataHelper.HasValidStructure(null, 1, out errors), Throws.ArgumentNullException);
        }

        [Test()]
        public void HasValidStructure_MetadataStrutcureIdIs0_ThrowArgumentNullException()
        {
            //Arrange
            XmlMetadataConverter xmlMetadataHelper = new XmlMetadataConverter();
            List<String> errors = new List<string>();
            JObject jObject = new JObject();

            //Act & Assert
            Assert.That(() => xmlMetadataHelper.HasValidStructure(jObject, 0, out errors), Throws.ArgumentNullException);
        }

        [Test()]
        public void HasValidStructure_InValid_ReturnFalsAndErrors()
        {
            //Arrange
            XmlMetadataConverter xmlMetadataHelper = new XmlMetadataConverter();
            List<String> errors = new List<string>();
            var jsonObject = new JObject();
            jsonObject.Add("Date", DateTime.Now);
            jsonObject.Add("Album", "Me Against The World");
            jsonObject.Add("Year", 1995);
            jsonObject.Add("Artist", "2Pac");

            //Act & Assert
            bool valid = xmlMetadataHelper.HasValidStructure(jsonObject, 1, out errors);

            // Assert
            Assert.IsFalse(valid, "The result of the function should be false, but is true");
            Assert.That(errors.Count, Is.EqualTo(4), "The number of errors should be 4");
        }

        //[Test()]
        //public void ConvertTo_XmlToXml_returnXml()
        //{
        //    string path = AppDomain.CurrentDomain.BaseDirectory;
        //    string dic = Path.Combine(path, "App_Data/gbif/");
        //    string xsdPath = Path.Combine(path, dic, "eml.xsd");
        //    string internalXmlPath = Path.Combine(path, dic, "internalConcept.xml");
        //    string outputPath = Path.Combine(path, dic, "metadata.xml");

        //    // arrange

        //    XmlDocument internalXml = new XmlDocument();
        //    internalXml.Load(internalXmlPath);

        //    XmlMetadataConverter converter = new XmlMetadataConverter();

        //    // act
        //    var output = converter.ConvertTo(internalXml, xsdPath);

        //    // assert

        //}
    }
}