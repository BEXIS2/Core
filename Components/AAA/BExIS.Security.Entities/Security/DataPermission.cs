using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace BExIS.Security.Entities.Security
{
    public class DataPermission : Permission
    {
        public virtual Entity Entity { get; set; }
        public virtual long DataId { get; set; }

        public virtual RightType RightType { get; set; }
    }

    public enum RightType
    {
        Create = 0,
        Read = 1,
        Update = 2,
        Delete = 3,
        Download = 4,
        View = 5,
        Grant = 6
    }
}
