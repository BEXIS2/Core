using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BExIS.Dlm.Entities.DataStructure
{
    public class Parameter : DataContainer
    {
        public virtual ICollection<VariableParameterUsage> VariableUsages { get; set; } // Variable is the controller of this association

        public Parameter()
        {
            VariableUsages = new List<VariableParameterUsage>();
            // VariableUsages.First().Variable.Parameters //sample: all parameteres that are associated with first variable which I have assoiated with!!

        }
    }

}
