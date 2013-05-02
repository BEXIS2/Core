using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BExIS.Core.Data
{
    public class EntityVersionInfo
    {
        public virtual Int32 VersionNo { get; set; }
        public virtual DateTime TimeStamp { get; set; }
    }
}
