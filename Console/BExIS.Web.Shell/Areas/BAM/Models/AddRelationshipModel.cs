using BExIS.Dlm.Entities.Party;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BExIS.Modules.Bam.UI.Models
{
    public class AddRelationshipModel
    {
        public IEnumerable<PartyRelationshipType> PartyRelationshipTypes { get; set; }
        public Party SourceParty { get; set; }
        public bool isAsSource { get; set; }
    }
}