using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BExIS.Dlm.Entities.Data;
using Vaiona.Entities.Common;

namespace BExIS.Dlm.Entities.DataStructure
{
    public class DataView: BaseEntity
    {
        public virtual string Name { get; set; }
        public virtual string ContentSelectionCriterion { get; set; } // to select which tuples are visible trough the view
        public virtual string ContainerSelectionCriterion { get; set; } // to select which variables or parameters are visisble through the view

        public virtual Dataset Dataset { get; set; }
        public virtual ICollection<DataStructure> DataStructures { get; set; }
    }
}
