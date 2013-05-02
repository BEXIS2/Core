using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BExIS.Core.Data;
using BExIS.Security.Entities.Types;

namespace BExIS.Security.Entities
{
    public class FeatureRule : BaseEntity
    {
        #region Attributes

        public virtual RuleType RuleType { get; set; }

        #endregion


        #region Associations

        public virtual Feature Feature { get; set; }
        public virtual Subject Subject { get; set; }

        #endregion


        #region Methods

        #endregion
    }
}
