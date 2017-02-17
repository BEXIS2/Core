using Vaiona.Entities.Common;

namespace BExIS.Dim.Entities.Mapping
{
    public class Mapping : BaseEntity, IBusinessVersionedEntity
    {
        public virtual LinkElement Source { get; set; }
        public virtual LinkElement Target { get; set; }
        public virtual TransformationRule TransformationRule { get; set; }
    }
}
