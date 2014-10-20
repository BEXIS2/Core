using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vaiona.Entities.Common;

namespace BExIS.Security.Entities.Security
{
    public class Entity : BaseEntity
    {
        public virtual string Name { get; set; }
        public virtual string ClassName { get; set; }
        public virtual string AssemblyName { get; set; }

        public virtual ICollection<DataPermission> DataPermissions { get; set; }

        public Entity()
        {
            DataPermissions = new List<DataPermission>();
        }
    }
}
