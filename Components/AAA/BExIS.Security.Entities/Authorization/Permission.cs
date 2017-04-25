using BExIS.Security.Entities.Subjects;
using Vaiona.Entities.Common;

namespace BExIS.Security.Entities.Authorization
{
    public enum PermissionType
    {
        Deny = 0,
        Grant = 1
    }

    public enum RightType
    {
        Read = 0,
        Write = 1,
        Delete = 2,
        Grant = 3
    }

    public abstract class Permission : BaseEntity
    {
        public virtual PermissionType PermissionType { get; set; }
        public virtual short Rights { get; set; }
        public virtual Subject Subject { get; set; }
    }
}