using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BExIS.Dlm.Entities.DataStructure
{
    public class Variable : DataContainer
    {
        #region Attributes

        #endregion

        #region Associations

        public virtual ICollection<VariableParameterUsage> ParameterUsages { get; set; } // Variable is the controller of this association
        public virtual ICollection<StructuredDataVariableUsage> StructuredDataUsages { get; set; } // StructuredDataStructure is the controller of this association
        public virtual Classifier Classification { get; set; }

        #endregion

        #region Mathods

        public Variable()
        {
            ParameterUsages = new List<VariableParameterUsage>();
            StructuredDataUsages = new List<StructuredDataVariableUsage>();
            Classification = new Classifier();
        }

        #endregion
    }
}
