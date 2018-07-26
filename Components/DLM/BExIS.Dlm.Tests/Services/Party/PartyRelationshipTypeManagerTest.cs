using System;
using BExIS.App.Testing;
using BExIS.Utils.Config;
using NUnit.Framework;
using BExIS.Dlm.Services.Party;
using FluentAssertions;
using System.Linq;
using BExIS.Dlm.Entities.Party;
using System.Collections.Generic;

namespace BExIS.Dlm.Tests.Services.Party
{
    public class PartyRelationshipTypeManagerTest
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
            helper.Dispose();
        }

        [Test()]
        public void CreatePartyRelationshipAndTypePairTest()
        {
            PartyRelationshipTypeManager partyRelationshipTypeManager = new PartyRelationshipTypeManager();
            var partyStatusTypes = new List<PartyStatusType>();
            partyStatusTypes.Add(new PartyStatusType() { Name = "Created", Description = "" });

            PartyTypeManager partyTypeManager = new PartyTypeManager();
            var sourcePartyType = partyTypeManager.Create("sourceTestType", "", "", partyStatusTypes);
            var targetPartyType = partyTypeManager.Create("targetTestType", "", "", partyStatusTypes);
            //create relationshiptype and one "typePair"
            var partyRelationshipType =  partyRelationshipTypeManager.Create("title", "T i t l e", "Descr", false, 100, -1, false, sourcePartyType, targetPartyType, "typepairTitle", "", "", "", 0);
            long partyRelationshipTypeId = partyRelationshipType.Id;
            PartyTypePair partyRelationshipTypePair1 = partyRelationshipTypeManager.PartyTypePairRepository.Get( cc=>cc.PartyRelationshipType.Id== partyRelationshipType.Id).First();
            long partyRelationshipTypePairId = partyRelationshipTypePair1.Id;
            partyRelationshipType.Should().NotBeNull();
            partyRelationshipType.Id.Should().BeGreaterThan(0);
            var fetchedpartyRelationshipType = partyRelationshipTypeManager.PartyRelationshipTypeRepository.Get(partyRelationshipType.Id);
            partyRelationshipType.Title.Should().BeEquivalentTo(fetchedpartyRelationshipType.Title);
            partyRelationshipType.DisplayName.Should().BeEquivalentTo(fetchedpartyRelationshipType.DisplayName);
            partyRelationshipType.Description.Should().BeEquivalentTo(fetchedpartyRelationshipType.Description);
            partyRelationshipType.IndicatesHierarchy.Should().Equals(fetchedpartyRelationshipType.IndicatesHierarchy);
            partyRelationshipType.MaxCardinality.Should().Equals(fetchedpartyRelationshipType.MaxCardinality);
            partyRelationshipType.MinCardinality.Should().Equals(fetchedpartyRelationshipType.MinCardinality);
            //Add second "partyTypePair"
            var partyRelationshipTypePair2 = partyRelationshipTypeManager.AddPartyTypePair("typePair2", sourcePartyType, targetPartyType, "pair description", true, partyRelationshipType, "", "", 0);
            var partyRelationshipTypePair2Id = partyRelationshipTypePair2.Id;
            partyRelationshipTypePair2.Should().NotBeNull();
            partyRelationshipTypePair2.Id.Should().BeGreaterThan(0);
            // partyRelationshipTypePair1 added differently than the other one; therefor, two test cases needed
            // test partyRelationshipTypePair1
            var fetchedpartyRelationshipTypePair1 = partyRelationshipTypeManager.PartyTypePairRepository.Get(partyRelationshipTypePairId);
            partyRelationshipTypePair1.SourcePartyType.Should().Equals(fetchedpartyRelationshipTypePair1.SourcePartyType);
            partyRelationshipTypePair1.TargetPartyType.Should().Equals(fetchedpartyRelationshipTypePair1.TargetPartyType);
            partyRelationshipTypePair1.ConditionSource.Should().BeEquivalentTo(fetchedpartyRelationshipTypePair1.ConditionSource);
            partyRelationshipTypePair1.ConditionTarget.Should().BeEquivalentTo(fetchedpartyRelationshipTypePair1.ConditionTarget);
            partyRelationshipTypePair1.Description.Should().BeEquivalentTo(fetchedpartyRelationshipTypePair1.Description);
            partyRelationshipTypePair1.PartyRelationShipTypeDefault.Should().Equals(fetchedpartyRelationshipTypePair1.PartyRelationShipTypeDefault);
            partyRelationshipTypePair1.PermissionTemplate.Should().Equals(fetchedpartyRelationshipTypePair1.PermissionTemplate);
            partyRelationshipTypePair1.Title.Should().Equals(fetchedpartyRelationshipTypePair1.Title);
            partyRelationshipTypePair1.PartyRelationshipType.Should().Equals(fetchedpartyRelationshipTypePair1.PartyRelationshipType);
            // test partyRelationshipTypePair2
            var fetchedpartyRelationshipTypePair2 = partyRelationshipTypeManager.PartyTypePairRepository.Get(partyRelationshipTypePair2Id);
            partyRelationshipTypePair2.SourcePartyType.Should().Equals(fetchedpartyRelationshipTypePair2.SourcePartyType);
            partyRelationshipTypePair2.TargetPartyType.Should().Equals(fetchedpartyRelationshipTypePair2.TargetPartyType);
            partyRelationshipTypePair2.ConditionSource.Should().BeEquivalentTo(fetchedpartyRelationshipTypePair2.ConditionSource);
            partyRelationshipTypePair2.ConditionTarget.Should().BeEquivalentTo(fetchedpartyRelationshipTypePair2.ConditionTarget);
            partyRelationshipTypePair2.Description.Should().BeEquivalentTo(fetchedpartyRelationshipTypePair2.Description);
            partyRelationshipTypePair2.PartyRelationShipTypeDefault.Should().Equals(fetchedpartyRelationshipTypePair2.PartyRelationShipTypeDefault);
            partyRelationshipTypePair2.PermissionTemplate.Should().Equals(fetchedpartyRelationshipTypePair2.PermissionTemplate);
            partyRelationshipTypePair2.Title.Should().Equals(fetchedpartyRelationshipTypePair2.Title);
            partyRelationshipTypePair2.PartyRelationshipType.Should().Equals(fetchedpartyRelationshipTypePair2.PartyRelationshipType);
            // cleanup the DB           
            partyRelationshipTypeManager.Delete(partyRelationshipType);
            partyTypeManager.Delete(sourcePartyType);
            partyTypeManager.Delete(targetPartyType);
            //check if delete is successful
            PartyRelationshipType partyRelationshipTypeAfterDelete = partyRelationshipTypeManager.PartyRelationshipTypeRepository.Get(cc => cc.Id == partyRelationshipTypeId).FirstOrDefault();
            //By Deleting relationshiptype, partyTypePairs should be deleted as well
            PartyTypePair partyTypePair1AfterDelete = partyRelationshipTypeManager.PartyTypePairRepository.Get(cc => cc.Id == partyRelationshipTypePairId).FirstOrDefault();
            PartyTypePair partyTypePair2AfterDelete = partyRelationshipTypeManager.PartyTypePairRepository.Get(cc => cc.Id == partyRelationshipTypePair2Id).FirstOrDefault();
            partyRelationshipTypeAfterDelete.Should().BeNull();
            partyTypePair1AfterDelete.Should().BeNull();
            partyTypePair2AfterDelete.Should().BeNull();
        }

        [Test()]
        public void UpdatePartyRelationshipAndTypePairTest()
        {
            PartyRelationshipTypeManager partyRelationshipTypeManager = new PartyRelationshipTypeManager();
            PartyTypeManager partyTypeManager = new PartyTypeManager();
            var partyStatusTypes = new List<PartyStatusType>();
            partyStatusTypes.Add(new PartyStatusType() { Name = "Created", Description = "" });
            var sourcePartyType = partyTypeManager.Create("sourceTestType", "", "", partyStatusTypes);
            var targetPartyType = partyTypeManager.Create("targetTestType", "", "", partyStatusTypes);
            //create relationshiptype and one "typePair"
            var partyRelationshipType = partyRelationshipTypeManager.Create("title", "T i t l e", "Descr", false, 100, -1, false, sourcePartyType, targetPartyType, "typepairTitle", "", "", "", 0);
            long partyRelationshipTypeId = partyRelationshipType.Id;
            partyRelationshipTypeManager.Update(partyRelationshipTypeId, "updatedTitle", "updated Title", "updatedDescription", true, 10, 5);
            PartyRelationshipType partyRelationshipTypeAfterUpdate = partyRelationshipTypeManager.PartyRelationshipTypeRepository.Get(cc => cc.Id == partyRelationshipTypeId).FirstOrDefault();
            partyRelationshipTypeAfterUpdate.Title.Should().BeEquivalentTo("updatedTitle");
            partyRelationshipTypeAfterUpdate.DisplayName.Should().BeEquivalentTo("updated Title");
            partyRelationshipTypeAfterUpdate.Description.Should().BeEquivalentTo("updatedDescription");
            partyRelationshipTypeAfterUpdate.IndicatesHierarchy.Should().Equals(true);
            partyRelationshipTypeAfterUpdate.MaxCardinality.Should().Equals(10);
            partyRelationshipTypeAfterUpdate.MinCardinality.Should().Equals(5);
            // cleanup the DB
            partyRelationshipTypeManager.Delete(partyRelationshipType);
            partyTypeManager.Delete(sourcePartyType);
            partyTypeManager.Delete(targetPartyType);
        }

        [Test()]         public void DeleteListOfPartyRelationshipTest()         {
            PartyRelationshipTypeManager partyRelationshipTypeManager = new PartyRelationshipTypeManager();
            PartyTypeManager partyTypeManager = new PartyTypeManager();
            var partyStatusTypes = new List<PartyStatusType>();
            partyStatusTypes.Add(new PartyStatusType() { Name = "Created", Description = "" });
            var sourcePartyType = partyTypeManager.Create("sourceTestType", "", "", partyStatusTypes);
            var targetPartyType = partyTypeManager.Create("targetTestType", "", "", partyStatusTypes);
            //create relationshiptype and one "typePair"
            var partyRelationshipType = partyRelationshipTypeManager.Create("title", "T i t l e", "Descr", false, 100, -1, false, sourcePartyType, targetPartyType, "typepairTitle", "", "", "", 0);
            long partyRelationshipTypeId = partyRelationshipType.Id;
            var partyRelationshipType2 = partyRelationshipTypeManager.Create("title2", "T i t l e2", "Descr", false, 100, -1, false, sourcePartyType, targetPartyType, "typepairTitle", "", "", "", 0);
            long partyRelationshipType2Id = partyRelationshipType2.Id;
            List<PartyRelationshipType> partyRelationshipTypes = new List<PartyRelationshipType>();
            partyRelationshipTypes.Add(partyRelationshipType);
            partyRelationshipTypes.Add(partyRelationshipType2);
            partyRelationshipTypeManager.Delete(partyRelationshipTypes);
            PartyRelationshipType partyRelationshipType1AfterDelete = partyRelationshipTypeManager.PartyRelationshipTypeRepository.Get(cc => cc.Id == partyRelationshipTypeId).FirstOrDefault();
            PartyRelationshipType partyRelationshipType2AfterDelete = partyRelationshipTypeManager.PartyRelationshipTypeRepository.Get(cc => cc.Id == partyRelationshipType2Id).FirstOrDefault();
            partyRelationshipType1AfterDelete.Should().BeNull();
            partyRelationshipType2AfterDelete.Should().BeNull();
        }

        [Test()]         public void GetAllPartyRelationshipTypesTest()
        {
            //Scenario: 1- There are three partyRelationshipType which 1 of them has a pair with target Type of "person" ,
            // 2- There are three partyRelationshipType which 1 of them has a pair with target Type of "person" , and one of them is with source type of "person"
            //
            PartyRelationshipTypeManager partyRelationshipTypeManager = new PartyRelationshipTypeManager();
            PartyTypeManager partyTypeManager = new PartyTypeManager();
            var partyStatusTypes = new List<PartyStatusType>();
            partyStatusTypes.Add(new PartyStatusType() { Name = "Created", Description = "" });
            var personPartyType = partyTypeManager.Create("personTestType", "", "", partyStatusTypes);
            var TargetPartyType = partyTypeManager.Create("secondTestType", "", "", partyStatusTypes);
            var thirdPartyType = partyTypeManager.Create("thirdTestType", "", "", partyStatusTypes);
            //create relationshiptype and one "typePair"
            var partyRelationshipType = partyRelationshipTypeManager.Create("title", "T i t l e", "Descr", false, 100, -1, false, personPartyType, TargetPartyType, "typepairTitle", "", "", "", 0);
            long partyRelationshipTypeId = partyRelationshipType.Id;
            var partyRelationshipTypePair2 = partyRelationshipTypeManager.AddPartyTypePair("typePair2", TargetPartyType, thirdPartyType, "pair description", true, partyRelationshipType, "", "", 0);
            var partyRelationshipTypePair2Id = partyRelationshipTypePair2.Id;
            //second party rel type
            var partyRelationshipType2 = partyRelationshipTypeManager.Create("title2", "T i t l e", "Descr", false, 100, -1, false, thirdPartyType,personPartyType, "typepairTitle2", "", "", "", 0);
            long partyRelationshipType2Id = partyRelationshipType.Id;
            //third party rel type
            var partyRelationshipType3 = partyRelationshipTypeManager.Create("title3", "T i t l e", "Descr", false, 100, -1, false, thirdPartyType, TargetPartyType, "typepairTitle3", "", "", "", 0);
            long partyRelationshipType3Id = partyRelationshipType3.Id;
            var partyRelationshipTypePair3 = partyRelationshipTypeManager.AddPartyTypePair("typePair4", thirdPartyType, TargetPartyType,  "pair description", true, partyRelationshipType, "", "", 0);
            // test
            //get all party relationshiptype which has a source type for "person" , it should be the second one
            var partyRelTypes=partyRelationshipTypeManager.GetAllPartyRelationshipTypes(personPartyType.Id);
            partyRelTypes.Count().Should().Be(1);
            partyRelTypes.First().Id.Should().Be(partyRelationshipType.Id);
            //get all party relationshiptype which has a target or source type for "person" , it should be the first and second one
            partyRelTypes = partyRelationshipTypeManager.GetAllPartyRelationshipTypes(personPartyType.Id,true);
            partyRelTypes.Count().Should().Be(2);
            partyRelTypes.Any(cc => cc.Id == partyRelationshipType2Id).Should().Be(true);
            partyRelTypes.Any(cc => cc.Id == partyRelationshipTypeId).Should().Be(true);
            partyRelTypes.Any(cc => cc.Id == partyRelationshipType3Id).Should().Be(false);
            //clean up DB
            partyRelationshipTypeManager.Delete(partyRelationshipType);
            partyRelationshipTypeManager.Delete(partyRelationshipType2);
            partyRelationshipTypeManager.Delete(partyRelationshipType3);
            partyTypeManager.Delete(personPartyType);
            partyTypeManager.Delete(TargetPartyType);
            partyTypeManager.Delete(thirdPartyType);
        }

        [Test()]
        public void UpdatePartyTypePair()
        {
            PartyRelationshipTypeManager partyRelationshipTypeManager = new PartyRelationshipTypeManager();
            PartyTypeManager partyTypeManager = new PartyTypeManager();
            var partyStatusTypes = new List<PartyStatusType>();
            partyStatusTypes.Add(new PartyStatusType() { Name = "Created", Description = "" });
            var personPartyType = partyTypeManager.Create("personTestType", "", "", partyStatusTypes);
            var TargetPartyType = partyTypeManager.Create("secondTestType", "", "", partyStatusTypes);
            var thirdPartyType = partyTypeManager.Create("thirdTestType", "", "", partyStatusTypes);
            var partyRelationshipType = partyRelationshipTypeManager.Create("title", "T i t l e", "Descr", false, 100, -1, false, personPartyType, TargetPartyType, "typepairTitle1", "", "", "", 0);
            var partyTypePairId = partyRelationshipTypeManager.PartyTypePairRepository.Get(cc=>cc.PartyRelationshipType.Id== partyRelationshipType.Id).First().Id;
            var partyRelationshipType2 = partyRelationshipTypeManager.Create("title2", "T i t l e2", "Descr", false, 100, -1, false, personPartyType, TargetPartyType, "typepairTitle2", "", "", "", 0);

            PartyTypePair typePair = partyRelationshipTypeManager.PartyTypePairRepository.Get(partyTypePairId);
            typePair.Title = "updatedTitle";
            typePair.SourcePartyType = thirdPartyType;
            typePair.TargetPartyType= personPartyType;
            typePair.Description="updatedDescription";
            typePair.ConditionSource= "no con source";
            typePair.ConditionTarget= "no con target";
            typePair.PartyRelationshipType= partyRelationshipType2;
            typePair.PermissionTemplate= 10;
            typePair.PartyRelationShipTypeDefault=true;
            partyRelationshipTypeManager.UpdatePartyTypePair(typePair);
            //partyRelationshipTypeManager.UpdatePartyTypePair(partyTypePairId, typePair.Title, typePair.SourcePartyType,typePair.TargetPartyType, typePair.Description,typePair.ConditionSource, typePair.ConditionTarget, typePair.PartyRelationShipTypeDefault, typePair.PartyRelationshipType, typePair.PermissionTemplate);
            // test partyRelationshipTypePair
            var fetchedpartyRelationshipTypePair = partyRelationshipTypeManager.PartyTypePairRepository.Get(partyTypePairId);
            fetchedpartyRelationshipTypePair.SourcePartyType.Id.Should().Equals(typePair.SourcePartyType.Id);
            fetchedpartyRelationshipTypePair.TargetPartyType.Id.Should().Equals(typePair.TargetPartyType.Id);
            fetchedpartyRelationshipTypePair.Description.Should().BeEquivalentTo(typePair.Description);
            fetchedpartyRelationshipTypePair.PartyRelationShipTypeDefault.Should().Equals(typePair.PartyRelationShipTypeDefault);
            fetchedpartyRelationshipTypePair.PermissionTemplate.Should().Equals(typePair.PermissionTemplate);
            fetchedpartyRelationshipTypePair.ConditionSource.Should().Equals(typePair.ConditionSource);
            fetchedpartyRelationshipTypePair.ConditionTarget.Should().Equals(typePair.ConditionTarget);
            fetchedpartyRelationshipTypePair.PartyRelationshipType.Should().Equals(typePair.PartyRelationshipType);
            fetchedpartyRelationshipTypePair.Title.Should().Equals(typePair.Title);
            //clean up DB
            partyRelationshipTypeManager.Delete(partyRelationshipType);
            partyRelationshipTypeManager.Delete(partyRelationshipType2);
            partyTypeManager.Delete(personPartyType);
            partyTypeManager.Delete(TargetPartyType);
            partyTypeManager.Delete(thirdPartyType);
        }

        [Test()]
        public void RemovePartyTypePairTest()
        {
            //Scenario: Adding 3 partyTypePair to a relationsiptype and then remove 1 of them seperately and the other two as a list
            PartyRelationshipTypeManager partyRelationshipTypeManager = new PartyRelationshipTypeManager();
            PartyTypeManager partyTypeManager = new PartyTypeManager();
            var partyStatusTypes = new List<PartyStatusType>();
            partyStatusTypes.Add(new PartyStatusType() { Name = "Created", Description = "" });
            var personPartyType = partyTypeManager.Create("personTestType", "", "", partyStatusTypes);
            var TargetPartyType = partyTypeManager.Create("secondTestType", "", "", partyStatusTypes);
            var thirdPartyType = partyTypeManager.Create("thirdTestType", "", "", partyStatusTypes);
            //create relationshiptype and one "typePair"
            var partyRelationshipType = partyRelationshipTypeManager.Create("title", "T i t l e", "Descr", false, 100, -1, false, personPartyType, TargetPartyType, "typepairTitle", "", "", "", 0);
            long partyRelationshipTypeId = partyRelationshipType.Id;
            var partyRelationshipTypePair = partyRelationshipTypeManager.PartyTypePairRepository.Get(cc => cc.PartyRelationshipType.Id ==partyRelationshipType.Id).First();
            var partyRelationshipTypePair2 = partyRelationshipTypeManager.AddPartyTypePair("typePair2", TargetPartyType, thirdPartyType, "pair description", true, partyRelationshipType, "", "", 0);
            var partyRelationshipTypePair2Id = partyRelationshipTypePair2.Id;
            var partyRelationshipTypePair3= partyRelationshipTypeManager.AddPartyTypePair("typePair2", TargetPartyType, thirdPartyType, "pair description", true, partyRelationshipType, "", "", 0);
            List<PartyTypePair> partyTypePairs = new List<PartyTypePair>();
            partyTypePairs.Add(partyRelationshipTypePair2);
            partyTypePairs.Add(partyRelationshipTypePair3);
            partyRelationshipTypeManager.RemovePartyTypePair(partyRelationshipTypePair);
            partyRelationshipTypeManager.RemovePartyTypePair(partyRelationshipTypePair2);
            partyRelationshipTypeManager.RemovePartyTypePair(partyRelationshipTypePair3);
            var partyTypePairsAfterDelete=partyRelationshipTypeManager.PartyTypePairRepository.Get(cc => cc.PartyRelationshipType.Id == partyRelationshipTypeId);
            partyTypePairsAfterDelete.Count().Should().Be(0);
        }


    }
}
