using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BExIS.Dlm.Entities.Party
{
    /// <summary>
    /// RelationshipTypes are described from the source point of view. The sources are the higher level or the container entities.
    /// For example, A Project HIRES a person, so the relationship would be HIRES or Employs. When a consortium is created it HOLDS at least two other organizations.
    /// The type of the organizations allowed to be HOLD are determined by the PartyRelationshipType, ...
    /// PartyRelationshipType can have cardinality to restrict the number of allowed 2nd parties IN RELATIONSHIP with any given 1st party.
    /// </summary>
    public class PartyRelationshipType
    {
        #region Attributes
        public string Title { get; set; }
        public string Description { get; set; }
        
        /// <summary>
        /// If true, the the source party type is considered to be the parent of the target party type, hence the same for the associated parties in the linked relationship
        /// </summary>
        public Boolean IndicatesHierarchy { get; set; }
        
        /// <summary>
        /// Minimum numbers of parties of the target type that must be asscoated to a party of the source type
        /// </summary>
        public int MinCardinality { get; set; }

        /// <summary>
        /// Maximum numbers of parties of the target type that can be asscoated to a party of the source type
        /// </summary>
        public int MaxCardinality { get; set; }
        #endregion

        #region Associations
        public virtual ICollection<PartyTypePair> AssociatedPairs { get; set; }
        public virtual ICollection<PartyRelationship> PartyRelationships{ get; set; }
        #endregion

    }
}
