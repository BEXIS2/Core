using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BExIS.Dlm.Entities.Party
{
    public class PartyCustomAttributeValue
    {
        #region Attributes
        public string Value { get; set; }
        #endregion

        #region Associations
        public virtual PartyCustomAttribute CustomAttribute { get; set; }
        #endregion
    }
}
