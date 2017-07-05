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
        public virtual Operation Child { get; set; }
    }
}