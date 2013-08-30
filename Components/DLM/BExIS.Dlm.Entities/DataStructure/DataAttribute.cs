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

        public virtual ICollection<Variable> UsagesAsVariable { get; set; } // StructuredDataStructure is the controller of this association
        public virtual ICollection<Parameter> UsagesAsParameter { get; set; } 
        public virtual Classifier Classification { get; set; }

        #endregion

        #region Mathods

        public DataAttribute(): base()
        {
            UsagesAsParameter = new List<Parameter>();
            UsagesAsVariable = new List<Variable>();
            //Classification = new Classifier();
        }

        #endregion
    }
}
