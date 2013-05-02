using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;
using BExIS.Core.Data;

namespace BExIS.Dlm.Entities.DataStructure
{
    public class VariableParameterUsage : BaseEntity
    {
        //it is possible to remove ID and use Parameter.Id + Variable.Id as composite key. check at mapping time

        public virtual Parameter Parameter { get; set; }
        public virtual Variable Variable { get; set; }

        public virtual bool IsOptional { get; set; }

    }

    public class StructuredDataVariableUsage : BaseEntity
    {
        // it is possible to remove ID and use Parameter.Id + Variable.Id as composite key. check at mapping time

        public virtual Variable Variable { get; set; }
        public virtual StructuredDataStructure DataStructure { get; set; }

        public virtual bool IsOptional { get; set; }

    }
}
