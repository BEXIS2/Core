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
using BExIS.Dlm.Services.DataStructure;
using System.Security.Cryptography;
using Vaiona.Persistence.Api;

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

            String Name = "meaning name for unit test_";
            Name.Should().NotBeNull();
            String ShortName = "meaning name for unit test_";
            ShortName.Should().NotBeNull();
            String Description = "meaning name for unit test_";
            Description.Should().NotBeNull();
            Selectable selectable = (Selectable)Enum.Parse(typeof(Selectable), "1");
            selectable.Should().NotBeNull();
            Approved approved = (Approved)Enum.Parse(typeof(Approved), "1");
            approved.Should().NotBeNull();

            DataStructureManager dsm = new DataStructureManager();
            var structureRepo = dsm.GetUnitOfWork().GetReadOnlyRepository<StructuredDataStructure>();
            StructuredDataStructure dataStructure = structureRepo.Query(p => p.Name.ToLower().Equals("dsfortesting")).FirstOrDefault();
            if (structureRepo.Query(p => p.Name.ToLower().Equals("dsfortesting")).Count() <= 0)
            {
                DatasetHelper dsHelper = new DatasetHelper();
                dataStructure = dsHelper.CreateADataStructure();
            }
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
            res.Selectable.Should().Be(0);
            res.Approved.Should().Be(0);
            NUnit.Framework.Assert.IsNotNull(res); 

        }

        [TestMethod()]
        public void createExternalLink()
        {
            new TestSetupHelper(WebApiConfig.Register, false);

            ImeaningManagr _meaningManager = new meaningManager();

            string Name = Convert.ToString("prefix category unit");
            Name.Should().NotBeNullOrEmpty();
            string Description = Convert.ToString("prefix category unit description");
            Description.Should().NotBeNullOrEmpty();
            PrefixCategory prefixCategory = new PrefixCategory(Name, Description);


            String uri = Convert.ToString("http://darwinprefix.com");
            uri.Should().NotBeNullOrEmpty();

            String name = Convert.ToString("darwin prefix");
            name.Should().NotBeNullOrEmpty();

            ExternalLinkType type = ExternalLinkType.prefix;
            type.Should().Be(1);

            ExternalLink prefix = null;
            prefix.Should().BeNull();

            ExternalLink res_prefix_parent = _meaningManager.addExternalLink(uri, name, type, prefix, _meaningManager.getPrefixCategory(prefixCategory.Id));
            NUnit.Framework.Assert.IsNotNull(res_prefix_parent);

            uri = Convert.ToString("http://darwinprefix.com/water");
            uri.Should().NotBeNullOrEmpty();

            name = Convert.ToString("darwin water");
            name.Should().NotBeNullOrEmpty();

            type = ExternalLinkType.link;
            type.Should().Be(2);

            prefix = res_prefix_parent;
            prefix.Should().NotBeNull();

            ExternalLink res_link_prefix = _meaningManager.addExternalLink(uri, name, type, prefix, null);
            NUnit.Framework.Assert.IsNotNull(res_link_prefix);

            //editing an external link
            uri = Convert.ToString("htpp://testUri_edited_aquadiva.com");
            uri.Should().NotBeNullOrEmpty();

            name = Convert.ToString("test name external link edited water groumds");
            name.Should().NotBeNullOrEmpty();

            type = ExternalLinkType.link;
            type.Should().NotBeNull();

            res_link_prefix = _meaningManager.editExternalLink(res_link_prefix.Id.ToString(),uri,name,type,prefix,null );
            res_link_prefix.URI.Should().Be("htpp://testUri_edited_aquadiva.com");
            res_link_prefix.Name.Should().Be("test name external link edited water groumds");
            res_link_prefix.Type.Should().Be(2);
            res_link_prefix.Prefix.Should().Equals(res_prefix_parent);
            NUnit.Framework.Assert.IsNotNull(res_link_prefix);
        }

    }
}