using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BExIS.Security.Entities.Subjects;
using Vaiona.Entities.Common;

namespace BExIS.Security.Entities.Objects
{
    public abstract class Permission : BaseEntity
    {
        public virtual Subject Subject { get; set; }

        public virtual PermissionType PermissionType { get; set; }
    }

    public enum PermissionType
    {
        Deny,
        Allow
    }
}
