using BExIS.App.Testing; using BExIS.Dlm.Entities.Party; using BExIS.Dlm.Services.Party; using BExIS.Utils.Config; using FluentAssertions; using NUnit.Framework; using System; using System.Collections.Generic; using System.Linq; using System.Text; using System.Threading.Tasks;  namespace BExIS.Dlm.Tests.Services.Party {     public class PartyTypeManagerTests     {         private TestSetupHelper helper = null;         [OneTimeSetUp]         public void OneTimeSetUp()         {             helper = new TestSetupHelper(WebApiConfig.Register, false);         }          [OneTimeTearDown]         public void OneTimeTearDown()         {             helper.Dispose();         }          [Test()]         public void CreatePartyTypeTest()         {             PartyTypeManager partyTypeManager = new PartyTypeManager();             try
            {
                var partyStatusTypes = new List<PartyStatusType>();
                partyStatusTypes.Add(new PartyStatusType() { Name = "Created", Description = "" });
                var partyType = partyTypeManager.Create("partyTypetest", "test", "party type test", partyStatusTypes);
                long partyTypeId = partyType.Id;
                partyType.Should().NotBeNull();
                partyType.Id.Should().BeGreaterThan(0);
                var fetchedPartyType = partyTypeManager.PartyTypeRepository.Get(partyType.Id);
                partyType.Title.Should().BeEquivalentTo(fetchedPartyType.Title);
                partyType.DisplayName.Should().BeEquivalentTo(fetchedPartyType.DisplayName);
                partyType.Description.Should().BeEquivalentTo(fetchedPartyType.Description);
                partyTypeManager.Delete(partyType);// cleanup the DB                 var partyTypeAfterDelete = partyTypeManager.PartyTypeRepository.Get(cc => cc.Id == partyTypeId).FirstOrDefault();
                partyTypeAfterDelete.Should().BeNull();
            }             finally             {
                partyTypeManager.Dispose();
            }         }          [Test()]         public void CreateCustomAttributeTest()         {             PartyTypeManager partyTypeManager = new PartyTypeManager();             try
            {
                var partyStatusTypes = new List<PartyStatusType>();
                partyStatusTypes.Add(new PartyStatusType() { Name = "Created", Description = "" });
                var partyType = partyTypeManager.Create("partyTypetest", "test", "party type test", partyStatusTypes);
                long partyTypeId = partyType.Id;
                partyType.Should().NotBeNull();
                var partyCustomAttribute = partyTypeManager.CreatePartyCustomAttribute(partyType, "string", "Name", "nothing", "validVals", "noCondition", false, false, true);
                long partyCustomAttributeId = partyCustomAttribute.Id;
                partyCustomAttribute.Should().NotBeNull();
                partyCustomAttribute.Id.Should().BeGreaterThan(0);
                var fetchedPartyCustomAttribute = partyTypeManager.PartyCustomAttributeRepository.Get(partyCustomAttribute.Id);
                partyCustomAttribute.Condition.Should().BeEquivalentTo(fetchedPartyCustomAttribute.Condition);
                partyCustomAttribute.DataType.Should().BeEquivalentTo(fetchedPartyCustomAttribute.DataType);
                partyCustomAttribute.DisplayName.Should().BeEquivalentTo(fetchedPartyCustomAttribute.DisplayName);
                partyCustomAttribute.Description.Should().BeEquivalentTo(fetchedPartyCustomAttribute.Description);
                partyCustomAttribute.DisplayOrder.Should().Equals(fetchedPartyCustomAttribute.DisplayOrder);
                partyCustomAttribute.IsMain.Should().Equals(fetchedPartyCustomAttribute.IsMain);
                partyCustomAttribute.IsUnique.Should().Equals(fetchedPartyCustomAttribute.IsUnique);
                partyCustomAttribute.DisplayOrder.Should().Equals(fetchedPartyCustomAttribute.DisplayOrder);
                partyTypeManager.DeletePartyCustomAttribute(partyCustomAttribute);// cleanup the DB                 var partyCustomAttributeAfterDelete = partyTypeManager.PartyCustomAttributeRepository.Get(cc => cc.Id == partyCustomAttributeId).FirstOrDefault();
                partyCustomAttributeAfterDelete.Should().BeNull();
                var objPartyCustomAttribute = new PartyCustomAttribute()
                {
                    Condition = "condition",
                    DataType = "datatype",
                    Description = "description"
                    ,
                    DisplayName = "displayname",
                    DisplayOrder = 1,
                    IsMain = false,
                    IsUnique = false,
                    IsValueOptional = false,
                    Name = "name",
                    PartyType = partyType,
                    ValidValues = "someVals"
                };
                var partyCustomAttributeFromObject = partyTypeManager.CreatePartyCustomAttribute(objPartyCustomAttribute);
                var fetchedPartyCustomAttributeFromObject = partyTypeManager.PartyCustomAttributeRepository.Get(partyCustomAttributeFromObject.Id);
                partyCustomAttributeFromObject.Condition.Should().BeEquivalentTo(fetchedPartyCustomAttributeFromObject.Condition);
                partyCustomAttributeFromObject.DataType.Should().BeEquivalentTo(fetchedPartyCustomAttributeFromObject.DataType);
                partyCustomAttributeFromObject.DisplayName.Should().BeEquivalentTo(fetchedPartyCustomAttributeFromObject.DisplayName);
                partyCustomAttributeFromObject.Description.Should().BeEquivalentTo(fetchedPartyCustomAttributeFromObject.Description);
                partyCustomAttributeFromObject.DisplayOrder.Should().Equals(fetchedPartyCustomAttributeFromObject.DisplayOrder);
                partyCustomAttributeFromObject.IsMain.Should().Equals(fetchedPartyCustomAttributeFromObject.IsMain);
                partyCustomAttributeFromObject.IsUnique.Should().Equals(fetchedPartyCustomAttributeFromObject.IsUnique);
                partyCustomAttributeFromObject.DisplayOrder.Should().Equals(fetchedPartyCustomAttributeFromObject.DisplayOrder);
                partyTypeManager.DeletePartyCustomAttribute(partyCustomAttributeFromObject);// cleanup the DB                 partyTypeManager.Delete(partyType);//                  var partyTypeAfterDelete = partyTypeManager.PartyTypeRepository.Get(cc => cc.Id == partyTypeId).FirstOrDefault();
                partyTypeAfterDelete.Should().BeNull();
            }             finally             {
                partyTypeManager.Dispose();
            }         }          [Test()]         public void DeleteListOfPartyTypeTest()         {             PartyTypeManager partyTypeManager = new PartyTypeManager();             try
            {
                var partyStatusTypes = new List<PartyStatusType>();
                partyStatusTypes.Add(new PartyStatusType() { Name = "Created", Description = "" });
                var partyType = partyTypeManager.Create("partyTypetest", "test", "party type test", partyStatusTypes);
                var partyType2 = partyTypeManager.Create("partyTypetest2", "test2", "party type test2", partyStatusTypes);
                var partyType1Id = partyType.Id;
                var partyType2Id = partyType2.Id;
                List<PartyType> partyTypes = new List<PartyType>();
                partyTypes.Add(partyType);
                partyTypes.Add(partyType2);
                partyTypeManager.Delete(partyTypes);
                var partyType1AfterDelete = partyTypeManager.PartyTypeRepository.Get(cc => cc.Id == partyType1Id).FirstOrDefault();
                var partyType2AfterDelete = partyTypeManager.PartyTypeRepository.Get(cc => cc.Id == partyType2Id).FirstOrDefault();
                partyType1AfterDelete.Should().BeNull();
                partyType2AfterDelete.Should().BeNull();
            }             finally             {
                partyTypeManager.Dispose();
            }         }
        [Test()]         public void DeleteListOfPartyCustomAttributeTest()         {             PartyTypeManager partyTypeManager = new PartyTypeManager();             try
            {
                var partyStatusTypes = new List<PartyStatusType>();
                partyStatusTypes.Add(new PartyStatusType() { Name = "Created", Description = "" });
                var partyType = partyTypeManager.Create("partyTypetest", "test", "party type test", partyStatusTypes);

                var partyCustomAttr1 = partyTypeManager.CreatePartyCustomAttribute(partyType, "test", "name", "", "", "");
                var partyCustomAttr2 = partyTypeManager.CreatePartyCustomAttribute(partyType, "test2", "name2", "", "", "");

                //cleanup
                var partyCustomAttr1Id = partyCustomAttr1.Id;
                var partyCustomAttr2Id = partyCustomAttr2.Id;
                List<PartyCustomAttribute> partyCustomAttributes = new List<PartyCustomAttribute>();
                partyCustomAttributes.Add(partyCustomAttr1);
                partyCustomAttributes.Add(partyCustomAttr2);
                partyTypeManager.DeletePartyCustomAttribute(partyCustomAttributes);
                partyTypeManager.Delete(partyType);

                //check if it is deleted
                var partyCustomAttr1AfterDelete = partyTypeManager.PartyTypeRepository.Query(cc => cc.Id == partyCustomAttr1Id).FirstOrDefault();
                var partyCustomAttr2AfterDelete = partyTypeManager.PartyTypeRepository.Query(cc => cc.Id == partyCustomAttr2Id).FirstOrDefault();
                partyCustomAttr1AfterDelete.Should().BeNull();
                partyCustomAttr2AfterDelete.Should().BeNull();
            }             finally             {
                partyTypeManager.Dispose();
            }         }          [Test()]
        public void UpdateCustomAttributeTest()         {             PartyTypeManager partyTypeManager = new PartyTypeManager();
            try
            {
                var partyStatusTypes = new List<PartyStatusType>();
                partyStatusTypes.Add(new PartyStatusType() { Name = "Created", Description = "" });
                var partyType = partyTypeManager.Create("partyTypetest", "test", "party type test", partyStatusTypes);
                var partyType2 = partyTypeManager.Create("partyTypetest2", "test2", "party type test", partyStatusTypes);
                var partyCustomAttr1 = partyTypeManager.CreatePartyCustomAttribute(partyType, "test", "name", "", "", "", false, true, true, 1);
                partyCustomAttr1.DataType = "test3";
                partyCustomAttr1.Name = "otherName";
                partyCustomAttr1.Description = "desc";
                partyCustomAttr1.ValidValues = "test3";
                partyCustomAttr1.DisplayName = "test 3";
                partyCustomAttr1.DisplayOrder = 2;
                partyCustomAttr1.IsMain = true;
                partyCustomAttr1.IsUnique = true;
                partyCustomAttr1.IsValueOptional = false;
                partyCustomAttr1.PartyType = partyType2;
                partyTypeManager.UpdatePartyCustomAttribute(partyCustomAttr1);
                var updatedPartyCustomAttr = partyTypeManager.PartyCustomAttributeRepository.Get(partyCustomAttr1.Id);
                partyCustomAttr1.Condition.Should().BeEquivalentTo(updatedPartyCustomAttr.Condition);
                partyCustomAttr1.DataType.Should().BeEquivalentTo(updatedPartyCustomAttr.DataType);
                partyCustomAttr1.DisplayName.Should().BeEquivalentTo(updatedPartyCustomAttr.DisplayName);
                partyCustomAttr1.Description.Should().BeEquivalentTo(updatedPartyCustomAttr.Description);
                partyCustomAttr1.DisplayOrder.Should().Equals(updatedPartyCustomAttr.DisplayOrder);
                partyCustomAttr1.IsMain.Should().Equals(updatedPartyCustomAttr.IsMain);
                partyCustomAttr1.IsUnique.Should().Equals(updatedPartyCustomAttr.IsUnique);
                partyCustomAttr1.DisplayOrder.Should().Equals(updatedPartyCustomAttr.DisplayOrder);
                partyTypeManager.DeletePartyCustomAttribute(updatedPartyCustomAttr);
                partyTypeManager.Delete(partyType);
            }
            finally
            {
                partyTypeManager.Dispose();
            }
        }          }  } 