using BExIS.App.Testing;
using BExIS.Dlm.Entities.Curation;
using BExIS.Dlm.Entities.Data;
using BExIS.Dlm.Entities.DataStructure;
using BExIS.Dlm.Services.Administration;
using BExIS.Dlm.Services.Curation;
using BExIS.Dlm.Services.Data;
using BExIS.Dlm.Services.MetadataStructure;
using BExIS.Dlm.Tests.Helpers;
using BExIS.Security.Entities.Subjects;
using BExIS.Security.Services.Subjects;
using BExIS.Utils.Config;
using BExIS.Utils.NH.Querying;
using FluentAssertions;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using Vaiona.Persistence.Api;

namespace BExIS.Dlm.Tests.Services.Curation
{
    public class CurationEntryTests
    {
        private TestSetupHelper helper = null;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            helper = new TestSetupHelper(WebApiConfig.Register, false);

            using (UserManager userManager = new UserManager())
            using (var identityUserService = new IdentityUserService())
            {
                User admin = new User()
                {
                    Name = "Admin",
                    DisplayName = "Admin",
                    Email = "bexis2-support@uni-jena.de"
                };

                userManager.CreateAsync(admin).Wait();
            }
        }

        [SetUp]
        public void SetUp()
        {
            var dsHelper = new DatasetHelper();

        }

        [TearDown]
        public void TearDown()
        {


        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            using (var uow = this.GetUnitOfWork())
            {
                var repo = uow.GetRepository<CurationEntry>();

                var l = repo.Query().ToList();
                foreach (var item in l)
                {
                    repo.Delete(item);
                }
                uow.Commit();
            }

            var dsHelper = new DatasetHelper();
            dsHelper.PurgeAllDatasets();
            dsHelper.PurgeAllDataStructures();
            dsHelper.PurgeAllResearchPlans();

        }

        [Test()]
        public void Create_valid_CurationEntry()
        {

            //act
            using (var curationEntryManager = new CurationManager())
            using (var userManager = new UserManager())
            {

                //Arrange
                var dsHelper = new DatasetHelper();
                var ds = dsHelper.CreateDataset();
                var user = userManager.Users.FirstOrDefault();

                var curationEntry = curationEntryManager.Create(
                    "Test Topic", 
                    CurationEntryType.None, 
                    ds.Id, "Test Name", "Test Description", "Test Solution", 1, "Test Source", new List<CurationNote>(), false, false, user);


                //Assert
                curationEntry.Should().NotBeNull();
                curationEntry.Id.Should().BeGreaterThan(0);
                curationEntry.Topic.Should().Be("Test Topic");
                curationEntry.Type.Should().Be(CurationEntryType.None);
                curationEntry.Dataset.Should().Be(ds);
                curationEntry.Name.Should().Be("Test Name");
                curationEntry.Description.Should().Be("Test Description");
                curationEntry.Solution.Should().Be("Test Solution");
                curationEntry.Position.Should().Be(1);
                curationEntry.Source.Should().Be("Test Source");
                curationEntry.Notes.Should().NotBeNull();
  
                curationEntry.UserlsDone.Should().BeFalse();
                curationEntry.IsApproved.Should().BeFalse();
            }
        }

        [Test()]
        public void Delete_valid_CurationEntry()
        {

            //act
            using (var curationEntryManager = new CurationManager())
            using (var userManager = new UserManager())
            {

                //Arrange
                var dsHelper = new DatasetHelper();
                var ds = dsHelper.CreateDataset();
                var user = userManager.Users.FirstOrDefault();

                var curationEntry = curationEntryManager.Create(
                    "Test Topic",
                    CurationEntryType.None,
                    ds.Id, "Test Name", "Test Description", "Test Solution", 1, "Test Source", new List<CurationNote>(), false, false, user);


                //Assert
                curationEntry.Should().NotBeNull();

                curationEntryManager.Delete(curationEntry.Id);

                var curationEntryDeleted = curationEntryManager.CurationEntries.Where(c=> c.Id == curationEntry.Id);

                curationEntryDeleted.Should().BeEmpty();

            }
        }
    }
}