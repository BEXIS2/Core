using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BExIS.Security.Entities.Objects;
using Vaiona.Entities.Common;

namespace BExIS.Security.Entities.Authorization
{
    public class Role : BaseEntity
    {
        #region Attributes

        public virtual string Name { get; set; }
        public virtual string Description { get; set; }

        #endregion
    }
}
