using Vaiona.Entities.Common;

namespace BExIS.Dim.Entities.Publication
{
    public class MetadataStructureToRepository : BaseEntity, IBusinessVersionedEntity
    {
        public virtual long MetadataStructureId { get; set; }
        public virtual long RepositoryId { get; set; }
    }

}
