using System.Collections.Generic;
using Vaiona.Entities.Common;

namespace BExIS.Dlm.Entities.DataStructure
{
    public class Classifier : BaseEntity
    {
        public virtual string Name { get; set; }
        public virtual string Description { get; set; }

        public virtual Classifier Parent { get; set; }
        public virtual ICollection<Classifier> Children { get; set; }
        //public virtual string Lineage { get; set; }
        //public virtual string LineageId { get; set; } // needs more design!

    }
}
