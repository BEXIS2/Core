using BExIS.Dlm.Entities.Party;
using BExIS.Dlm.Services.Party;
using System;
using System.Linq;
using System.Text.RegularExpressions;

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
    }
}