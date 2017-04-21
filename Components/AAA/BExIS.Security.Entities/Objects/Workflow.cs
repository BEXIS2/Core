using Vaiona.Entities.Common;

namespace BExIS.Security.Entities.Objects
{
    public class Workflow : BaseEntity
    {
        public virtual string Description { get; set; }
        public virtual string Name { get; set; }
        public virtual Feature Feature { get; set; }
    }
}
