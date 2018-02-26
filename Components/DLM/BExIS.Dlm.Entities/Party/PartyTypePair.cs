using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
        public virtual Boolean PartyRelationShipTypeDefault { get; set; }
        #endregion

        #region Associations
        public virtual PartyType AllowedSource { get; set; }
        public virtual PartyType AllowedTarget { get; set; }
        public virtual PartyRelationshipType PartyRelationshipType { get; set; }
        #endregion

    }
}
