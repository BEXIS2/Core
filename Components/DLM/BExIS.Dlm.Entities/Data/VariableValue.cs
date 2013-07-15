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
        public DataAttribute DataAttribute
        {
            get
            {
                if (this.Tuple.DatasetVersion.Dataset.DataStructure.Self is StructuredDataStructure)
                {
                    return (this.Usage.DataAttribute);
                }
                return (null);
            }
        }

        [XmlIgnore]
        public Variable Usage
        {
            get
            {
                if (this.Tuple.DatasetVersion.Dataset.DataStructure.Self is StructuredDataStructure)
                {
                    Variable u = (this.Tuple.DatasetVersion.Dataset.DataStructure.Self as StructuredDataStructure).Variables
                        .Where(p => p.Id.Equals(this.VariableId))
                        .Select(p => p).FirstOrDefault();
                    return (u);
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
