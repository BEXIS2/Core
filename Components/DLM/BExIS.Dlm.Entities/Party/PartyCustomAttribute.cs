using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
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
        public virtual string Name { get; set; }
        public virtual string Description { get; set; }
        public virtual bool IsValueOptional { get; set; }
        public virtual int DisplayOrder { get; set; }
        public virtual string DataType { get; set; } // the type of value. mainly used for UI rendering and validation purposes
        // restricts the vlaues allowed at the corresponding attribute's values. dictionary is chosen to have internal code/ UI friendly names.
        // empty list means no restriction.
        [XmlIgnore]
        public virtual List<Dictionary<string, string>> ValidValues { get; set; } 
        #endregion

        #region Associations

        public virtual PartyType PartyType { get; set; }
        public virtual ICollection<PartyCustomAttributeValue> CustomAttributeValues { get; set; }
        
        #endregion
    
    }
}
