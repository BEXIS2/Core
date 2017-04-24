using BExIS.Security.Entities.Authorization;
using System.Collections.Generic;
using Vaiona.Entities.Common;

namespace BExIS.Security.Entities.Subjects
{
    public abstract class Subject : BaseEntity
    {
        public Subject()
        {
            Permissions = new List<Permission>();
            Roles = new List<Role>();
        }

        public virtual string Name { get; set; }
        public virtual ICollection<Permission> Permissions { get; set; }
        public virtual ICollection<Role> Roles { get; set; }
    }
}