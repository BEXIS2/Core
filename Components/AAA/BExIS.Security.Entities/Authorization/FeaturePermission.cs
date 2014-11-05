using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BExIS.Security.Entities.Objects;

namespace BExIS.Security.Entities.Authorization
{
    public class FeaturePermission : Permission
    {
        #region Associations

        public virtual Feature Feature { get; set; }

        #endregion
    }
}
