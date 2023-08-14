using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using FluentAssertions;
using BExIS.Dlm.Tests.Helpers;
using BExIS.Dlm.Entities.DataStructure;
using Vaiona.Persistence.Api;
using BExIS.App.Testing;
using BExIS.Utils.Config;
using NUnit.Framework;
using Newtonsoft.Json;

namespace BExIS.Dlm.Entities.Meanings.Tests
{
    [TestClass()]
    public class meaningManagerTests
    {
        private TestSetupHelper helper = null;
        [SetUp]
        public void SetUp()
        {
            helper = new TestSetupHelper(WebApiConfig.Register, false);
        }

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

        [TestMethod()]
        public void createTest()
        {
            new TestSetupHelper(WebApiConfig.Register, false);
            ImeaningManagr _meaningManager = new meaningManager();

            String Name = "meaning name for unit test";
            Name.Should().NotBeNull();
            String ShortName = "meaning name for unit test";
            ShortName.Should().NotBeNull();
            String Description = "meaning name for unit test";
            Description.Should().NotBeNull();
            Selectable selectable = (Selectable)Enum.Parse(typeof(Selectable), "1");
            selectable.Should().NotBeNull();
            Approved approved = (Approved)Enum.Parse(typeof(Approved), "1");
            approved.Should().NotBeNull();

            DatasetHelper dsHelper = new DatasetHelper();
            StructuredDataStructure dataStructure = dsHelper.CreateADataStructure();
            dataStructure.Should().NotBeNull();
            List<string> variable = dataStructure.Variables.ToList<Variable>().Select(c => c.Id.ToString()).ToList<string>();
            variable.Should().NotBeEmpty();

            _meaningManager.getExternalLinks()["Key"].ToString().Should().NotBeNullOrEmpty();
            _meaningManager.getExternalLinks()["Key"].ToString().Should().Be("Success");
            List<string> externalLink = JsonConvert.DeserializeObject<List<ExternalLink>>(_meaningManager.getExternalLinks()["Value"].ToString()).Select(c => c.Id.ToString()).ToList<string>();

            _meaningManager.getMeanings()["Key"].ToString().Should().NotBeNullOrEmpty();
            _meaningManager.getMeanings()["Key"].ToString().Should().Be("Success");
            List<string> related_meaning = JsonConvert.DeserializeObject<List<Meaning>>(_meaningManager.getExternalLinks()["Value"].ToString()).Select(c => c.Id.ToString()).ToList<string>();


            JObject res = _meaningManager.addMeaning(Name, ShortName, Description, selectable, approved, variable, externalLink, related_meaning);
            res["Key"].ToString().Should().NotBeNullOrEmpty();
            res["Key"].ToString().Should().Be("Success");
            NUnit.Framework.Assert.IsNotNull(res);
        }

        [TestMethod()]
        public void createExternalLink()
        {
            new TestSetupHelper(WebApiConfig.Register, false);

            ImeaningManagr _meaningManager = new meaningManager();

            String uri = Convert.ToString("htpp://testUri.com");
            uri.Should().NotBeNullOrEmpty();

            String name = Convert.ToString("test name external link");
            name.Should().NotBeNullOrEmpty();

            String type = Convert.ToString("test type external link ");
            type.Should().NotBeNullOrEmpty();

            JObject res = _meaningManager.addExternalLink(uri, name, type);
            res["Key"].ToString().Should().NotBeNullOrEmpty();
            res["Key"].ToString().Should().Be("Success");
            NUnit.Framework.Assert.IsNotNull(res);
        }

    }
}