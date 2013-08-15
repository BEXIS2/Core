using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vaiona.Entities.Common;
using BExIS.Dlm.Entities.Data;

namespace BExIS.Dlm.Entities.Administration
{
    public class ResearchPlan: BaseEntity
    {
        public virtual string Title { get; set; }
        public virtual string Description { get; set; }

        public virtual ICollection<Dataset> Datasets { get; set; }
        public virtual ICollection<DataStructure.DataStructure> DataStructures { get; set; }
        // Accessibility Policy
        // Execution Unit
        // Metadata Structure
    }
}
