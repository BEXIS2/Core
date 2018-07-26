using BExIS.App.Testing;
using BExIS.Dlm.Entities.Party;
using BExIS.Dlm.Services.Party;
using BExIS.Utils.Config;
using FluentAssertions;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BExIS.Dlm.Tests.Services.Party
{
    public class PartyManagerTests
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
        public void CreatePartyTest()
        {
            //Scenario: create, create with CustomAttributeValues<Id,value>, create with CustomAttributeValues<Name,value>, 
            PartyManager partyManager = new PartyManager();
            PartyTypeManager partyTypeManager = new PartyTypeManager();
            var partyStatusTypes = new List<PartyStatusType>();
            partyStatusTypes.Add(new PartyStatusType() { Name = "Created", Description = "" });

            var partyTypeTest =partyTypeManager.Create("partyTypeTitle", "", "", partyStatusTypes);
            var partyStatusType = partyTypeManager.GetStatusType(partyTypeTest, "Created");
            var party1=partyManager.Create(partyTypeTest,"alias", "description test", DateTime.Now.AddMonths(-1), DateTime.Now.AddMonths(1), partyStatusType);
            party1.Should().NotBeNull();
            party1.Id.Should().BeGreaterThan(0);
            var party1Id = party1.Id;
            var fetchedParty = partyManager.PartyRepository.Get(party1Id);
            party1.Name.Should().BeEquivalentTo(fetchedParty.Name);
            party1.Alias.Should().BeEquivalentTo(fetchedParty.Alias);
            party1.Description.Should().BeEquivalentTo(fetchedParty.Description);
            party1.EndDate.ToShortDateString().Should().BeEquivalentTo(fetchedParty.EndDate.ToShortDateString());
            party1.PartyType.Id.Should().Be(fetchedParty.PartyType.Id);
            party1.StartDate.ToShortDateString().Should().BeEquivalentTo(fetchedParty.StartDate.ToShortDateString());
            party1.Id.Should().Be(fetchedParty.Id);
            //cleanup
            partyManager.Delete(party1);
            var partyAfterDelete = partyManager.PartyRepository.Get(cc => cc.Id == party1Id).FirstOrDefault();
            partyAfterDelete.Should().BeNull();
            //create cstom attributes
            var partyCustomAttribute1 = partyTypeManager.CreatePartyCustomAttribute(partyTypeTest, "String", "FirstName", "", "", "",isMain:true);
            var partyCustomAttribute2 = partyTypeManager.CreatePartyCustomAttribute(partyTypeTest, "String", "LastName", "", "", "", isMain: true);
            var partyCustomAttribute3 = partyTypeManager.CreatePartyCustomAttribute(partyTypeTest, "Int", "Age", "", "", "");
            //create with CustomAttributeValues<Id,value>
            Dictionary<long, String> customAttributeValues = new Dictionary<long, string>();
            customAttributeValues.Add(partyCustomAttribute1.Id,"Masoud");
            customAttributeValues.Add(partyCustomAttribute2.Id, "Allahyari");
            customAttributeValues.Add(partyCustomAttribute3.Id, "31");
            var party2 = partyManager.Create(partyTypeTest, "", null, null, customAttributeValues);
            var party2Id = party2.Id;
            var fetchedParty2 = partyManager.PartyRepository.Get(party2Id);
            //First name and lastname in custom attributes were defined as main;therefore, party name should be "name lastname"
            party2.Name.Should().BeEquivalentTo("Masoud Allahyari");
            party2.Alias.Should().BeEquivalentTo(fetchedParty2.Alias);
            party2.Description.Should().BeEquivalentTo(fetchedParty2.Description);
            party2.EndDate.ToShortDateString().Should().Be(fetchedParty2.EndDate.ToShortDateString());
            party2.PartyType.Id.Should().Be(fetchedParty2.PartyType.Id);
            party2.StartDate.ToShortDateString().Should().Be(fetchedParty2.StartDate.ToShortDateString());
            party2.Id.Should().Be(fetchedParty2.Id);
            var fethedCustomAttributeValues = fetchedParty2.CustomAttributeValues;
            fethedCustomAttributeValues.Count().Should().Be(customAttributeValues.Count());

            fethedCustomAttributeValues.Any(cc => cc.Value == "Masoud" && cc.CustomAttribute.Id== partyCustomAttribute1.Id).Should().Be(true);
            fethedCustomAttributeValues.Any(cc => cc.Value == "Allahyari" && cc.CustomAttribute.Id == partyCustomAttribute2.Id).Should().Be(true);
            fethedCustomAttributeValues.Any(cc => cc.Value == "31" && cc.CustomAttribute.Id == partyCustomAttribute3.Id).Should().Be(true);
            fethedCustomAttributeValues.Any(cc => cc.Value == "30" && cc.CustomAttribute.Id == partyCustomAttribute3.Id).Should().Be(false);
            //Cleanup DB
            partyManager.Delete(party2);
            //custom Attribute values should have benn deleted 
            var customAttrValues = partyManager.PartyCustomAttributeValueRepository.Get(cc => cc.Party.Id == party2Id);
            customAttrValues.Count().Should().Be(0);
            //create with CustomAttributeValues<Name,value>
            Dictionary<String, String> customAttributeValues2 = new Dictionary<String, string>();
            customAttributeValues2.Add(partyCustomAttribute1.Name, "Alex");
            customAttributeValues2.Add(partyCustomAttribute2.Name, "Abedini");
            customAttributeValues2.Add(partyCustomAttribute3.Name, "31");
            var party3 = partyManager.Create(partyTypeTest, "", null, null, customAttributeValues2);
            var party3Id = party3.Id;
            var fetchedParty3 = partyManager.PartyRepository.Get(party3Id);
            party3.Name.Should().BeEquivalentTo("Alex Abedini");
            party3.Alias.Should().BeEquivalentTo(fetchedParty3.Alias);
            party3.Description.Should().BeEquivalentTo(fetchedParty3.Description);           
            party3.EndDate.ToShortDateString().Should().BeEquivalentTo(fetchedParty3.EndDate.ToShortDateString());
            party3.PartyType.Id.Should().Be(fetchedParty3.PartyType.Id);
            party3.StartDate.ToShortDateString().Should().BeEquivalentTo(fetchedParty3.StartDate.ToShortDateString());
            party3.Id.Should().Be(fetchedParty3.Id);
            fethedCustomAttributeValues = fetchedParty3.CustomAttributeValues;
            fethedCustomAttributeValues.Count().Should().Be(customAttributeValues2.Count());
            fethedCustomAttributeValues.Any(cc => cc.Value == "Alex" && cc.CustomAttribute.Id == partyCustomAttribute1.Id).Should().Be(true);
            fethedCustomAttributeValues.Any(cc => cc.Value == "Abedini" && cc.CustomAttribute.Id == partyCustomAttribute2.Id).Should().Be(true);
            fethedCustomAttributeValues.Any(cc => cc.Value == "31" && cc.CustomAttribute.Id == partyCustomAttribute3.Id).Should().Be(true);
            fethedCustomAttributeValues.Any(cc => cc.Value == "30" && cc.CustomAttribute.Id == partyCustomAttribute3.Id).Should().Be(false);
            //Cleanup DB
            partyManager.Delete(party3);
            partyTypeManager.Delete(partyTypeTest);
        }

        [Test()]
        public void DeletePartyTest()
        {
            //Deleting single party is already tested in PartyCreation
            PartyManager partyManager = new PartyManager();
            PartyTypeManager partyTypeManager = new PartyTypeManager();
            var partyStatusTypes = new List<PartyStatusType>();
            partyStatusTypes.Add(new PartyStatusType() { Name = "Created", Description = "" });

            var partyTypeTest = partyTypeManager.Create("partyTypeTitle", "", "", partyStatusTypes);
            var partyStatusType = partyTypeManager.GetStatusType(partyTypeTest, "Created");
            var party1 = partyManager.Create(partyTypeTest, "alias", "description test", DateTime.Now.AddMonths(-1), DateTime.Now.AddMonths(1), partyStatusType);
            var party2 = partyManager.Create(partyTypeTest, "alias2", "description test2", DateTime.Now.AddMonths(-1), DateTime.Now.AddMonths(1), partyStatusType);
            List<Dlm.Entities.Party.Party> parties = new List<Dlm.Entities.Party.Party>();
            parties.Add(party1);
            parties.Add(party2);
            partyManager.Delete(parties);
            var party1AfterDelete = partyManager.PartyRepository.Get(party1.Id);
            var party2AfterDelete = partyManager.PartyRepository.Get(party2.Id);
            party1AfterDelete.Should().BeNull();
            party2AfterDelete.Should().BeNull();
            partyTypeManager.Delete(partyTypeTest);
            
        }
        [Test()]
        public void UpdatePartyTest()
        {
            //Deleting single party is already tested in PartyCreation
            PartyManager partyManager = new PartyManager();
            PartyTypeManager partyTypeManager = new PartyTypeManager();
            var partyStatusTypes = new List<PartyStatusType>();
            partyStatusTypes.Add(new PartyStatusType() { Name = "Created", Description = "" });
            var partyTypeTest = partyTypeManager.Create("partyTypeTitle", "", "", partyStatusTypes);
            var partyStatusType = partyTypeManager.GetStatusType(partyTypeTest, "Created");
            //create cstom attributes
            var partyCustomAttribute1 = partyTypeManager.CreatePartyCustomAttribute(partyTypeTest, "String", "FirstName", "", "", "", isMain: true);
            var partyCustomAttribute2 = partyTypeManager.CreatePartyCustomAttribute(partyTypeTest, "String", "LastName", "", "", "", isMain: true);
            var partyCustomAttribute3 = partyTypeManager.CreatePartyCustomAttribute(partyTypeTest, "Int", "Age", "", "", "");
            //create with CustomAttributeValues<Id,value>
            Dictionary<long, String> customAttributeValues = new Dictionary<long, string>();
            customAttributeValues.Add(partyCustomAttribute1.Id, "Masoud");
            customAttributeValues.Add(partyCustomAttribute2.Id, "Allahyari");
            customAttributeValues.Add(partyCustomAttribute3.Id, "31");
            var party = partyManager.Create(partyTypeTest,  "description test",DateTime.Now.AddDays(-10),DateTime.Now.AddDays(10), customAttributeValues);
            var updatedParty = partyManager.PartyRepository.Get(party.Id);
            updatedParty.Alias = "alias2";
            updatedParty.Description = "desc";
            updatedParty.EndDate = DateTime.Now.AddMonths(1);
            updatedParty.StartDate = DateTime.Now.AddMonths(-1);
            partyManager.Update(updatedParty);
            var fetchedParty = partyManager.PartyRepository.Get(party.Id);
            //First name and lastname in custom attributes were defined as main;therefore, party name should be "name lastname"
            party.Name.Should().BeEquivalentTo("Masoud Allahyari");
            party.Alias.Should().BeEquivalentTo(fetchedParty.Alias);
            party.Description.Should().BeEquivalentTo(fetchedParty.Description);
            party.EndDate.ToShortDateString().Should().Be(fetchedParty.EndDate.ToShortDateString());
            party.PartyType.Id.Should().Be(fetchedParty.PartyType.Id);
            party.StartDate.ToShortDateString().Should().Be(fetchedParty.StartDate.ToShortDateString());
            party.Id.Should().Be(fetchedParty.Id);
         
            partyManager.Delete(party);
            partyTypeManager.Delete(partyTypeTest);
        }
        [Test()]
        public void TempPartyToPermanentTest() { }
        public void AddPartyRelationshipTest() {
            //add relationship 
            //test maximun and minimum cardinality
        }
        public void UpdatePartyRelationshipTest()
        {
        }
        public void RemovePartyRelationshipTest()
        {// alone and list
        }
        public void AddPartyCustomAttributeValueTest() {
            //test one, test dict<id,value>, test dict<object,value>
        }
        public void UpdatePartyCustomAttributeValueTest()
        {
            //test one, test list
        }
        public void RemovePartyCustomAttributeValueTest()
        {
            //test one, test list
        }

        public void GetPartyByCustomAttributeValuesTest()
        {
            //Scenario: add 3 parties having very similar custom attributes but different in one, get one of them calling this method
        }
        public void AddPartyUserTest()
        {
            //add a user to party and check the existance
        }

        public void RemovePartyUserTest()
        {
        }

        public void GetPartyByUserTest()
        { }

        public void GetUserIdByPartyTest()
        {

        }

        public void UpdateOrAddPartyGridCustomColumnTest() { }
        public void CheckUniquenessTest()
        {
            //3 methods
        }
        public void ValidateRelationshipsTest()
        { }
        public void CheckConditionTest() { }
        public void GetPartyCustomGridColumnsTest() { }

    }
}
