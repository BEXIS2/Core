using System;

namespace BEXIS2.DB.Entities.Common
{
    public class EntityStateInfo
    {
        public virtual string State { get; set; }
        public virtual DateTime? Timestamp { get; set; }
        public virtual string Comment { get; set; }
    }
}