using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vaiona.Entities.Common;

namespace BExIS.Dlm.Entities.Party
{
    public class Party : BaseEntity
    {
        public string Name { get; set; }
        public string Alias { get; set; }
        public string Description { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        #region Associations
        public virtual PartyType PartyType { get; set; }
        public virtual ICollection<PartyCustomAttributeValue> CustomAttributeValues { get; set; }
        
        public virtual ICollection<PartyStatus> History{ get; set; }
        public virtual PartyStatus CurrentStatus { get; set; }

        #endregion
    }
}
