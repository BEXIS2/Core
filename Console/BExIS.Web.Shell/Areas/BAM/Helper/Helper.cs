using BExIS.Dlm.Entities.Party;
using BExIS.Dlm.Services.Party;
using NCalc;
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

        public static bool CheckCondition(String condition, long partyId)
        {
            if (string.IsNullOrEmpty(condition))
                return true;
            var partyManager = new PartyManager();
            var party = partyManager.PartyRepository.Get(partyId);
            if (party == null)
                return false;
            var newCondition = condition;
            //if text is sourounded with [] means the value comes from an element
            //Extract such text and replace them by the value of the related element
            MatchCollection matches = Regex.Matches(condition, @"\[(.*?)\]");
            foreach (Match match in matches)
            {
                var customAttributeName = match.Groups[1].Value;
                var customAttributeValue = party.CustomAttributeValues.FirstOrDefault(cc => cc.CustomAttribute.Name == customAttributeName);
                if (customAttributeValue == null)
                    throw new Exception(string.Format("There is not any custom attribute name which has {0} name.", customAttributeName));
                newCondition = newCondition.Replace(match.Groups[0].Value, string.Format("'{0}'",customAttributeValue.Value));
            }
            try
            {
                Expression e = new Expression(newCondition);
                return ((bool)e.Evaluate());
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }
    }
}