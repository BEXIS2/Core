using BExIS.App.Testing;
using BExIS.Dim.Entities.Mappings;
using BExIS.Dim.Services.Mappings;
using BExIS.Utils.Config;
using NUnit.Framework;
using System;
using System.IO;
using System.Xml.Linq;
using Vaiona.Persistence.Api;

namespace BExIS.Dim.Helpers.Tests
{
    [TestFixture()]
    public class MappingUtilsTests
    {
        private TestSetupHelper helper = null;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            // because these tests are working on in-memory objects (datasets) only, there is no need to do the test app setup
            helper = new TestSetupHelper(WebApiConfig.Register, true);
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

        //[Test()]
        public void GetValuesFromMetadata_MultiMappingsOnDifferentKomplexTypes_ListOfValues()
        {
            //Arrange
            using (var mappingManager = new MappingManager())

            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                // install module dim because the libaries are otheside of the first setup

                IRepository<LinkElement> repo = uow.GetRepository<LinkElement>();

                // load metadata with data to map
                var basepath = AppDomain.CurrentDomain.BaseDirectory;

                string path = Path.Combine(basepath, "Data\\GetValuesFromMetadata_MultiMappingsOnDifferentKomplexTypes_ListOfValues.xml");
                var metadata = XDocument.Load(path);

                // Mappings
                // for this case MicroAgent/Name and the name of the owner/person of abcd are mapped to the Key Author
                // after the init of the database with the seed data the following data are needed.
                // ----
                // Author - LinkElementId - 8
                // MicroAgent - LinkElement - 26 - elementid - 6 - type 4
                // MicroAgent/Name - LinkElement - 28 elementid 1 - type 6
                // Abcd - LinkElement as Parent - 1
                // System - LinkElement - 2
                // Mapping -> abcd -> system - 1
                // Mapping -> system -> abcd - 2

                LinkElement microagent = mappingManager.GetLinkElement(6, LinkElementType.ComplexMetadataAttribute);
                LinkElement author = mappingManager.GetLinkElement(0, LinkElementType.Key);
                LinkElement microagentName = mappingManager.GetLinkElement(1, LinkElementType.MetadataNestedAttributeUsage);

                // create Mapping MicroAgent ->Author

                // create Mapping  Author -> MicroAgent

                // create Mapping MicroAgent/Name -> Author

                // create Mapping Author -> MicroAgent/Name
            }

            //Act
            //var values = MappingUtils.GetValuesFromMetadata();
            //Assert
        }
    }
}