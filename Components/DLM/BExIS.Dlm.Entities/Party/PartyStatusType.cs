using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vaiona.Entities.Common;

namespace BExIS.Dlm.Entities.Party
{
    public class PartyStatusType: BaseEntity
    {
        #region Attributes
        public string Name { get; set; }
        public string Description { get; set; }
        public int DisplayOrder { get; set; }
        #endregion

        #region Associations
        public virtual PartyType PartyType { get; set; }
        public virtual ICollection<PartyStatus> Statuses{ get; set; }
        #endregion
    }
}
