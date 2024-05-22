using Vaiona.Entities.Common;

namespace BExIS.Dim.Entities.Publication
{
    public class Broker : BaseEntity, IBusinessVersionedEntity
    {
        public virtual string Name { get; set; }
        public virtual string Server { get; set; }
        public virtual string Type { get; set; }
        public virtual string UserName { get; set; }
        public virtual string Password { get; set; }
        public virtual string MetadataFormat { get; set; }
        public virtual string PrimaryDataFormat { get; set; }
        public virtual string Link { get; set; }
        public virtual Repository Repository { get; set; }

    }
}
