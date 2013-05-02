using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BExIS.Security.Entities.Info
{
    public class ActionInfo
    {
        public virtual string Area { get; set; }
        public virtual string Controller { get; set; }
        public virtual string Action { get; set; }
    }
}
