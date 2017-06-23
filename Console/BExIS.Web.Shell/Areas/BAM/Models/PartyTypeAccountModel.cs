using BExIS.Dlm.Entities.Party;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BExIS.Web.Shell.Areas.BAM.Models
{
    public class PartyTypeAccountModel
    {
        public PartyType PartyType { get; set; }
        public Dictionary<string,PartyTypePair> PartyRelationshipTypes { get; set; }
    }
}