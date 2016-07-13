using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vaiona.Entities.Common;

namespace BExIS.Dlm.Entities.Party
{
    public class Party : BaseEntity
    {
        public virtual string Name { get; set; }
        public virtual string Alias { get; set; }
        public virtual string Description { get; set; }
        public virtual DateTime StartDate { get; set; }
        public virtual DateTime EndDate { get; set; }
        #region Associations
        public virtual PartyType PartyType { get; set; }
        public virtual ICollection<PartyCustomAttributeValue> CustomAttributeValues { get; set; }
        
        public virtual ICollection<PartyStatus> History{ get; set; }
        public virtual PartyStatus CurrentStatus { get; set; }

        #endregion
    }
}
