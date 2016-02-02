using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vaiona.Entities.Common;

namespace BExIS.Dlm.Entities.Party
{
    public class PartyType : BaseEntity
    {
        #region Attributes
        public string Title { get; set; }
        public string Description { get; set; }
        #endregion

        #region Associations
        public virtual ICollection<Party> Parties { get; set; }
        public virtual ICollection<PartyCustomAttribute> CustomAttributes { get; set; }
        public virtual ICollection<PartyStatusType> StatusTypes { get; set; }
        #endregion
    }
}
