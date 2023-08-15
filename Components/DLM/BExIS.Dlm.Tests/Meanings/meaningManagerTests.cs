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
using NHibernate.Mapping;
using System.Security.Cryptography.X509Certificates;

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

            // trying to edit the inserdted command 
            Name = "meaning name for unit test edited";
            Name.Should().NotBeNull();
            ShortName = "meaning name for unit test tedited";
            ShortName.Should().NotBeNull();
            Description = "meaning name for unit test edited";
            Description.Should().NotBeNull();
            selectable = (Selectable)Enum.Parse(typeof(Selectable), "0");
            selectable.Should().NotBeNull();
            approved = (Approved)Enum.Parse(typeof(Approved), "0");
            approved.Should().NotBeNull();
            var obj = JObject.Parse(res["Value"].ToString());
            res = _meaningManager.editMeaning(obj["Id"].ToString(), Name, ShortName, Description, selectable, approved, variable, externalLink, related_meaning);
            JObject.Parse(res["Value"].ToString())["Name"].ToString().Should().Be("meaning name for unit test edited");
            JObject.Parse(res["Value"].ToString())["ShortName"].ToString().Should().Be("meaning name for unit test tedited");
            JObject.Parse(res["Value"].ToString())["Description"].ToString().Should().Be("meaning name for unit test edited");
            JObject.Parse(res["Value"].ToString())["Selectable"].ToString().Should().Be("0");
            JObject.Parse(res["Value"].ToString())["Approved"].ToString().Should().Be("0");
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

            //editing an external link
            uri = Convert.ToString("htpp://testUri_edited_.com");
            uri.Should().NotBeNullOrEmpty();

            name = Convert.ToString("test name external link edited");
            name.Should().NotBeNullOrEmpty();

            type = Convert.ToString("test type external link edited");
            type.Should().NotBeNullOrEmpty();

            var obj = JObject.Parse(res["Value"].ToString());
            res = _meaningManager.editExternalLink(obj["Id"].ToString(),uri,name,type );
            JObject.Parse(res["Value"].ToString())["URI"].ToString().Should().Be("htpp://testUri_edited_.com");
            JObject.Parse(res["Value"].ToString())["Name"].ToString().Should().Be("test name external link edited");
            JObject.Parse(res["Value"].ToString())["Type"].ToString().Should().Be("test type external link edited");
            NUnit.Framework.Assert.IsNotNull(res);

        }

    }
}