using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using BExIS.App.Testing;
using BExIS.Utils.Config;
using BEXIS.JSON.Helpers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;
using NUnit.Framework;
using Vaiona.Utils.Cfg;
using Assert = NUnit.Framework.Assert;

namespace BExIS.Xml.Helpers.UnitTests
{
    [TestFixture()]
    public class XmlMetadataConverterTests
    {
        private XDocument _document;
        private TestSetupHelper helper = null;


        [OneTimeSetUp]
        /// It is called once prior to executing any of the tests in a fixture.
        /// Multiple methods can be marked. Order is not preserved.
        /// Inheritance is supported, call sequence form the parents
        public void OneTimeSetUp()
        {
            // because these tests are working on in-memory objects (datasets) only, there is no need to do the test app setup
            helper = new TestSetupHelper(WebApiConfig.Register, false);
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

        [Test()]
        public void ConvertTo_XmlToJson_ReturnJson()
        {
            //Arrange
            XmlMetadataConverter xmlMetadataHelper = new XmlMetadataConverter();

            var metadata = "ConvertTo_XmlToJson.xml";
            XmlDocument xmlDocument = new XmlDocument();
            string path = AppDomain.CurrentDomain.BaseDirectory;
            string metadataPath = Path.Combine(path, metadata);

            xmlDocument.Load(metadataPath);

            var metadataStructureConverter = new MetadataStructureConverter();
            var schema = metadataStructureConverter.ConvertToJsonSchema(1);


            //Act
            JObject result = xmlMetadataHelper.ConvertTo(xmlDocument);
            JObject result2 = xmlMetadataHelper.ConvertTo(xmlDocument,true);

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

            using (StreamReader r = new StreamReader(metadataInputPath, Encoding.Default))
            {
                XmlMetadataConverter metadataConverter = new XmlMetadataConverter();

                string json = r.ReadToEnd();
                JObject metadataInputJson = JObject.Parse(json);

                // Act
                XmlDocument metadataOut = metadataConverter.ConvertTo(metadataInputJson);


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


                Assert.That(aElements.Count, Is.EqualTo(bElements.Count()), string.Format("number of elements a {0} is different then b {1}",aElements.Count(),bElements.Count()));

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
                            Assert.That(aChild.Attributes().Count(), Is.EqualTo(bChild.Attributes().Count()), string.Format("number of attributes a {0} is different then b {1}",aChild.Name, bChild.Name));

                            for (int j = 0; j < aChild.Attributes().Count(); j++)
                            {
                                var aAttr = aChild.Attributes().ElementAt(j);
                                var bAttr = bChild.Attributes().ElementAt(j);

                                Assert.That(aAttr.Name, Is.EqualTo(bAttr.Name), string.Format("child attr {0} is not equal to {1}", aChild.Name, bChild.Name));

                            }
                        }
                    }
                }
            }

            //Assert
            //Assert.Pass();
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

                        }
                    }
                }
            }
            

            //Assert
            //Assert.Pass();
        }

    }
}