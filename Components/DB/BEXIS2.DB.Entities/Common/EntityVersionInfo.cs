using System;

namespace BEXIS2.DB.Entities.Common
{
    public class EntityVersionInfo
    {
        public virtual Int32 VersionNo { get; set; }
        public virtual DateTime TimeStamp { get; set; }
    }
}