﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vaiona.Entities.Common;

namespace BExIS.Dlm.Entities.Party
{
    /// <summary>
    /// this.PartyRelationshipType.PartyTypePairs.AllowedSource MUST CONTAIN this.FirstParty.PartyRelationshipType
    /// AND
    /// this.PartyRelationshipType.PartyTypePairs.AllowedAtrget MUST CONTAIN this.SecondParty.PartyRelationshipType
    /// </summary>
    public class PartyRelationship : BaseEntity
    {
        public PartyRelationship()
        {
            PartyRelationshipType = new PartyRelationshipType();
            FirstParty = new Party();
            SecondParty = new Party();
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
        #endregion

        #region Associations
        public virtual PartyRelationshipType PartyRelationshipType { get; set; }
        public virtual PartyTypePair PartyTypePair { get; set; }
        public virtual Party FirstParty { get; set; }
        public virtual Party SecondParty { get; set; }
        #endregion
    }
}
