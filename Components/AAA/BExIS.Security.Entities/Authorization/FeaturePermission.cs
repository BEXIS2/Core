using BExIS.Security.Entities.Objects;
using BExIS.Security.Entities.Subjects;
using BEXIS2.DB.Entities.Common;

namespace BExIS.Security.Entities.Authorization
{
    public enum PermissionType
    {
        Deny = 0,
        Grant = 1
    }

    public class FeaturePermission : BaseEntity
    {
        public virtual Feature Feature { get; set; }
        public virtual PermissionType PermissionType { get; set; }
        public virtual Subject Subject { get; set; }
    }
}