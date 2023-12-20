using Newtonsoft.Json;
using System.Collections.Generic;

namespace BExIS.Modules.Bam.UI.Configurations
{
    public class AccountPartyRelationshipType
    {
        public AccountPartyRelationshipType()
        {
            PartyRelationshipTypes = new List<string>();
        }

        [JsonProperty("partyRelationshipTypes")]
        public List<string> PartyRelationshipTypes { get; set; }

        [JsonProperty("partyType")]
        public string PartyType { get; set; }
    }
}