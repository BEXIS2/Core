using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BExIS.Dlm.Entities.Party
{
    public class PartyTypePair
    {
        #region Attributes
        public string Title { get; set; }
        public string Description { get; set; }
        #endregion

        #region Associations
        public virtual PartyType AlowedSource { get; set; }
        public virtual PartyType AlowedTarget { get; set; }
        public virtual PartyRelationshipType PartyRelationshipType { get; set; }
        #endregion

    }
}
