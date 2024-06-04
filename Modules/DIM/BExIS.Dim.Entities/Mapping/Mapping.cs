using Vaiona.Entities.Common;

namespace BExIS.Dim.Entities.Mapping
{
    public class Mapping : BaseEntity, IBusinessVersionedEntity
    {
        public virtual long Level { get; set; }
        public virtual LinkElement Source { get; set; }
        public virtual LinkElement Target { get; set; }
        public virtual TransformationRule TransformationRule { get; set; }

        public virtual Mapping Parent { get; set; }

        public Mapping()
        {
            Level = 0;
            Source = new LinkElement();
            Target = new LinkElement();
            TransformationRule = new TransformationRule();
        }
    }
}