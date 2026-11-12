using System;
using BEXIS2.DB.Entities.Common;

namespace BEXIS2.Security.Entities.Versions
{
    public class Version : BaseEntity
    {
        public virtual long Id { get; set; }
        public virtual string Module { get; set; }
        public virtual string Value { get; set; }
        public virtual DateTime Date { get; set; }
    }
}