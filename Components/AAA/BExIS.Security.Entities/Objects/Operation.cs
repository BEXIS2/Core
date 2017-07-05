using System.Collections.Generic;
using Vaiona.Entities.Common;

namespace BExIS.Security.Entities.Objects
{
    public class Operation : BaseEntity
    {
        public virtual string Module { get; set; }
        public virtual string Controller { get; set; }
        public virtual string Action { get; set; }
        public virtual Workflow Workflow { get; set; }
        public virtual Operation Parent { get; set; }
        public virtual ICollection<Operation> Children { get; set; }

        public virtual ICollection<Operation> Ancestors
        {
            get
            {
                var ancestors = new List<Operation>();

                if (Parent == null) return ancestors;

                ancestors.Add(Parent);
                ancestors.AddRange(Parent.Ancestors);
                return ancestors;
            }
        }

        public Operation()
        {
            Children = new List<Operation>();
        }
    }
}