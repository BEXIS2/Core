using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BExIS.Core.Data;

namespace BExIS.Security.Entities
{
    public abstract class Subject : BaseEntity
    {
        #region Attributes

        public virtual string Name { get; set; }
        public virtual string LowerCaseName { get; set; }

        public virtual string Description { get; set; }

        public virtual string Comment { get; set; }

        #endregion


        #region Associations

        public virtual ICollection<FeatureRule> FeatureRules { get; set; }

        #endregion


        #region Methods

        #endregion
    }
}
