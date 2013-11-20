using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vaiona.Entities.Common;

namespace BExIS.Security.Entities
{
    public abstract class Permission : BaseEntity
    {
        #region Attributes

        public virtual PermissionType PermissionType { get; set; }

        #endregion


        #region Associations

        public virtual ICollection<Subject> Subjects { get; set; }

        #endregion


        #region Methods



        #endregion
    }

    public enum PermissionType
    {
        Deny,
        Allow
    }
}
