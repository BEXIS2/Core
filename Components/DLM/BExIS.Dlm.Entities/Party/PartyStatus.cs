using System;
using System.Xml;
using Vaiona.Entities.Common;

namespace BExIS.Dlm.Entities.Party
{
    public class PartyStatus : BaseEntity
    {
        #region Attributes

        public virtual DateTime Timestamp { get; set; }
        public virtual string Description { get; set; }
        public virtual XmlDocument ChangeLog { get; set; }

        #endregion Attributes

        #region Associations

        public virtual PartyStatusType StatusType { get; set; }
        public virtual Party Party { get; set; }

        #endregion Associations
    }
}