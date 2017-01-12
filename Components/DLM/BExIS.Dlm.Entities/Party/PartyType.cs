using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vaiona.Entities.Common;

namespace BExIS.Dlm.Entities.Party
{
    public class PartyType : BaseEntity
    {
        public PartyType()
        {
            Parties = new List<Party>();
            CustomAttributes = new List<PartyCustomAttribute>();
            StatusTypes = new List<PartyStatusType>();
        }
        #region Attributes
        public virtual string Title { get; set; }
        public virtual string Description { get; set; }
        #endregion

        #region Associations
        public virtual ICollection<Party> Parties { get; set; }
        public virtual ICollection<PartyCustomAttribute> CustomAttributes { get; set; }
        public virtual ICollection<PartyStatusType> StatusTypes { get; set; }
        public virtual ICollection<PartyTypePair> PartyTypePairSources { get; set; }
        public virtual ICollection<PartyTypePair> PartyTypePairTargets { get; set; }
        #endregion
    }
}
