using System;
using Vaiona.Entities.Common;

namespace BExIS.Dlm.Entities.Party
{
    public class PartyTypePair : BaseEntity
    {
        #region Attributes

        public virtual string Title { get; set; }
        public virtual string Description { get; set; }
        public virtual string ConditionSource { get; set; }
        public virtual string ConditionTarget { get; set; }
        public virtual int PermissionTemplate { get; set; }
        public virtual Boolean PartyRelationShipTypeDefault { get; set; }

        #endregion Attributes

        #region Associations

        public virtual PartyType SourcePartyType { get; set; }
        public virtual PartyType TargetPartyType { get; set; }
        public virtual PartyRelationshipType PartyRelationshipType { get; set; }

        #endregion Associations
    }
}