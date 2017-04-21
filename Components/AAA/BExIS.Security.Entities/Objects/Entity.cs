using BExIS.Security.Entities.Authorization;
using System;
using System.Collections.Generic;
using Vaiona.Entities.Common;

namespace BExIS.Security.Entities.Objects
{
    public class Entity : BaseEntity
    {
        public virtual string AssemblyPath { get; set; }
        public virtual string ClassPath { get; set; }
        public virtual string Name { get; set; }
        public virtual ICollection<EntityPermission> Permissions { get; set; }

        // TODO: REMOVAL of obsolete properties
        [Obsolete]
        public virtual bool Securable { get; set; }

        [Obsolete]
        public virtual bool UseMetadata { get; set; }

        public Entity()
        {
            Permissions = new List<EntityPermission>();
        }
    }
}
