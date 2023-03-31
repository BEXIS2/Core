using BExIS.Dlm.Entities.Party;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BExIS.Modules.Bam.UI.Models
{
    public class ReadPartyRelationshipTypePairModel
    {
        public long SourcePartyTypeId { get; set; }
        public long TargetPartyTypeId { get; set; }
    
    
        public static ReadPartyRelationshipTypePairModel Convert(PartyTypePair partyTypePair)
        {
            return new ReadPartyRelationshipTypePairModel
            {
                SourcePartyTypeId = partyTypePair.SourcePartyType.Id,
                TargetPartyTypeId = partyTypePair.TargetPartyType.Id
            };
        }
    }
}