using Autofac;
using BExIS.App.Testing;
using BExIS.Security.Services.Objects;
using BExIS.Security.Services.Utilities;
using BExIS.Utils.Config;
using BEXIS2.Dlm.Entities.Data;
using BEXIS2.Dlm.Services.Data;
using BEXIS2.Dlm.Services.MetadataStructure;
using BEXIS2.Utils.Cfg;
using Castle.Core.Logging;
using NHibernate;
using NHibernate.Cfg;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.ServiceModel.Syndication;


namespace BExIS.Dlm.Tests.Services.Data
{
    public class EntityTemplateManager2Tests
    {
        private TestSetupHelper helper = null;

        private IContainer _container;
        private ILifetimeScope _scope;

        private EntityTemplate2Manager _entityTemplateManager = null;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            helper = new TestSetupHelper(WebApiConfig.Register, false);

            var builder = new ContainerBuilder();

            // 1. NHibernate SessionFactory einmalig initialisieren (Beispiel)
            var connectionString = AppConfiguration.DefaultApplicationConnection.ConnectionString;
            var cfg = new Configuration();
            cfg.SetProperty(NHibernate.Cfg.Environment.ConnectionString, connectionString);

            string workspacepath = Path.Combine(AppConfiguration.WorkspaceRootPath, "Components");
    
            DirectoryInfo mappingDirectory = new DirectoryInfo(workspacepath);
            cfg.AddDirectory(mappingDirectory);

            ISessionFactory _sessionFactory = cfg.Configure().BuildSessionFactory();
            // 2. ISessionFactory als Singleton registrieren
            builder.RegisterInstance(_sessionFactory).As<ISessionFactory>().SingleInstance();
            // 3. ISession als "Per Lifetime Scope" binden (für Tests: Verwerfen am Scope-Ende inklusive)
            builder.Register(context => context.Resolve<ISessionFactory>().OpenSession())
                   .As<ISession>()
                   .InstancePerLifetimeScope();     

            // Register your actual implementations
            builder.RegisterType<ConsoleLogger>().As<ILogger>();
            builder.RegisterType<EntityTemplate2Manager>().AsSelf();

            // 4. Alle Manager/Services im Assembly automatisch registrieren
            // Sucht nach Klassen, deren Name auf "Manager" oder "Service" endet
            builder.RegisterAssemblyTypes(typeof(EntityTemplateManager2Tests).Assembly) // bzw. Assembly.GetExecutingAssembly()
                   .Where(t => t.Name.EndsWith("Manager") || t.Name.EndsWith("Service"))
                   .AsSelf()
                   .InstancePerRequest();



            _container = builder.Build();

            // Create a lifetime scope for the test
            _scope = _container.BeginLifetimeScope();
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
        }


        [Test()]
        public void Create_Valid_EntityTemplate()
        {
            using (var entityManager = new EntityManager())
            using (var metadataStructureManager = new MetadataStructureManager())
            {
                _entityTemplateManager = _scope.Resolve<EntityTemplate2Manager>();

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
                var created = _entityTemplateManager.Create(entityTemplate);
                var fromdb = _entityTemplateManager.GetAllAsync().LastOrDefault();

                //Assert
                Assert.IsNotNull(created);
                Assert.IsNotNull(fromdb);
                Assert.That(created.Id.Equals(fromdb.Id));
            }
        }

        [Test]
        public void Create_EntityTypeIsNull_Exception()
        {
            using (var metadataStructureManager = new MetadataStructureManager())
            {
                //Arrange
                _entityTemplateManager = _scope.Resolve<EntityTemplate2Manager>();
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
                var ex = Assert.Throws<ArgumentNullException>(() => _entityTemplateManager.Create(entityTemplate));

                //Assert
                Assert.That(ex.ParamName, Is.EqualTo("Entity type must not be null."));
            }
        }

        [Test]
        public void Create_MetadataStructureIsNull_Exception()
        {
            using (var entityManager = new EntityManager())

            {
                //Arrange
                _entityTemplateManager = _scope.Resolve<EntityTemplate2Manager>();
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
                var ex = Assert.Throws<ArgumentNullException>(() => _entityTemplateManager.Create(entityTemplate));

                //Assert
                Assert.That(ex.ParamName, Is.EqualTo("MetadataStructure must not be null."));
            }
        }

        [Test]
        public void Create_ObjectIsNull_ArgumentNullException()
        {
            using (var metadataStructureManager = new MetadataStructureManager())
            {
                //Arrange
                _entityTemplateManager = _scope.Resolve<EntityTemplate2Manager>();

                //Act
                var ex = Assert.Throws<ArgumentNullException>(() => _entityTemplateManager.Create(null));

                //Assert
                Assert.That(ex.ParamName, Is.EqualTo("Entity template must not be null."));
            }
        }

        [Test()]
        public void Update_UpdateNameofValidEntityTemplate_UpdatedEntityTemplate()
        {
            using (var entityManager = new EntityManager())
            using (var metadataStructureManager = new MetadataStructureManager())
            {
                //Arrange
                _entityTemplateManager = _scope.Resolve<EntityTemplate2Manager>();

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
                var created = _entityTemplateManager.Create(entityTemplate);

                var fromdb = _entityTemplateManager.GetAllAsync().LastOrDefault();

                fromdb.Name = "Temp updated";
                _entityTemplateManager.Update(fromdb);

                var updated = _entityTemplateManager.Get(created.Id);
                //Assert
                Assert.IsNotNull(updated);
                Assert.That(created.Name.Equals("Temp updated"));
            }
        }

        [Test()]
        public void Delete_Valid_DeletingSuccess()
        {
            using (var entityManager = new EntityManager())
            using (var metadataStructureManager = new MetadataStructureManager())
            {
                //Arrange
                _entityTemplateManager = _scope.Resolve<EntityTemplate2Manager>();

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
                var created = _entityTemplateManager.Create(entityTemplate);
                var id = created.Id;

                _entityTemplateManager.Delete(created);

                var deleted = _entityTemplateManager.Get(id);
                //Assert
                Assert.IsNull(deleted);
            }
        }

        [Test()]
        public void Delete_ById_Success()
        {
 
            using (var entityManager = new EntityManager())
            using (var metadataStructureManager = new MetadataStructureManager())
            {
                //Arrange
                _entityTemplateManager = _scope.Resolve<EntityTemplate2Manager>();
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
                var created = _entityTemplateManager.Create(entityTemplate);
                var id = created.Id;

                _entityTemplateManager.Delete(id);

                var deleted = _entityTemplateManager.Get(id);
                //Assert
                Assert.IsNull(deleted);
            }
        }
    }
}