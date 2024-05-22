using Vaiona.Entities.Common;

namespace BExIS.Dim.Entities.Publication
{
    public class Repository : BaseEntity, IBusinessVersionedEntity
    {
        //public virtual Broker Broker { get; set; }
        public virtual string Name { get; set; }
        public virtual string Url { get; set; }

    }
}
