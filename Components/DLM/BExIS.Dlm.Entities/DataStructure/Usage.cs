using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;
using Vaiona.Entities.Common;

namespace BExIS.Dlm.Entities.DataStructure
{
    public class Parameter : BaseEntity
    {
        public virtual DataAttribute DataAttribute { get; set; }
        public virtual Variable Variable { get; set; }

        public virtual bool IsValueOptional { get; set; }
        public virtual string Label { get; set; }

    }

    public class Variable : BaseEntity
    {
        public virtual DataAttribute DataAttribute { get; set; }
        public virtual StructuredDataStructure DataStructure { get; set; }
        public virtual ICollection<Parameter> Parameters { get; set; } // DataAttribute is the controller of this association

        public virtual bool IsValueOptional { get; set; }
        public virtual string Label { get; set; }

    }
}
