using NUnit.Framework;
using BExIS.Dlm.Entities.Data;
using BExIS.Dlm.Services.Data;
using Vaiona.Persistence.Api;
using BExIS.App.Testing;
using BExIS.Utils.Config;
using System.Linq;
using BExIS.Dlm.Tests.Helpers;
using BExIS.Dlm.Entities.DataStructure;
using FluentAssertions;
using System;

namespace BExIS.Dlm.Tests.Services.Data
{
    [TestFixture]
    public class TagManagerTests
    {
        private TestSetupHelper helper = null;
        private DatasetHelper dsHelper;
        Dataset dataset = null;

        [SetUp]
        public void Setup()
        {
            helper = new TestSetupHelper(WebApiConfig.Register, false);
            dsHelper = new DatasetHelper();
            dataset = dsHelper.CreateDataset();
        }

        [TearDown]
        public void TearDown()
        {
            //using (IUnitOfWork uow = this.GetUnitOfWork())
            //{
            //    IRepository<Tag> repo = uow.GetRepository<Tag>();

            //    repo.Get().ToList().ForEach(v => repo.Delete(v));
            //    uow.Commit();
            //}

            dsHelper.PurgeAllDatasets();
            dsHelper.PurgeAllDataStructures();
            dsHelper.PurgeAllResearchPlans();
        }


        [Test]
        public void Create_ShouldReturnCreatedTag()
        {
            using (var _tagManager = new TagManager())
            {

                // Arrange
                var tag = new Tag { Nr = 1.0, Final = false, Show = false, ReleaseDate = DateTime.Now};

                // Act
                Tag result = _tagManager.Create(tag);

                // Assert
                Assert.IsNotNull(result);
                Assert.AreEqual(tag.Id, result.Id);
                Assert.AreEqual(tag.Nr, result.Nr);
            }

        }

        [Test]
        public void Update_ShouldReturnUpdatedTag()
        {

            using (var _tagManager = new TagManager())
            {
                // Arrange
                var tag = new Tag { Nr = 1.0, Final = false, Show = false, ReleaseDate = DateTime.Now };

                tag = _tagManager.Create(tag);
                tag.Nr += 1.0;

                // Act
                var result = _tagManager.Update(tag);

                // Assert
                Assert.IsNotNull(result);
                Assert.AreEqual(tag.Id, result.Id);
                Assert.AreEqual(tag.Nr, 2.0);
            }
        }

        [Test]
        public void Delete_ById_ShouldReturnTrue()
        {
            using (var _tagManager = new TagManager())
            {
                // Arrange
                var tag = new Tag { Nr = 0.1, Final = false, Show = false, ReleaseDate = DateTime.Now };

                tag = _tagManager.Create(tag);

                // Act
                var result = _tagManager.Delete(tag.Id);

                // Assert
                Assert.IsTrue(result);
            
            }
        }
    }
}
