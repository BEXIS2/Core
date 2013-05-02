using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BExIS.Core.Data;
using BExIS.Security.Entities.Info;

namespace BExIS.Security.Entities
{
    public class Feature : BaseEntity
    {
        #region Attributes

        public virtual string Name { get; set; }
        public virtual string LowerCaseName { get; set; }

        public virtual ActionInfo ActionInfo { get; set; }

        #endregion


        #region Associations

        public virtual Feature Parent { get; set; }

        public virtual ICollection<FeatureRule> FeatureRules { get; set; }

        #endregion


        #region Methods

        public Feature()
        {
            FeatureRules = new List<FeatureRule>();
        }

        #endregion
    }
}
