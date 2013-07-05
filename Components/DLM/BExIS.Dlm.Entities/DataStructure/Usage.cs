using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;
using BExIS.Core.Data;

namespace BExIS.Dlm.Entities.DataStructure
{
    public class ParameterUsage : BaseEntity
    {
        public virtual DataAttribute DataAttribute { get; set; }
        public virtual VariableUsage VariableUsage { get; set; }

        public virtual bool IsValueOptional { get; set; }
        public virtual string Label { get; set; }

    }

    public class VariableUsage : BaseEntity
    {
        public virtual DataAttribute DataAttribute { get; set; }
        public virtual StructuredDataStructure DataStructure { get; set; }
        public virtual ICollection<ParameterUsage> ParameterUsages { get; set; } // DataAttribute is the controller of this association

        public virtual bool IsValueOptional { get; set; }
        public virtual string Label { get; set; }

    }
}
