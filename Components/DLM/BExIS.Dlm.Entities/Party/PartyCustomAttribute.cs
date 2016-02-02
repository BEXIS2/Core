using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vaiona.Entities.Common;

namespace BExIS.Dlm.Entities.Party
{
    public class PartyCustomAttribute: BaseEntity
    {
        public PartyCustomAttribute()
        {
            ValidValues = new List<Dictionary<string, string>>();
        }

        #region Attributes
        public string Name { get; set; }
        public string Description { get; set; }
        public bool IsValueOptional { get; set; }
        public int DisplayOrder { get; set; }
        public string DataType { get; set; } // the type of value. mainly used for UI rendering and validation purposes
        // restricts the vlaues allowed at the corresponding attribute's values. dictionary is chosen to have internal code/ UI friendly names.
        // empty list means no restriction.
        public List<Dictionary<string, string>> ValidValues { get; set; } 
        #endregion

        #region Associations

        public PartyType PartyType { get; set; }
        public virtual ICollection<PartyCustomAttributeValue> CustomAttributeValues { get; set; }
        
        #endregion
    
    }
}
