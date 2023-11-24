using System;
using BExIS.Dlm.Tests.Helpers;
using BExIS.App.Testing;
using BExIS.Utils.Config;
using NUnit.Framework;
using BExIS.Dlm.Services.Meanings;
using BExIS.Dlm.Entities.DataStructure;
using BExIS.Dlm.Services.DataStructure;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;
using Vaiona.Persistence.Api;

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
            NUnit.Framework.Assert.IsNotNull(res);

        }

        [Test()]
        public void createExternalLink()
        {
            //Arrange
            ImeaningManagr _meaningManager = new MeaningManager();
            string Name = Convert.ToString("prefix category unit");
            Name.Should().NotBeNullOrEmpty();
            string Description = Convert.ToString("prefix category unit description");
            Description.Should().NotBeNullOrEmpty();
            PrefixCategory prefixCategory = new PrefixCategory(Name, Description);

            prefixCategory = _meaningManager.addPrefixCategory(prefixCategory);

            String uri = Convert.ToString("htpp://testUri.com");
            String name = Convert.ToString("test name external link");
            ExternalLinkType type = ExternalLinkType.prefix;

            ExternalLink res = _meaningManager.addExternalLink(uri, name, type, null, _meaningManager.getPrefixCategory(prefixCategory.Id));
            NUnit.Framework.Assert.IsNotNull(res);

            //creating another prefix to be edited an external link
            uri = Convert.ToString("htpp://testUri_edited_.com");
            name = Convert.ToString("test name external link edited");
            type = ExternalLinkType.prefix;
            res = _meaningManager.addExternalLink(uri, name, type, null, _meaningManager.getPrefixCategory(prefixCategory.Id));
            NUnit.Framework.Assert.IsNotNull(res);

            //editing a prefix to external link external link
            uri = Convert.ToString("htpp://testUri_edited_.com");
            name = Convert.ToString("test name external link edited");
            type = ExternalLinkType.dictionary;

            //Act
            res = _meaningManager.editExternalLink(res.Id.ToString(),uri,name,type , res, null);

            //Assert
            NUnit.Framework.Assert.That(res.URI.Equals("htpp://testUri_edited_.com"));
            NUnit.Framework.Assert.That(res.Name.Equals("test name external link edited"));
            NUnit.Framework.Assert.That(res.Type.Equals(ExternalLinkType.dictionary));
            NUnit.Framework.Assert.That(res.Prefix.Equals(res));
            NUnit.Framework.Assert.IsNull(res.prefixCategory);


        }

        [Test()]
        public void createMeaning_()
        {
            new TestSetupHelper(WebApiConfig.Register, false);
            ImeaningManagr _meaningManager = new MeaningManager();

            //create 3 external links : 1 type:relation and 2 with different types
            String uri = Convert.ToString("http://ad:hasconnection");
            String name = Convert.ToString("hasconnection");
            ExternalLinkType type = ExternalLinkType.relation;
            ExternalLink prefix = null;
            ExternalLink externalLinkRelation = _meaningManager.addExternalLink(uri, name, type, prefix, null);

            uri = Convert.ToString("http://ad:water");
            name = Convert.ToString("water");
            type = ExternalLinkType.Class;
            prefix = null;
            ExternalLink externalLinkClass = _meaningManager.addExternalLink(uri, name, type, prefix, null);

            uri = Convert.ToString("http://ad:soil");
            name = Convert.ToString("soil");
            type = ExternalLinkType.entitity;
            prefix = null;
            ExternalLink externalLinkEntity = _meaningManager.addExternalLink(uri, name, type, prefix, null);

            uri = Convert.ToString("http://ad:haschildren");
            name = Convert.ToString("has children");
            type = ExternalLinkType.relation;
            prefix = null;
            ExternalLink externalLinkRelation2 = _meaningManager.addExternalLink(uri, name, type, prefix, null);

            MeaningEntry mapping1 = new MeaningEntry();
            mapping1.MappingRelation = externalLinkRelation;
            mapping1.MappedLinks = new List<ExternalLink> { externalLinkClass, externalLinkEntity };

            MeaningEntry mapping2 = new MeaningEntry();
            mapping2.MappingRelation = externalLinkRelation2;
            mapping2.MappedLinks = new List<ExternalLink> { externalLinkEntity };

            List<MeaningEntry> entries_annotations = new List<MeaningEntry>() { mapping1, mapping2 };

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

            Meaning meaning = new Meaning(Name, ShortName, Description, selectable, approved, entries_annotations, null);

            meaning = _meaningManager.addMeaning(meaning);
            NUnit.Framework.Assert.IsNotNull(meaning);

            meaning.Name = "meaning name for unit test edited";
            meaning.Name.Should().NotBeNull();
            meaning.ShortName = "meaning name for unit test tedited";
            meaning.ShortName.Should().NotBeNull();
            meaning.Description = "meaning name for unit test edited";
            meaning.Description.Should().NotBeNull();
            meaning.Selectable = (Selectable)Enum.Parse(typeof(Selectable), "0");
            meaning.Selectable.Should().NotBeNull();
            meaning.Approved = (Approved)Enum.Parse(typeof(Approved), "0");
            meaning.Approved.Should().NotBeNull();
            meaning.ExternalLinks = new List<MeaningEntry>();
            meaning.Related_meaning = new List<Meaning>();

            meaning = _meaningManager.editMeaning(meaning);
            meaning.Name.ToString().Should().Be("meaning name for unit test edited");
            meaning.ShortName.Should().Be("meaning name for unit test tedited");
            meaning.Description.Should().Be("meaning name for unit test edited");
            meaning.Selectable.Should().Be(0);
            meaning.Approved.Should().Be(0);
            meaning.ExternalLinks.Count().Should().Be(0);
            meaning.Related_meaning.Count().Should().Be(0);

            Meaning childmeaning = new Meaning();
            // trying to edit the inserdted command 
            childmeaning.Name = "meaning name for unit test edited";
            childmeaning.Name.Should().NotBeNull();
            childmeaning.ShortName = "meaning name for unit test tedited";
            childmeaning.ShortName.Should().NotBeNull();
            childmeaning.Description = "meaning name for unit test edited";
            childmeaning.Description.Should().NotBeNull();
            childmeaning.Selectable = (Selectable)Enum.Parse(typeof(Selectable), "0");
            childmeaning.Selectable.Should().NotBeNull();
            childmeaning.Approved = (Approved)Enum.Parse(typeof(Approved), "0");
            childmeaning.Approved.Should().NotBeNull();
            childmeaning.ExternalLinks = new List<MeaningEntry>();
            childmeaning.Related_meaning = new List<Meaning>() { meaning };

            childmeaning = _meaningManager.addMeaning(childmeaning);
            childmeaning.Name.ToString().Should().Be("meaning name for unit test edited");
            childmeaning.ShortName.Should().Be("meaning name for unit test tedited");
            childmeaning.Description.Should().Be("meaning name for unit test edited");
            childmeaning.Selectable.Should().Be(0);
            childmeaning.Approved.Should().Be(0);
            childmeaning.ExternalLinks.Count().Should().Be(0);
            childmeaning.Related_meaning.Count().Should().Be(1);
            NUnit.Framework.Assert.IsNotNull(childmeaning);

        }

        [Test()]
        public void createMeaning_2()
        {
            new TestSetupHelper(WebApiConfig.Register, false);
            ImeaningManagr _meaningManager = new MeaningManager();

            //create 3 external links : 1 type:relation and 2 with different types
            String uri = Convert.ToString("http://ad:hasconnection");
            String name = Convert.ToString("hasconnection");
            ExternalLinkType type = ExternalLinkType.relation;
            ExternalLink prefix = null;
            ExternalLink externalLinkRelation = new ExternalLink(uri, name, type, prefix, null);

            uri = Convert.ToString("http://ad:water");
            name = Convert.ToString("water");
            type = ExternalLinkType.Class;
            prefix = null;
            ExternalLink externalLinkClass = new ExternalLink(uri, name, type, prefix, null);

            uri = Convert.ToString("http://ad:soil");
            name = Convert.ToString("soil");
            type = ExternalLinkType.entitity;
            prefix = null;
            ExternalLink externalLinkEntity = new ExternalLink(uri, name, type, prefix, null);

            uri = Convert.ToString("http://ad:haschildren");
            name = Convert.ToString("has children");
            type = ExternalLinkType.relation;
            prefix = null;
            ExternalLink externalLinkRelation2 = new ExternalLink(uri, name, type, prefix, null);

            MeaningEntry mapping1 = new MeaningEntry();
            mapping1.MappingRelation = externalLinkRelation;
            mapping1.MappedLinks = new List<ExternalLink> { externalLinkClass, externalLinkEntity };

            MeaningEntry mapping2 = new MeaningEntry();
            mapping2.MappingRelation = externalLinkRelation2;
            mapping2.MappedLinks = new List<ExternalLink> { externalLinkEntity };

            List<MeaningEntry> entries_annotations = new List<MeaningEntry>() { mapping1, mapping2 };

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

            Meaning meaning = new Meaning(Name, ShortName, Description, selectable, approved, entries_annotations, null);

            meaning = _meaningManager.addMeaning(meaning);
            NUnit.Framework.Assert.IsNotNull(meaning);

            meaning.Name = "meaning name for unit test edited";
            meaning.Name.Should().NotBeNull();
            meaning.ShortName = "meaning name for unit test tedited";
            meaning.ShortName.Should().NotBeNull();
            meaning.Description = "meaning name for unit test edited";
            meaning.Description.Should().NotBeNull();
            meaning.Selectable = (Selectable)Enum.Parse(typeof(Selectable), "0");
            meaning.Selectable.Should().NotBeNull();
            meaning.Approved = (Approved)Enum.Parse(typeof(Approved), "0");
            meaning.Approved.Should().NotBeNull();
            meaning.ExternalLinks = new List<MeaningEntry>();
            meaning.Related_meaning = new List<Meaning>();

            meaning = _meaningManager.editMeaning(meaning);
            meaning.Name.ToString().Should().Be("meaning name for unit test edited");
            meaning.ShortName.Should().Be("meaning name for unit test tedited");
            meaning.Description.Should().Be("meaning name for unit test edited");
            meaning.Selectable.Should().Be(0);
            meaning.Approved.Should().Be(0);
            meaning.ExternalLinks.Count().Should().Be(0);
            meaning.Related_meaning.Count().Should().Be(0);

            Meaning childmeaning = new Meaning();
            // trying to edit the inserdted command 
            childmeaning.Name = "meaning name for unit test edited";
            childmeaning.Name.Should().NotBeNull();
            childmeaning.ShortName = "meaning name for unit test tedited";
            childmeaning.ShortName.Should().NotBeNull();
            childmeaning.Description = "meaning name for unit test edited";
            childmeaning.Description.Should().NotBeNull();
            childmeaning.Selectable = (Selectable)Enum.Parse(typeof(Selectable), "0");
            childmeaning.Selectable.Should().NotBeNull();
            childmeaning.Approved = (Approved)Enum.Parse(typeof(Approved), "0");
            childmeaning.Approved.Should().NotBeNull();
            childmeaning.ExternalLinks = new List<MeaningEntry>();
            childmeaning.Related_meaning = new List<Meaning>() { meaning };

            childmeaning = _meaningManager.addMeaning(childmeaning);
            childmeaning.Name.ToString().Should().Be("meaning name for unit test edited");
            childmeaning.ShortName.Should().Be("meaning name for unit test tedited");
            childmeaning.Description.Should().Be("meaning name for unit test edited");
            childmeaning.Selectable.Should().Be(0);
            childmeaning.Approved.Should().Be(0);
            childmeaning.ExternalLinks.Count().Should().Be(0);
            childmeaning.Related_meaning.Count().Should().Be(1);
            NUnit.Framework.Assert.IsNotNull(childmeaning);
        }

        [Test()]
        public void createExternalLink_()
        {
            new TestSetupHelper(WebApiConfig.Register, false);

            ImeaningManagr _meaningManager = new MeaningManager();

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

            type = ExternalLinkType.dictionary;
            type.Should().Be(7);

            prefix = res_prefix_parent;
            prefix.Should().NotBeNull();

            ExternalLink res_link_prefix = _meaningManager.addExternalLink(uri, name, type, prefix, null);
            NUnit.Framework.Assert.IsNotNull(res_link_prefix);

            //editing an external link
            uri = Convert.ToString("htpp://testUri_edited_aquadiva.com");
            uri.Should().NotBeNullOrEmpty();

            name = Convert.ToString("test name external link edited water groumds");
            name.Should().NotBeNullOrEmpty();

            type = ExternalLinkType.dictionary;
            type.Should().NotBeNull();

            res_link_prefix = _meaningManager.editExternalLink(res_link_prefix.Id.ToString(), uri, name, type, prefix, null);
            res_link_prefix.URI.Should().Be("htpp://testUri_edited_aquadiva.com");
            res_link_prefix.Name.Should().Be("test name external link edited water groumds");
            res_link_prefix.Type.Should().Be(7);
            res_link_prefix.Prefix.Should().Equals(res_prefix_parent);
            NUnit.Framework.Assert.IsNotNull(res_link_prefix);
        }

        [Test()]
        public void clear_test_persistent_data()
        {
            new TestSetupHelper(WebApiConfig.Register, false);
            ImeaningManagr _meaningManager = new MeaningManager();
            foreach (Meaning m in _meaningManager.getMeanings())
            {
                _meaningManager.deleteMeaning(m);
            }
            foreach (ExternalLink el in _meaningManager.getExternalLinks())
            {
                _meaningManager.deleteExternalLink(el);
            }
        }
    }
}