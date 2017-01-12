using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vaiona.Entities.Common;

namespace BExIS.Dlm.Entities.Party
{
    public class PartyTypePair:BaseEntity
    {
     
        #region Attributes
        public virtual string Title { get; set; }
        public virtual string Description { get; set; }
        #endregion

        #region Associations
        public virtual PartyType AlowedSource { get; set; }
        public virtual PartyType AlowedTarget { get; set; }
        public virtual PartyRelationshipType PartyRelationshipType { get; set; }
        #endregion

    }
}
