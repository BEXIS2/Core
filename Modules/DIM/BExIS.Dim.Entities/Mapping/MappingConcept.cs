using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vaiona.Entities.Common;

namespace BExIS.Dim.Entities.Mapping
{
    public class MappingConcept
    {
        public virtual long Id { get; set; }
        public virtual int Version { get; set; }
        public virtual string Name { get; set; }
        public virtual string Description { get; set; }
        public virtual string Url { get; set; }


        public MappingConcept()
        {
            Name = string.Empty;
            Description = string.Empty;
            Url = string.Empty;
        }
    } 
}
