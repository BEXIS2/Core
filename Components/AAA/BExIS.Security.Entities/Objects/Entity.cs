using BExIS.Security.Entities.Authorization;
using System;
using System.Collections.Generic;
using Vaiona.Entities.Common;

namespace BExIS.Security.Entities.Objects
{
    public class Entity : BaseEntity
    {
        public Entity()
        {
            Permissions = new List<EntityPermission>();
            Children = new List<Entity>();
        }

        public virtual ICollection<Entity> Ancestors
        {
            get
            {
                var ancestors = new List<Entity>();

                if (Parent == null) return ancestors;

                ancestors.Add(Parent);
                ancestors.AddRange(Parent.Ancestors);
                return ancestors;
            }
        }

        public virtual string AssemblyPath { get; set; }
        public virtual ICollection<Entity> Children { get; set; }
        public virtual string ClassPath { get; set; }
        public virtual string Name { get; set; }
        public virtual Entity Parent { get; set; }
        public virtual ICollection<EntityPermission> Permissions { get; set; }

        // TODO: REMOVAL of obsolete properties
        [Obsolete]
        public virtual bool Securable { get; set; }

        [Obsolete]
        public virtual bool UseMetadata { get; set; }
    }
}