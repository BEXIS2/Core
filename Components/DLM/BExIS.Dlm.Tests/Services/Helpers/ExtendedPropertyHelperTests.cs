using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using BExIS.App.Testing;
using BExIS.Utils.Config;
using BExIS.Dlm.Entities.DataStructure;
using BExIS.Dlm.Services.Helpers;
using BExIS.Dlm.Services.DataStructure;
using BExIS.Dlm.Services.MetadataStructure;

namespace BExIS.Dlm.Tests.Services.Helpers
{

    public class ExtendedPropertyHelperTests
    {
        private TestSetupHelper helper = null;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            helper = new TestSetupHelper(WebApiConfig.Register, false);

        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            helper.Dispose();
        }

        [Test()]
        public void Create_valid_createdProperty()
        {
            //Arrange

            using (var metadataAttributeManager = new MetadataAttributeManager())
            {
                var metadataAttr = metadataAttributeManager.MetadataAttributeRepo.Get().FirstOrDefault();

                ExtendedProperty property = new ExtendedProperty();
                property.Name = "Name";
                property.Description = "Description";

                ExtendedPropertyHelper extendedPropertyHelper = new ExtendedPropertyHelper();

                //Act
                var createdProperty = extendedPropertyHelper.Create(property, metadataAttr);

                //Assert
                Assert.That(createdProperty.Id>0);
            }

           
        }

        [Test()]
        public void Create_DataContainerIsNull_ArgumentNullException()
        {
            //Arrange


            ExtendedProperty property = new ExtendedProperty();
            property.Name = "Name";
            property.Description = "Description";

            ExtendedPropertyHelper extendedPropertyHelper = new ExtendedPropertyHelper();

            //Act
            //Assert
            Assert.Throws<ArgumentNullException>(() => extendedPropertyHelper.Create(property, null));
  
        }

        [Test()]
        public void Create_PropertyIsNull_ArgumentNullException()
        {
            //Arrange


            ExtendedProperty property = new ExtendedProperty();
            property.Name = "Name";
            property.Description = "Description";

            ExtendedPropertyHelper extendedPropertyHelper = new ExtendedPropertyHelper();

            //Act
            //Assert
            Assert.Throws<ArgumentNullException>(() => extendedPropertyHelper.Create(null, null));

        }

        [Test()]
        public void Delete_valid_PropertyDeleted()
        {
            using (var metadataAttributeManager = new MetadataAttributeManager())
            {
                var metadataAttr = metadataAttributeManager.MetadataAttributeRepo.Get().FirstOrDefault();

                ExtendedProperty property = new ExtendedProperty();
                property.Name = "Name";
                property.Description = "Description";

                ExtendedPropertyHelper extendedPropertyHelper = new ExtendedPropertyHelper();

                //Act
                var createdProperty = extendedPropertyHelper.Create(property, metadataAttr);

                Assert.That(createdProperty.Id > 0);

                var deleted = extendedPropertyHelper.Delete(createdProperty);

                Assert.True(deleted);

            }
        }
    }
}
