using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vaiona.Entities.Common;

namespace BExIS.Dim.Entities.Mappings
{
    public class MappingKey
    {
        public virtual long Id { get; set; }
        public virtual string Name { get; set; }
        public virtual string Description { get; set; }
        public virtual string Url { get; set; }
        public virtual bool Optional { get; set; }
        public virtual bool IsComplex { get; set; }
        public virtual MappingKey Parent { get; set; }
        public virtual MappingConcept Concept { get; set; }
        public virtual ICollection<MappingKey> Children { get; set; }

        public virtual string XPath { get; set; }


        public MappingKey()
        {
            Name = string.Empty;
            Description = string.Empty;
            Url = string.Empty;
            Optional = false;
            IsComplex = false;
            Children = new List<MappingKey>();
            XPath = string.Empty;
        }

    } 
}
