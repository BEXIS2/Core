using Vaiona.Entities.Common;

namespace BExIS.Dim.Entities.Mapping
{
    public class TransformationRule : BaseEntity, IBusinessVersionedEntity
    {
        public virtual string RegEx { get; set; }

    }
}
