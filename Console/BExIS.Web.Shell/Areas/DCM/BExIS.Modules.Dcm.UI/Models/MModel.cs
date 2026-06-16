using BExIS.Dim.Entities.Mappings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BExIS.Modules.Dcm.UI.Models
{
    public class MModel
    {

    }

    public class PartyMappingResultModel
    { 
        public string  Path { get; set; }
        public string ParentPath { get; set; }
        public long LinkElementId { get; set; }

        public bool Selector { get; set; }
        public bool Complexity { get; set; }

        public List<MappingPartyResultElemenet> List { get; set; }

        public PartyMappingResultModel() { }
    }
}