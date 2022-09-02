using BExIS.Dlm.Entities.Party;
using System.Collections.Generic;

namespace BExIS.Modules.Bam.UI.Models
{
    public class PartyTypeAccountModel
    {
        public Dictionary<PartyType, Dictionary<string, PartyTypePair>> PartyRelationshipsTypes = new Dictionary<PartyType, Dictionary<string, PartyTypePair>>();
        public Party Party { get; set; }
    }
}