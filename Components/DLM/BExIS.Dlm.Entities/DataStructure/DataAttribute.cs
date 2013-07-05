using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BExIS.Dlm.Entities.DataStructure
{
    public class DataAttribute : DataContainer
    {
        #region Attributes

        #endregion

        #region Associations

        public virtual ICollection<VariableUsage> UsagesAsVariable { get; set; } // StructuredDataStructure is the controller of this association
        public virtual ICollection<ParameterUsage> UsagesAsParameter { get; set; } 
        public virtual Classifier Classification { get; set; }

        #endregion

        #region Mathods

        public DataAttribute()
        {
            UsagesAsParameter = new List<ParameterUsage>();
            UsagesAsVariable = new List<VariableUsage>();
            Classification = new Classifier();
        }

        #endregion
    }
}
