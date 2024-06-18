namespace Vaiona.Entities.Common
{
    public interface IAuditableEntity
    {
        EntityAuditInfo CreationInfo { get; set; }
        EntityAuditInfo ModificationInfo { get; set; }
    }
}