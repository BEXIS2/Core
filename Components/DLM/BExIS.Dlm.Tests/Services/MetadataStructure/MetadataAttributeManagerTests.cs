using BExIS.App.Testing;
using BExIS.Dlm.Entities.DataStructure;
using BExIS.Dlm.Entities.MetadataStructure;
using BExIS.Dlm.Services.DataStructure;
using BExIS.Dlm.Services.MetadataStructure;
using BExIS.Utils.Config;
using NUnit.Framework;
using System;
using System.Linq;

namespace BExIS.Dlm.Tests.Services.Metadata
{
    public class MetadataAttributeManagerTests
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
        public void CreateMetadataParameter_WithDomainContraint_returnMetadataParameter()
        {
            MetadataParameter metadataParameter = new MetadataParameter();

            using (var metadataAttributeManager = new MetadataAttributeManager())
            using (var dataTypeManager = new DataTypeManager())
            {
                // Arrange

                // datatype
                var stringType = dataTypeManager.Repo.Get().FirstOrDefault();

                // domaincontraint
                DomainConstraint constraint = new DomainConstraint();
                constraint.Items.Add(new DomainItem() { Key = "test_key", Value="test_value" });

                
                metadataParameter.ShortName = "test";
                metadataParameter.Name = "test";
                metadataParameter.Description ="test";
                metadataParameter.DataType = stringType;

                // Act
                metadataParameter = metadataAttributeManager.Create(metadataParameter);

                metadataAttributeManager.AddConstraint(constraint, metadataParameter);

               


               
            }

            using (var metadataAttributeManager = new MetadataAttributeManager())
            using (var dataTypeManager = new DataTypeManager())
            {
                metadataParameter = metadataAttributeManager.MetadataParameterRepo.Get(metadataParameter.Id);

                // Assert
                Assert.IsNotNull(metadataParameter);
                Assert.That(metadataParameter.Id > 0);

                Assert.That(metadataParameter.Constraints.Count > 0);
                Assert.That(metadataParameter.Constraints.First().Id > 0);
            }

        }

        [Test()]
        public void CreateMetadataParameter_DataTypeIsNull_ArgumentNullException()
        {
            using (var metadataAttributeManager = new MetadataAttributeManager())
            {
                // Arrange
                // domaincontraint
                DomainConstraint constraint = new DomainConstraint();
                constraint.Items.Add(new DomainItem() { Key = "test_key", Value = "test_value" });

                MetadataParameter metadataParameter = new MetadataParameter();
                metadataParameter.ShortName = "test";
                metadataParameter.Name = "test";
                metadataParameter.Description = "test";
                metadataParameter.DataType = null;

                // Assert
                // Act 
                Assert.Throws<ArgumentNullException>(() => metadataAttributeManager.Create(metadataParameter));
            }
        }

        [Test()]
        public void CreateMetadataParameter_IsNull_ArgumentNullException()
        {
            using (var metadataAttributeManager = new MetadataAttributeManager())
            {
                // Arrange
                MetadataParameter p = null;

                // Assert
                // Act 
                Assert.Throws<ArgumentNullException>(() => metadataAttributeManager.Create(p));
            }
        }

        [Test()]
        public void CreateMetadataParameter_ShortNameIsNull_ArgumentNullException()
        {
            using (var metadataAttributeManager = new MetadataAttributeManager())
            {
                // Arrange

                MetadataParameter metadataParameter = new MetadataParameter();
                metadataParameter.Name = "test";
                metadataParameter.Description = "test";
                metadataParameter.DataType = null;

                // Assert
                // Act 
                Assert.Throws<ArgumentNullException>(() => metadataAttributeManager.Create(metadataParameter));
            }
        }

        [Test()]
        public void DeleteMetadataParameter_valid_removed()
        {
            using (var metadataAttributeManager = new MetadataAttributeManager())
            using (var dataTypeManager = new DataTypeManager())
            {
                // Arrange

                var stringType = dataTypeManager.Repo.Get().FirstOrDefault();


                MetadataParameter metadataParameter = new MetadataParameter();
                metadataParameter.ShortName = "delete";
                metadataParameter.Name = "test";
                metadataParameter.Description = "test";
                metadataParameter.DataType = stringType;

                metadataParameter = metadataAttributeManager.Create(metadataParameter);

                long id = metadataParameter.Id;


                // Act 

                metadataAttributeManager.Delete(metadataParameter);

                var deletedParameter = metadataAttributeManager.MetadataParameterRepo.Get(id);

                // Assert
                Assert.IsNull(deletedParameter);

            }
        }
    }
}