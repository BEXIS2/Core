using BExIS.App.Testing;
using BExIS.Dlm.Entities.Data;
using BExIS.Dlm.Entities.SpeciesMatching;
using BExIS.Dlm.Services.Data;
using BExIS.Dlm.Services.MetadataStructure;
using BExIS.Dlm.Services.SpeciesMatching;
using BExIS.Security.Services.Objects;
using BExIS.Security.Services.Subjects;
using BExIS.Utils.Config;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BExIS.Dlm.Tests.Services.SpeciesMatching
{
    internal class SpeciesMatchingResultManagerTest
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
            using (var speciesMatchingResultManager = new SpeciesMatchingResultManager())
            using (var userManager = new UserManager())
            using (var datasetManager = new DatasetManager())
            {
                //Arrange
                SpeciesMatchingResult matchingResult = new SpeciesMatchingResult();

                var user = userManager.Users.FirstOrDefault();
                var dataset = datasetManager.DatasetRepo.Get().FirstOrDefault();

                matchingResult.OriginalName = "Sunflower";
                matchingResult.EditedName = "";
                matchingResult.MatchedName = "";
                matchingResult.Status = "";
                matchingResult.MatchType = "";
                matchingResult.TimestampMatch = DateTime.Now;
                matchingResult.MatchSource = "";
                matchingResult.MatchSourceVersion = "";
                matchingResult.ConfirmedByUser = false;
                matchingResult.Dataset = dataset;
                matchingResult.DatasetVersionId = 1;

                //Act
                var created = speciesMatchingResultManager.Create(matchingResult);
                var fromdb = speciesMatchingResultManager.Repo.Get().LastOrDefault();

                //Assert
                Assert.IsNotNull(created);
                Assert.IsNotNull(fromdb);
                Assert.That(created.Id.Equals(fromdb.Id));
            }
        }
    }
}
