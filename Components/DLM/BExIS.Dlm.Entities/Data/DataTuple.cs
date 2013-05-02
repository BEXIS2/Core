using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BExIS.Core.Data;
using System.Xml.Linq;
using System.Xml;
using BExIS.Core.Serialization;

namespace BExIS.Dlm.Entities.Data
{
    [AutomaticMaterializationInfo("VariableValues", typeof(List<VariableValue>), "XmlVariableValues", typeof(XmlDocument))]
    [AutomaticMaterializationInfo("Amendments", typeof(List<Amendment>), "XmlAmendments", typeof(XmlDocument))]
    public class DataTuple : BaseEntity, IBusinessVersionedEntity
    {
        #region Attributes
        
        public virtual int OrderNo { get; set; }
        /// <summary>
        /// Every variablevalue has a value, result time and also sampling time (if apply)
        /// In addition the xml node contains all associated parameter values
        /// every variablevalue knows about its Obtaining method which is one of the items in the Variable ValueType
        /// </summary>
        public virtual XmlDocument XmlVariableValues { get; set; }
        public virtual XmlDocument XmlAmendments { get; set; } 
        
        #endregion
     
        #region Associations        

        public virtual Dataset Dataset { get; set; } // inverse map
        // Map from and to XmlVariableValues. Do not map to persistence data directly
        public virtual IList<VariableValue> VariableValues { get; set; }
        // Do not map to persistence data directly. Materialize after load
        public virtual IList<Amendment> Amendments { get; set; }
        
        #endregion        


        #region Methods

        // No need to override these functions, the base one performs the task for normal cases
        // If you have a very special case or the performance of the generic one is not good, then override the methods
        //public override void Dematerialize()
        //{            
        //    XmlVariableValues = (XmlDocument)transformer.ExportTo(VariableValues, "VariableValues", 1);            
        //    XmlAmendments = (XmlDocument)transformer.ExportTo(Amendments, "Amendments", 1);             
        //}

        //public override void Materialize()
        //{
        //    VariableValues = transformer.ImportFrom<List<VariableValue>>(XmlVariableValues, null);
        //    Amendments = transformer.ImportFrom<List<Amendment>>(XmlAmendments, null);
        //}

        public DataTuple()
        {
            //XmlVariableValues = new XmlDocument();
            //XmlAmendments = new XmlDocument();
            VariableValues = new List<VariableValue>();
            Amendments = new List<Amendment>();
        }

        public override void Materialize()
        {
            base.Materialize();
            VariableValues.ToList().ForEach(p => p.Tuple = this);
        }
        #endregion
    }
}
