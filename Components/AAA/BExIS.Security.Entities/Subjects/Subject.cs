using BExIS.Security.Entities.Authorization;
using System.Collections.Generic;
using Vaiona.Entities.Common;

namespace BExIS.Security.Entities.Subjects
{
    public abstract class Subject : BaseEntity
    {
        public virtual ICollection<EntityPermission> EntityPermissions { get; set; }
        public virtual ICollection<FeaturePermission> FeaturePermissions { get; set; }
        public virtual string Name { get; set; }
    }
}