using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using BExIS.Dlm.Entities.DataStructure;

namespace BExIS.Dlm.Entities.Data
{
    public class ParameterValue: DataValue
    {
        public Int64 ParameterId { get; set; } // when Parameter is not loaded. It happens when mapping from Xml data is performed after loading object from DB

        [XmlIgnore]
        public VariableValue VariableValue{ get; set; } // reference to the containing variable value

        [XmlIgnore]
        public DataAttribute DataAttribute
        {
            get
            {
                if (this.VariableValue.Tuple.DatasetVersion.Dataset.DataStructure.Self is StructuredDataStructure)
                {
                    return (this.Parameter.DataAttribute);
                }
                return (null);
            }
        }

        [XmlIgnore]
        public Parameter Parameter
        {
            get
            {
                if (this.VariableValue.Tuple.DatasetVersion.Dataset.DataStructure.Self is StructuredDataStructure)
                {
                    var q = from vu in (this.VariableValue.Tuple.DatasetVersion.Dataset.DataStructure.Self as StructuredDataStructure).Variables
                            from pu in vu.Parameters
                            where pu.Id.Equals(this.ParameterId)
                            select pu;
                    return (q.FirstOrDefault());
                                      
                }
                return (null);
            }
        }

    }
}
