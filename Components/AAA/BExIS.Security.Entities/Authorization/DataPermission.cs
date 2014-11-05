using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BExIS.Security.Entities.Objects;

namespace BExIS.Security.Entities.Authorization
{
    public class DataPermission : Permission
    {
        #region Attributes

        public virtual long DataId { get; set; }
        public virtual RightType RightType { get; set; }

        #endregion

        #region Associations

        public virtual Entity Entity { get; set; }

        #endregion
    }
}
