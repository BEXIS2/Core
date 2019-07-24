using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BExIS.Dim.Entities.Mapping
{
    public class MappingPartyResultElemenet
    {
        public string Value { get; set; }
        public long PartyId { get; set; }
    }

    public class MappingEntityResultElement
    {
        public string Value { get; set; }
        public long EntityId { get; set; }
        public string Url { get; set; }
    }
}