using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using FluentAssertions;
using BExIS.Dlm.Tests.Helpers;
using BExIS.Dlm.Entities.DataStructure;
using BExIS.App.Testing;
using BExIS.Utils.Config;
using NUnit.Framework;
using Newtonsoft.Json;
using BExIS.Dlm.Services.Meanings;

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
        public void createMeaning()
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

            _meaningManager.getExternalLinks().ToString().Should().NotBeNullOrEmpty();
            List<string> externalLink = _meaningManager.getExternalLinks().Select(c => c.Id.ToString()).ToList<string>();

            _meaningManager.getMeanings().Should().NotBeNullOrEmpty();
            List<string> related_meaning = _meaningManager.getMeanings().Select(c => c.Id.ToString()).ToList<string>();


            Meaning res = _meaningManager.addMeaning(Name, ShortName, Description, selectable, approved, variable, externalLink, related_meaning);
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
            var obj = res;
            res = _meaningManager.editMeaning(res.Id.ToString(), Name, ShortName, Description, selectable, approved, variable, externalLink, related_meaning);
            res.Name.ToString().Should().Be("meaning name for unit test edited");
            res.ShortName.Should().Be("meaning name for unit test tedited");
            res.Description.Should().Be("meaning name for unit test edited");
            res.Selectable.Should().Be("0");
            res.Approved.Should().Be("0");
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

            ExternalLink res = _meaningManager.addExternalLink(uri, name, type);
            NUnit.Framework.Assert.IsNotNull(res);

            //editing an external link
            uri = Convert.ToString("htpp://testUri_edited_.com");
            uri.Should().NotBeNullOrEmpty();

            name = Convert.ToString("test name external link edited");
            name.Should().NotBeNullOrEmpty();

            type = Convert.ToString("test type external link edited");
            type.Should().NotBeNullOrEmpty();

            res = _meaningManager.editExternalLink(res.Id.ToString(),uri,name,type );
            res.URI.Should().Be("htpp://testUri_edited_.com");
            res.Name.Should().Be("test name external link edited");
            res.Type.Should().Be("test type external link edited");
            NUnit.Framework.Assert.IsNotNull(res);

        }

    }
}