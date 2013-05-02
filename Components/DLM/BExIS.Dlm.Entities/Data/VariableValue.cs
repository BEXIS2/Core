using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BExIS.Dlm.Entities.DataStructure;
using System.Xml;
using System.Xml.Serialization;

namespace BExIS.Dlm.Entities.Data
{
    public class VariableValue: DataValue
    {
        #region Attributes

        [XmlIgnore]
        public DataTuple Tuple { get; set; } // reference to the containing tuple

        public Int64 VariableId { get; set; } // when variable is not loaded!

        #endregion

        #region Associations
        
        public IList<ParameterValue> ParameterValues { get; set; }

        [XmlIgnore]
        public Variable Variable
        {
            get
            {
                if (this.Tuple.Dataset.DataStructure.Self is StructuredDataStructure)
                {
                    Variable v = (this.Tuple.Dataset.DataStructure.Self as StructuredDataStructure).VariableUsages.Where(p => p.Variable.Id.Equals(this.VariableId)).Select(p=>p.Variable).FirstOrDefault();
                    return (v);
                }
                return (null);
            }
        }
        
        #endregion

        #region Mathods

        public VariableValue()
        {
            ParameterValues = new List<ParameterValue>();
        }

        #endregion

    }
}
