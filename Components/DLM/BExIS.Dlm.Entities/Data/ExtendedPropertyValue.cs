using System;
using System.Linq;
using System.Xml.Serialization;
using BExIS.Dlm.Entities.DataStructure;

namespace BExIS.Dlm.Entities.Data
{
    public class ExtendedPropertyValue: DataValue
    {
        #region Attributes

        public Int64 ExtendedPropertyId { get; set; } // when ExtendedProperty is not loaded. It happens when mapping from Xml data is performed after loading object from DB

        #endregion

        #region Associations

        [XmlIgnore]
        public DatasetVersion DatasetVersion { get; set; }

        [XmlIgnore]
        public ExtendedProperty ExtendedProperty //{ get; set; } // it should be connected to the real extended property object
        {
            get
            {
                if (this.DatasetVersion.Dataset.DataStructure.Self is StructuredDataStructure)
                {
                    StructuredDataStructure sds = (this.DatasetVersion.Dataset.DataStructure.Self as StructuredDataStructure);
                    ExtendedProperty ep = (from vu in sds.Variables
                                          let da = vu.DataAttribute
                                          from exp in da.ExtendedProperties
                                          where exp.Id == ExtendedPropertyId
                                          select exp).FirstOrDefault();
                    if(ep == null)  ep = (from vu in sds.Variables
                                           from pu in vu.Parameters  
                                           let da = pu.DataAttribute
                                           from exp in da.ExtendedProperties
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
