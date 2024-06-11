using System.Collections.Generic;
using Vaiona.Entities.Common;

namespace BExIS.Dlm.Entities.Party
{
    public class PartyCustomAttribute : BaseEntity
    {
        public PartyCustomAttribute()
        {
            CustomAttributeValues = new List<PartyCustomAttributeValue>();
        }

        #region Attributes

        public virtual string Name { get; set; }
        private string displayName;

        public virtual string DisplayName
        { get { return string.IsNullOrEmpty(displayName) ? Name : displayName; } set { displayName = value; } }

        public virtual string Condition { get; set; }
        public virtual string Description { get; set; }
        public virtual bool IsValueOptional { get; set; }
        public virtual bool IsUnique { get; set; }
        public virtual bool IsMain { get; set; }
        public virtual int DisplayOrder { get; set; }
        public virtual string DataType { get; set; } // the type of value. mainly used for UI rendering and validation purposes
                                                     // restricts the vlaues allowed at the corresponding attribute's values. dictionary is chosen to have internal code/ UI friendly names.
                                                     // empty list means no restriction.

        public virtual string ValidValues { get; set; }

        #endregion Attributes

        #region Associations

        public virtual PartyType PartyType { get; set; }
        public virtual ICollection<PartyCustomAttributeValue> CustomAttributeValues { get; set; }

        #endregion Associations
    }
}