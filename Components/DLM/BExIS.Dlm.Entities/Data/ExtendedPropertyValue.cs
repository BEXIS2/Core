using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BExIS.Dlm.Entities.DataStructure;
using System.Xml.Serialization;

namespace BExIS.Dlm.Entities.Data
{
    public class ExtendedPropertyValue: DataValue
    {
        #region Attributes

        public Int64 ExtendedPropertyId { get; set; } // when ExtendedProperty is not loaded. It happens when mapping from Xml data is performed after loading object from DB

        #endregion

        #region Associations

        [XmlIgnore]
        public Dataset Dataset { get; set; }

        [XmlIgnore]
        public ExtendedProperty ExtendedProperty //{ get; set; } // it should be connected to the real extended property object
        {
            get
            {
                if (this.Dataset.DataStructure.Self is StructuredDataStructure)
                {
                    StructuredDataStructure sds = (this.Dataset.DataStructure.Self as StructuredDataStructure);
                    ExtendedProperty ep = (from vu in sds.VariableUsages
                                          let v = vu.Variable
                                          from exp in v.ExtendedProperties
                                          where exp.Id == ExtendedPropertyId
                                          select exp).FirstOrDefault();
                    if(ep == null)  ep = (from vu in sds.VariableUsages
                                           let v = vu.Variable
                                           from pu in v.ParameterUsages
                                           let p = pu.Parameter
                                           from exp in p.ExtendedProperties
                                           where exp.Id == ExtendedPropertyId
                                           select exp).FirstOrDefault();
                                              
                    return (ep);
                }
                return (null);
            }
        }


        #endregion

        #region Methods

        #endregion

    }
}
