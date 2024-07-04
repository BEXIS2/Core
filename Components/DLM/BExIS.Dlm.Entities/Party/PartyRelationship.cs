using System;
using Vaiona.Entities.Common;

namespace BExIS.Dlm.Entities.Party
{
    /// <summary>
    /// this.PartyRelationshipType.PartyTypePairs.SourcePartyType MUST CONTAIN this.SourceParty.PartyRelationshipType
    /// AND
    /// this.PartyRelationshipType.PartyTypePairs.AllowedAtrget MUST CONTAIN this.TargetParty.PartyRelationshipType
    /// </summary>
    public class PartyRelationship : BaseEntity
    {
        public PartyRelationship()
        {
            PartyRelationshipType = new PartyRelationshipType();
            SourceParty = new Party();
            TargetParty = new Party();
        }

        #region Attributes

        public virtual string Title { get; set; }
        public virtual string Description { get; set; }
        public virtual DateTime StartDate { get; set; }
        public virtual DateTime EndDate { get; set; }
        public virtual int Permission { get; set; }

        /// <summary>
        /// Any restriction on the relationship not captured by the other attributes. For example: Person 1 is the coauthor of Person 2 in the scope of Paper 1
        /// </summary>
        public virtual String Scope { get; set; }

        #endregion Attributes

        #region Associations

        public virtual PartyRelationshipType PartyRelationshipType { get; set; }
        public virtual PartyTypePair PartyTypePair { get; set; }
        public virtual Party SourceParty { get; set; }
        public virtual Party TargetParty { get; set; }

        #endregion Associations
    }
}