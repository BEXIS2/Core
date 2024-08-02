using BExIS.App.Testing;
using BExIS.Dlm.Entities.Data;
using BExIS.Dlm.Services.Data;
using BExIS.Dlm.Services.MetadataStructure;
using BExIS.Security.Services.Objects;
using BExIS.Utils.Config;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BExIS.Dlm.Tests.Services.Data
{
    public class EntityTemplateManagerTests
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
        }

        [Test()]
        public void Create_Valid_EntityTemplate()
        {
            using (var entityTemplateManager = new EntityTemplateManager())
            using (var entityManager = new EntityManager())
            using (var metadataStructureManager = new MetadataStructureManager())
            {
                //Arrange
                EntityTemplate entityTemplate = new EntityTemplate();

                var entity = entityManager.EntityRepository.Get().FirstOrDefault();

                entityTemplate.MetadataStructure = metadataStructureManager.Repo.Get().FirstOrDefault();
                entityTemplate.Name = "Temp 1";
                entityTemplate.Description = "Description";
                entityTemplate.EntityType = entity;
                entityTemplate.MetadataInvalidSaveMode = true;
                entityTemplate.MetadataFields = new List<int> { 1, 2, 3, 4 };
                entityTemplate.NotificationGroups = new List<long> { 1, 2, 3, 4 };
                entityTemplate.DatastructureList = new List<long> { 1, 2, 3, 4 };
                entityTemplate.AllowedFileTypes = new List<string> { ".pdf", ".txt" };

                //Act
                var created = entityTemplateManager.Create(entityTemplate);
                var fromdb = entityTemplateManager.Repo.Get().LastOrDefault();

                //Assert
                Assert.IsNotNull(created);
                Assert.IsNotNull(fromdb);
                Assert.That(created.Id.Equals(fromdb.Id));
            }
        }

        [Test]
        public void Create_EntityTypeIsNull_Exception()
        {
            using (var entityTemplateManager = new EntityTemplateManager())
            using (var metadataStructureManager = new MetadataStructureManager())
            {
                //Arrange
                EntityTemplate entityTemplate = new EntityTemplate();

                entityTemplate.MetadataStructure = metadataStructureManager.Repo.Get().FirstOrDefault();
                entityTemplate.Name = "Temp 1";
                entityTemplate.Description = "Description";
                entityTemplate.EntityType = null;
                entityTemplate.MetadataInvalidSaveMode = true;
                entityTemplate.MetadataFields = new List<int> { 1, 2, 3, 4 };
                entityTemplate.NotificationGroups = new List<long> { 1, 2, 3, 4 };
                entityTemplate.DatastructureList = new List<long> { 1, 2, 3, 4 };
                entityTemplate.AllowedFileTypes = new List<string> { ".pdf", ".txt" };

                //Act
                var ex = Assert.Throws<ArgumentNullException>(() => entityTemplateManager.Create(entityTemplate));

                //Assert
                Assert.That(ex.ParamName, Is.EqualTo("Entity type must not be null."));
            }
        }

        [Test]
        public void Create_MetadataStructureIsNull_Exception()
        {
            using (var entityTemplateManager = new EntityTemplateManager())
            using (var entityManager = new EntityManager())

            {
                //Arrange
                var entity = entityManager.EntityRepository.Get().FirstOrDefault();

                EntityTemplate entityTemplate = new EntityTemplate();

                entityTemplate.MetadataStructure = null;
                entityTemplate.Name = "Temp 1";
                entityTemplate.Description = "Description";
                entityTemplate.EntityType = entity;
                entityTemplate.MetadataInvalidSaveMode = true;
                entityTemplate.MetadataFields = new List<int> { 1, 2, 3, 4 };
                entityTemplate.NotificationGroups = new List<long> { 1, 2, 3, 4 };
                entityTemplate.DatastructureList = new List<long> { 1, 2, 3, 4 };
                entityTemplate.AllowedFileTypes = new List<string> { ".pdf", ".txt" };

                //Act
                var ex = Assert.Throws<ArgumentNullException>(() => entityTemplateManager.Create(entityTemplate));

                //Assert
                Assert.That(ex.ParamName, Is.EqualTo("MetadataStructure must not be null."));
            }
        }

        [Test]
        public void Create_ObjectIsNull_ArgumentNullException()
        {
            using (var entityTemplateManager = new EntityTemplateManager())
            using (var metadataStructureManager = new MetadataStructureManager())
            {
                //Arrange
                //Act
                var ex = Assert.Throws<ArgumentNullException>(() => entityTemplateManager.Create(null));

                //Assert
                Assert.That(ex.ParamName, Is.EqualTo("Entity template must not be null."));
            }
        }

        [Test()]
        public void Update_UpdateNameofValidEntityTemplate_UpdatedEntityTemplate()
        {
            using (var entityTemplateManager = new EntityTemplateManager())
            using (var entityManager = new EntityManager())
            using (var metadataStructureManager = new MetadataStructureManager())
            {
                //Arrange
                EntityTemplate entityTemplate = new EntityTemplate();

                var entity = entityManager.EntityRepository.Get().FirstOrDefault();

                entityTemplate.MetadataStructure = metadataStructureManager.Repo.Get().FirstOrDefault();
                entityTemplate.Name = "Temp 1";
                entityTemplate.Description = "Description";
                entityTemplate.EntityType = entity;
                entityTemplate.MetadataInvalidSaveMode = true;
                entityTemplate.MetadataFields = new List<int> { 1, 2, 3, 4 };
                entityTemplate.NotificationGroups = new List<long> { 1, 2, 3, 4 };
                entityTemplate.DatastructureList = new List<long> { 1, 2, 3, 4 };
                entityTemplate.AllowedFileTypes = new List<string> { ".pdf", ".txt" };

                //Act
                var created = entityTemplateManager.Create(entityTemplate);

                var fromdb = entityTemplateManager.Repo.Get().LastOrDefault();

                fromdb.Name = "Temp updated";
                entityTemplateManager.Update(fromdb);

                var updated = entityTemplateManager.Repo.Get(created.Id);
                //Assert
                Assert.IsNotNull(updated);
                Assert.That(created.Name.Equals("Temp updated"));
            }
        }

        [Test()]
        public void Delete_Valid_DeletingSuccess()
        {
            using (var entityTemplateManager = new EntityTemplateManager())
            using (var entityManager = new EntityManager())
            using (var metadataStructureManager = new MetadataStructureManager())
            {
                //Arrange
                EntityTemplate entityTemplate = new EntityTemplate();

                var entity = entityManager.EntityRepository.Get().FirstOrDefault();

                entityTemplate.MetadataStructure = metadataStructureManager.Repo.Get().FirstOrDefault();
                entityTemplate.Name = "Temp 1";
                entityTemplate.Description = "Description";
                entityTemplate.EntityType = entity;
                entityTemplate.MetadataInvalidSaveMode = true;
                entityTemplate.MetadataFields = new List<int> { 1, 2, 3, 4 };
                entityTemplate.NotificationGroups = new List<long> { 1, 2, 3, 4 };
                entityTemplate.DatastructureList = new List<long> { 1, 2, 3, 4 };
                entityTemplate.AllowedFileTypes = new List<string> { ".pdf", ".txt" };

                //Act
                var created = entityTemplateManager.Create(entityTemplate);
                var id = created.Id;

                entityTemplateManager.Delete(created);

                var deleted = entityTemplateManager.Repo.Get(id);
                //Assert
                Assert.IsNull(deleted);
            }
        }

        [Test()]
        public void Delete_ById_Success()
        {
            using (var entityTemplateManager = new EntityTemplateManager())
            using (var entityManager = new EntityManager())
            using (var metadataStructureManager = new MetadataStructureManager())
            {
                //Arrange
                EntityTemplate entityTemplate = new EntityTemplate();

                var entity = entityManager.EntityRepository.Get().FirstOrDefault();

                entityTemplate.MetadataStructure = metadataStructureManager.Repo.Get().FirstOrDefault();
                entityTemplate.Name = "Temp 1";
                entityTemplate.Description = "Description";
                entityTemplate.EntityType = entity;
                entityTemplate.MetadataInvalidSaveMode = true;
                entityTemplate.MetadataFields = new List<int> { 1, 2, 3, 4 };
                entityTemplate.NotificationGroups = new List<long> { 1, 2, 3, 4 };
                entityTemplate.DatastructureList = new List<long> { 1, 2, 3, 4 };
                entityTemplate.AllowedFileTypes = new List<string> { ".pdf", ".txt" };

                //Act
                var created = entityTemplateManager.Create(entityTemplate);
                var id = created.Id;

                entityTemplateManager.Delete(id);

                var deleted = entityTemplateManager.Repo.Get(id);
                //Assert
                Assert.IsNull(deleted);
            }
        }
    }
}