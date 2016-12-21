using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vaiona.Entities.Common;

namespace BExIS.Dlm.Entities.Party
{
    public class PartyStatusType: BaseEntity
    {
        public PartyStatusType()
        {
            Statuses = new List<PartyStatus>();
        }
        #region Attributes
        public virtual string Name { get; set; }
        public virtual string Description { get; set; }
        public virtual int DisplayOrder { get; set; }
        #endregion

        #region Associations
        public virtual PartyType PartyType { get; set; }
        public virtual ICollection<PartyStatus> Statuses{ get; set; }
        #endregion
    }
}
