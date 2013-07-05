using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BExIS.Core.Data
{
    public interface ISystemVersionedEntity
    {
        //EntityVersionInfo VersionInfo { get; set; }
        Int32 VersionNo { get; set; }
        //DateTime TimeStamp { get; set; }
    }
}
