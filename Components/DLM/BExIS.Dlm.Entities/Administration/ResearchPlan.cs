using System.Collections.Generic;
using BExIS.Dlm.Entities.Data;
using Vaiona.Entities.Common;

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
