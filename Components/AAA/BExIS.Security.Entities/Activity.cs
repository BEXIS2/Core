using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vaiona.Entities.Common;

namespace BExIS.Security.Entities
{
    public class Activity : BaseEntity
    {
        #region Attributes

        public virtual string Name { get; set; }
        public virtual string Description { get; set; }

        #endregion


        #region Associations

        public virtual ICollection<Action> Actions { get; set; }
        public virtual ICollection<Feature> Features { get; set; }

        #endregion


        #region Methods

        #endregion
    }
}
