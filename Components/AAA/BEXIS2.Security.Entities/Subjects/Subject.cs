using BEXIS2.Security.Entities.Authorization;
using System.Collections.Generic;
using BEXIS2.DB.Entities.Common;

namespace BEXIS2.Security.Entities.Subjects
{
    public abstract class Subject : BaseEntity
    {
        public virtual ICollection<EntityPermission> EntityPermissions { get; set; }
        public virtual ICollection<FeaturePermission> FeaturePermissions { get; set; }
        public virtual string Name { get; set; }
        public virtual string DisplayName { get; set; }
    }
}