using BExIS.Dlm.Entities.Party;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BExIS.Modules.Bam.UI.Models
{
    public class ReadPartyRelationshipModel
    {
        public long Id { get; set; }

        public static ReadPartyRelationshipModel Convert(PartyRelationship partyRelationship)
        {
            var model = new ReadPartyRelationshipModel
            {
                Id = partyRelationship.Id
            };

            return model;
        }
    }
}