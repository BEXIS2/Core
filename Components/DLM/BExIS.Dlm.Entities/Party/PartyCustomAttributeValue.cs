using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vaiona.Entities.Common;

namespace BExIS.Dlm.Entities.Party
{
    public class PartyCustomAttributeValue: BaseEntity
    {
        #region Attributes
        public virtual string Value { get; set; }
        #endregion

        #region Associations
        public virtual PartyCustomAttribute CustomAttribute { get; set; }
        #endregion
    }
}
