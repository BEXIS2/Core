using Vaiona.Entities.Common;

namespace BExIS.Security.Entities.Objects
{
    public class Operation : BaseEntity
    {
        public virtual int Position { get; set; }
        public virtual string Module { get; set; }
        public virtual string Controller { get; set; }
        public virtual string Action { get; set; }
        public virtual Workflow Workflow { get; set; }
    }
}