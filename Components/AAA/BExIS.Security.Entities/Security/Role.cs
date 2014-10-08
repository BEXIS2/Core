using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vaiona.Entities.Common;

namespace BExIS.Security.Entities.Security
{
    public class Role : BaseEntity
    {
        public virtual string Name { get; set; }
        public virtual string Description { get; set; }

        public virtual int Rights { get; set; }
    }
}
