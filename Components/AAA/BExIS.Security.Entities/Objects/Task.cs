using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vaiona.Entities.Common;

namespace BExIS.Security.Entities.Objects
{
    public class Task : BaseEntity
    {
        public virtual string AreaName { get; set; }
        public virtual string ControllerName { get; set; }
        public virtual string ActionName { get; set; }

        public virtual Feature Feature { get; set; }
    }
}
