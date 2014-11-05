using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BExIS.Security.Entities.Objects;
using BExIS.Security.Entities.Subjects;
using Vaiona.Entities.Common;

namespace BExIS.Security.Entities.Authorization
{
    public abstract class Permission : BaseEntity
    {
        #region Attributes

        public virtual PermissionType PermissionType { get; set; }

        #endregion

        #region Associations

        public virtual Subject Subject { get; set; }

        #endregion
    }
}
