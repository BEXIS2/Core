using System;
using BExIS.Dlm.Tests.Helpers;
using BExIS.App.Testing;
using BExIS.Utils.Config;
using NUnit.Framework;
using BExIS.Dlm.Services.Meanings;

namespace BExIS.Dlm.Entities.Meanings.Tests
{ 
    public class MeaningManagerTests
    {
        private TestSetupHelper helper = null;

        [SetUp]
        public void SetUp()
        {
           
        }

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            helper = new TestSetupHelper(WebApiConfig.Register, false);
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {

            DatasetHelper dsHelper = new DatasetHelper();
            dsHelper.PurgeAllDataStructures();



            helper.Dispose();
        }

        [Test()]
        public void create_simpleMeaning_returnMeaning()
        {
            //Arrange
            ImeaningManagr _meaningManager = new MeaningManager();

            Meaning meaning = new Meaning();
            meaning.Name = "meaning name for unit test";
            meaning.ShortName = "meaning name for unit test";
            meaning.Description = "meaning name for unit test";
            meaning.Selectable = (Selectable)Enum.Parse(typeof(Selectable), "1");
            meaning.Approved = (Approved)Enum.Parse(typeof(Approved), "1");

            //Act
            Meaning res = _meaningManager.addMeaning(meaning);


            //Assert
            Assert.IsNotNull(res);

        }

        [Test()]
        public void createExternalLink()
        {
            //Arrange
            ImeaningManagr _meaningManager = new MeaningManager();

            String uri = Convert.ToString("htpp://testUri.com");
            String name = Convert.ToString("test name external link");
            String type = Convert.ToString("test type external link ");

            ExternalLink res = _meaningManager.addExternalLink(uri, name, type);
            NUnit.Framework.Assert.IsNotNull(res);

            //editing an external link
            uri = Convert.ToString("htpp://testUri_edited_.com");
            name = Convert.ToString("test name external link edited");
            type = Convert.ToString("test type external link edited");

            //Act
            res = _meaningManager.editExternalLink(res.Id.ToString(),uri,name,type );

            //Assert
            Assert.That(res.URI.Equals("htpp://testUri_edited_.com"));
            Assert.That(res.Name.Equals("test name external link edited"));
            Assert.That(res.Type.Equals("test type external link edited"));


        }

    }
}