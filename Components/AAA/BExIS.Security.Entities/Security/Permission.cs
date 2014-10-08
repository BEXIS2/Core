using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vaiona.Entities.Common;

namespace BExIS.Security.Entities.Security
{
    public abstract class Permission : BaseEntity
    {
        public virtual PermissionType PermissionType { get; set; }
    }

    public enum PermissionType
    {
        Deny = 0,
        Grant = 1
    }
}
