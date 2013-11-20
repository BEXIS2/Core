using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vaiona.Entities.Common;

namespace BExIS.Security.Entities
{
    public class Action : BaseEntity
    {
        #region Attributes

        public virtual string Name { get; set; }
        public virtual string Description { get; set; }

        public virtual string AreaName { get; set; }
        public virtual string ControllerName { get; set; }
        public virtual string ActionName { get; set; }

        #endregion


        #region Associations

        public virtual ICollection<Activity> Activities { get; set; }

        #endregion


        #region Methods

        #endregion
    }
}
