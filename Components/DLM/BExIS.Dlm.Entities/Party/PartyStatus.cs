using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using Vaiona.Entities.Common;

namespace BExIS.Dlm.Entities.Party
{
    public class PartyStatus: BaseEntity
    {
        #region Attributes
        public DateTime Timestamp { get; set; }
        public string Description { get; set; }
        public XmlDocument ChangeLog { get; set; }
        #endregion

        #region Associations
        public virtual PartyStatusType StatusType { get; set; }
        public virtual Party Party { get; set; }
        #endregion

    }
}
