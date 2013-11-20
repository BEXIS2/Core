using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BExIS.Security.Entities
{
    public class FeaturePermission : Permission
    {
        #region Attributes

        #endregion


        #region Associations

        public virtual Feature Feature { get; set; }

        #endregion


        #region Methods

        #endregion
    }
}
