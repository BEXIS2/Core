using System;
using System.Collections.Generic;
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
        public virtual bool IsTemp { get; set; }

        public Party()
        {
            CustomAttributeValues = new List<PartyCustomAttributeValue>();
            PartyType = new PartyType();
            History = new List<PartyStatus>();
        }

        #region Associations

        public virtual PartyType PartyType { get; set; }
        public virtual ICollection<PartyCustomAttributeValue> CustomAttributeValues { get; set; }
        public virtual ICollection<PartyStatus> History { get; set; }

        //Check late
        //It should be filled by the last CustomAttributeValue from CustomAttributeValues
        public virtual PartyStatus CurrentStatus { get; set; }

        #endregion Associations
    }
}