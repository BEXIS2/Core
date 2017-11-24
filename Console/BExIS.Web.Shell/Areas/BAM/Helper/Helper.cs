using BExIS.Dlm.Entities.Party;
using BExIS.Dlm.Services.Party;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using BExIS.Modules.Bam.UI.Models;
using System.Collections.Generic;

namespace BExIS.Modules.Bam.UI.Helpers
{
    public class Helper
    {
        public static int CountRelations(long sourcePartyId, PartyRelationshipType partyRelationshipType)
        {
            PartyManager partyManager = new PartyManager();
            var cnt = partyManager.PartyRelationshipRepository.Query(item => (item.PartyRelationshipType != null && item.PartyRelationshipType.Id == partyRelationshipType.Id)
                                      && (item.FirstParty != null && (item.FirstParty.Id == sourcePartyId) || (item.SecondParty.Id == sourcePartyId))
                                       && (item.EndDate >= DateTime.Now)).Count();
            return cnt;
        }

        public static string GetDisplayName(Dlm.Entities.Party.PartyRelationshipType partyRelatinshipType)
        {
            return (string.IsNullOrWhiteSpace(partyRelatinshipType.DisplayName) ? partyRelatinshipType.Title : partyRelatinshipType.DisplayName);
        }

        public static String ValidateRelationships(long partyId)
        {
            var partyManager = new PartyManager();
            var validations = partyManager.ValidateRelationships(partyId);
            string messages = "";
            foreach (var validation in validations)
            {
                messages += (String.Format("<br/>{0} relationship type '{1}'.", validation.Value, validation.Key.DisplayName));
            }
            if (!string.IsNullOrEmpty(messages))
                messages = "These relationship types are required : " + messages;
            return messages;
        }

        internal static Party EditParty(PartyModel partyModel, Dictionary<string, string> partyCustomAttributeValues)
        {
            PartyTypeManager partyTypeManager = new PartyTypeManager();
            PartyManager partyManager = new PartyManager();
            var party = new Party();
            try
            {
                var newAddPartyCustomAttrValues = new Dictionary<PartyCustomAttribute, string>();
                party = partyManager.Find(partyModel.Id);
                //Update some fields
                party.Description = partyModel.Description;
                party.StartDate = partyModel.StartDate.HasValue ? partyModel.StartDate.Value : DateTime.MinValue;
                party.EndDate = partyModel.EndDate.HasValue ? partyModel.EndDate.Value : DateTime.MaxValue;
                party = partyManager.Update(party);
                foreach (var partyCustomAttributeValueString in partyCustomAttributeValues)
                {
                    PartyCustomAttribute partyCustomAttribute = partyTypeManager.PartyCustomAttributeRepository.Get(int.Parse(partyCustomAttributeValueString.Key));
                    string value = string.IsNullOrEmpty(partyCustomAttributeValueString.Value) ? "" : partyCustomAttributeValueString.Value;
                    newAddPartyCustomAttrValues.Add(partyCustomAttribute, value);
                }
                partyManager.AddPartyCustomAttributeValues(party, partyCustomAttributeValues.ToDictionary(cc => long.Parse(cc.Key), cc => cc.Value));
            }
            finally
            {
                partyTypeManager?.Dispose();
                partyManager?.Dispose();
            }
            return party;
        }
        internal static Party CreateParty(PartyModel partyModel, Dictionary<string, string> partyCustomAttributeValues)
        {
            PartyTypeManager partyTypeManager = new PartyTypeManager();
            PartyManager partyManager = new PartyManager();
            var party = new Party();
            try
            {
                PartyType partyType = partyTypeManager.PartyTypeRepository.Get(partyModel.PartyType.Id);
                var partyStatusType = partyTypeManager.GetStatusType(partyType, "Created");
                // save party as temp if the reationships are required
                var requiredPartyRelationTypes = new PartyRelationshipTypeManager().GetAllPartyRelationshipTypes(partyType.Id).Where(cc => cc.MinCardinality > 0);
                //Create party
                party = partyManager.Create(partyType, "", partyModel.Description, partyModel.StartDate, partyModel.EndDate, partyStatusType, requiredPartyRelationTypes.Any());
                partyManager.AddPartyCustomAttributeValues(party, partyCustomAttributeValues.ToDictionary(cc => long.Parse(cc.Key), cc => cc.Value));
            }
            finally
            {
                partyTypeManager?.Dispose();
                partyManager?.Dispose();
            }
            return party;
        }
    }
}