﻿using BExIS.App.Testing;
using BExIS.Dlm.Services.Meanings;
using BExIS.Utils.Config;
using FluentAssertions;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BExIS.Dlm.Entities.Meanings.Tests
{
    public class MeaningManagerTests
    {
        private TestSetupHelper helper = null;
        private MeaningManager _meaningManager = null;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            helper = new TestSetupHelper(WebApiConfig.Register, false);

        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            helper?.Dispose();
        }

        //////////////////////////////////////////
        /// setups before every test function
        //////////////////////////////////////////
        [SetUp]
        public void SetUp()
        {
            _meaningManager = new MeaningManager();
        }

        [TearDown]
        public void CleanUp()
        {
            _meaningManager.Dispose();
        }

        //////////////////////////////////////////
        /// create and edit prefix categories
        //////////////////////////////////////////
        [Test(), Order(1)]
        public void _createPrefixCategory_returnPrefixCategory()
        {
            string Name = Convert.ToString("prefix category Name 1");
            Name.Should().NotBeNullOrEmpty();
            string Description = Convert.ToString("prefix category Name description 1");
            Description.Should().NotBeNullOrEmpty();
            PrefixCategory prefixCategory = new PrefixCategory(Name, Description);

            prefixCategory = _meaningManager.AddPrefixCategory(prefixCategory);
            NUnit.Framework.Assert.IsNotNull(prefixCategory);

        }

        [Test(), Order(2)]
        public void _editPrefixCategory_returnPrefixCategory()
        {
            PrefixCategory prefixCategory = _meaningManager.GetPrefixCategory().FirstOrDefault();
            string Old_name = prefixCategory.Name;
            prefixCategory.Name = Convert.ToString("prefix category Name 1 edited");
            prefixCategory.Description = Convert.ToString("prefix category Name description 1 edited");
            prefixCategory = _meaningManager.EditPrefixCategory(prefixCategory);
            NUnit.Framework.Assert.IsNotNull(prefixCategory);
            _meaningManager.GetPrefixCategory(Old_name).Should().BeNull();
            _meaningManager.GetPrefixCategory(Old_name).Should().BeNull();
        }

        //////////////////////////////////////////
        /// create and edit External Links
        //////////////////////////////////////////
        [Test(), Order(3)]
        public void _createExternalLinkasPrefix_returnExternalLink()
        {
            String uri = Convert.ToString("http://aquadiva.com");
            String name = Convert.ToString("AquaDiva Prefix");

            ExternalLink el1 = _meaningManager.AddExternalLink(uri, name, ExternalLinkType.prefix, null, _meaningManager.GetPrefixCategory(Convert.ToString("prefix category Name 1 edited")));
            NUnit.Framework.Assert.IsNotNull(el1);
        }

        [Test(), Order(4)]
        public void _createExternalLinkEntityasChildren_returnExternalLink()
        {
            String uri = Convert.ToString("http://aquadiva.com/water");
            String name = Convert.ToString("water");
            ExternalLink prefix = _meaningManager.GetPrefixes().FirstOrDefault(x => x.Name == "AquaDiva Prefix");
            ExternalLink el1 = _meaningManager.AddExternalLink(uri, name, ExternalLinkType.entity, prefix, null);
            NUnit.Framework.Assert.IsNotNull(el1);
        }

        [Test(), Order(5)]
        public void _createExternalLinkCharac_returnExternalLink()
        {
            String uri = Convert.ToString("http://aquadiva.com/PH");
            String name = Convert.ToString("PH");
            ExternalLink el1 = _meaningManager.AddExternalLink(uri, name, ExternalLinkType.characteristics, null, null);
            NUnit.Framework.Assert.IsNotNull(el1);
        }

        [Test(), Order(6)]
        public void _createExternalLinkAsRelation_returnExternalLink()
        {
            String uri = Convert.ToString("http://RelationURL/hasSynonym");
            String name = Convert.ToString("hasSynonym");
            ExternalLink el1 = _meaningManager.AddExternalLink(uri, name, ExternalLinkType.relationship, null, null);
            NUnit.Framework.Assert.IsNotNull(el1);
        }

        [Test(), Order(7)]
        public void _createExternalLinkAsVocaB_returnExternalLink()
        {
            String uri = Convert.ToString("http://vocabURL/deepwater");
            String name = Convert.ToString("deepwater");
            ExternalLink el1 = _meaningManager.AddExternalLink(uri, name, ExternalLinkType.vocabulary, null, null);
            NUnit.Framework.Assert.IsNotNull(el1);
        }

        [Test(), Order(8)]
        public void _editExternalLinkAsEnt_returnExternalLink()
        {
            ExternalLink el1 = _meaningManager.GetExternalLinks().FirstOrDefault(x => _meaningManager.GetViewLinkUri(x) == "http://aquadiva.com/water");
            string Old_URI = el1.URI;
            el1.URI = "http://aquadiva.com/groundwater";
            el1.Name = Convert.ToString("groundwater");
            el1 = _meaningManager.EditExternalLink(el1);
            NUnit.Framework.Assert.IsNotNull(el1);
            _meaningManager.GetExternalLink(Old_URI).Should().BeNull();
            _meaningManager.GetExternalLink(Old_URI).Should().BeNull();
        }

        //////////////////////////////////////////
        /// create and edit Meanings
        //////////////////////////////////////////
        [Test(), Order(9)]
        public void _createMeaningParent_returnMeaning()
        {
            Meaning meaning = new Meaning();
            meaning.Name = "Parent meaning name";
            meaning.ShortName = "Parent meaning name";
            meaning.Description = "Parent meaning description";
            meaning.Selectable = false;
            meaning.Approved = false;
            Meaning res = _meaningManager.AddMeaning(meaning);
            NUnit.Framework.Assert.IsNotNull(res);
        }

        [Test(), Order(10)]
        public void _createMeaningChild_returnMeaning()
        {
            Meaning meaning = new Meaning();
            meaning.Name = "1 Child meaning name";
            meaning.ShortName = "1 Child meaning name";
            meaning.Description = "1 Child meaning description";
            meaning.Selectable = false;
            meaning.Approved = false;
            Meaning parent = _meaningManager.GetMeanings().FirstOrDefault(x => x.Name == "Parent meaning name");
            Meaning res = _meaningManager.AddMeaning(meaning);
            NUnit.Framework.Assert.IsNotNull(res);

            meaning = new Meaning();
            meaning.Name = "2 Child meaning name";
            meaning.ShortName = "2 Child meaning name";
            meaning.Description = "2 Child meaning description";
            meaning.Selectable = false;
            meaning.Approved = false;
            parent = _meaningManager.GetMeanings().FirstOrDefault(x => x.Name == "Parent meaning name");
            res = _meaningManager.AddMeaning(meaning);
            NUnit.Framework.Assert.IsNotNull(res);
        }

        [Test(), Order(11)]
        public void _edit2EmptyMeaningTo1SharedMeaningEntries_returnMeaning()
        {
            Meaning meaning1 = _meaningManager.GetMeanings().FirstOrDefault(x => x.Name == "1 Child meaning name");

            ExternalLink relation = _meaningManager.GetExternalLink("http://vocabURL/hasSynonym");
            ExternalLink ent = _meaningManager.GetExternalLink("http://aquadiva.com/groundwater");
            ExternalLink voc = _meaningManager.GetExternalLink("http://vocabURL/deepwater");
            ExternalLink charac = _meaningManager.GetExternalLink("http://aquadiva.com/PH");

            // 1st mapping
            MeaningEntry me1 = new MeaningEntry(relation, new List<ExternalLink> { ent, voc });
            // 2nd mapping
            MeaningEntry me2 = new MeaningEntry(relation, new List<ExternalLink> { ent, charac });

            meaning1.ExternalLinks = new List<MeaningEntry>() { me1, me2 }; ;
            meaning1 = _meaningManager.EditMeaning(meaning1);
            NUnit.Framework.Assert.IsNotNull(meaning1);
            _meaningManager.GetMeaning(meaning1.Id).ExternalLinks.Should().NotBeEmpty();

            Meaning meaning2 = _meaningManager.GetMeanings().FirstOrDefault(x => x.Name == "2 Child meaning name");
            meaning2.ExternalLinks = new List<MeaningEntry>() { me1 }; ;
            meaning2 = _meaningManager.EditMeaning(meaning2);
            NUnit.Framework.Assert.IsNotNull(meaning2);
            _meaningManager.GetMeaning(meaning2.Id).ExternalLinks.Should().NotBeEmpty();
        }

        [Test(), Order(12)]
        public void deleteMeaningSharingMeaningEntriesWithOtherMeaning_returnMeaning()
        {
            Meaning meaning1 = _meaningManager.GetMeanings().FirstOrDefault(x => x.Name == "1 Child meaning name");
            Boolean res = _meaningManager.DeleteMeaning(meaning1);
            res.Should().BeTrue();
            Meaning meaning2 = _meaningManager.GetMeanings().FirstOrDefault(x => x.Name == "2 Child meaning name");
            meaning2.ExternalLinks.Should().NotBeEmpty();
        }

        //////////////////////////////////////////
        /// Delete all test data
        //////////////////////////////////////////
        [Test(), Order(13)]
        public void _delete_all_test_data()
        {
            foreach (Meaning m in _meaningManager.GetMeanings().Where(x => x.Related_meaning?.Count() != 0).Where(x => x.Related_meaning != null))
            {
                _meaningManager.DeleteMeaning(m);
            }
            foreach (Meaning m in _meaningManager.GetMeanings())
            {
                _meaningManager.DeleteMeaning(m);
            }
            foreach (ExternalLink el in _meaningManager.GetExternalLinks().Where(x => x.Type != ExternalLinkType.prefix))
            {
                _meaningManager.DeleteExternalLink(el);
            }
            foreach (ExternalLink el in _meaningManager.GetExternalLinks())
            {
                _meaningManager.DeleteExternalLink(el);
            }
            foreach (PrefixCategory pc in _meaningManager.GetPrefixCategory())
            {
                _meaningManager.DeletePrefixCategory(pc);
            }
        }
    }
}