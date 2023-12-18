using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vaiona.Entities.Common;

namespace BExIS.Dlm.Entities.Meanings
{
    public class MeaningEntry : BaseEntity
    {
     
        public virtual ExternalLink MappingRelation { get; set; }
        public virtual IList<ExternalLink> MappedLinks { get; set; }

        public MeaningEntry(){
            this.MappingRelation = new ExternalLink();
            this.MappedLinks = new List<ExternalLink>();
        }
        public MeaningEntry(MeaningEntry meaning)
        {
            this.MappingRelation = meaning.MappingRelation;
            this.MappedLinks = meaning.MappedLinks;
        }

        public MeaningEntry( ExternalLink mapping_relation, IList<ExternalLink> MappedLinks)
        {
            this.MappingRelation = mapping_relation;
            this.MappedLinks = MappedLinks;
        }
    }
}
