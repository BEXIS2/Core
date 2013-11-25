using System.Collections.Generic;
using Vaiona.Entities.Common;

namespace BExIS.Dlm.Entities.DataStructure
{
    public class Parameter : BaseEntity
    {
        public virtual DataAttribute DataAttribute { get; set; }
        public virtual Variable Variable { get; set; }

        public virtual bool IsValueOptional { get; set; }
        public virtual string Label { get; set; }

        /// <summary>
        /// The actual data type of the value is defined by the DataAttribute.DataType.
        /// So developer should take care and convert this value to proper type before assigning it to parameter values
        /// </summary>
        public virtual string DefaultValue { get; set; }
        public virtual string MissingValue { get; set; } 

    }

    public class Variable : BaseEntity
    {
        public virtual DataAttribute DataAttribute { get; set; }
        public virtual StructuredDataStructure DataStructure { get; set; }
        public virtual ICollection<Parameter> Parameters { get; set; } // DataAttribute is the controller of this association

        public virtual bool IsValueOptional { get; set; }
        public virtual string Label { get; set; }

        /// <summary>
        /// The actual data type of the value is defined by the DataAttribute.DataType.
        /// So developer should take care and convert this value to proper type before assigning it to variable values
        /// </summary>
        public virtual string DefaultValue { get; set; }
        public virtual string MissingValue { get; set; } 

    }
}
