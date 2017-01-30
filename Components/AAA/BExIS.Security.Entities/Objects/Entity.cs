using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BExIS.Security.Entities.Authorization;
using Vaiona.Entities.Common;

namespace BExIS.Security.Entities.Objects
{
    public class Entity : BaseEntity
    {
        #region Attributes

        public virtual string Name { get; set; }

        public virtual string AssemblyPath { get; set; }
        public virtual string ClassPath { get; set; }

        public virtual bool Securable { get; set; }
        public virtual bool UseMetadata { get; set; }

        #endregion

        #region Associations

        public virtual ICollection<DataPermission> DataPermissions { get; set; }

        #endregion

        #region Methods

        public Entity()
        {
            DataPermissions = new List<DataPermission>();
        }

        #endregion
    }
}
