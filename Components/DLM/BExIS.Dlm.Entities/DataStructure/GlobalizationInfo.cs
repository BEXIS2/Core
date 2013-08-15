using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vaiona.Entities.Common;

namespace BExIS.Dlm.Entities.DataStructure
{
    public class GlobalizationInfo: BaseEntity
    {
        #region Attributes

        public virtual string CultureId { get; set; }
        public virtual string DisplayName { get; set; }
        public virtual string Description { get; set; }

        #endregion

        #region Associations

        public virtual DataContainer DataContainer { get; set; }

        #endregion

        #region Mathods

        #endregion
    }
}
