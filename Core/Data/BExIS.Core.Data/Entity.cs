using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.Xml;
using BExIS.Core.Serialization;
using System.Reflection;

namespace BExIS.Core.Data
{
    using EntityId = Int64;
    using EntityRef = Int64;

    public abstract class BaseEntity: ISystemVersionedEntity
    {
        #region Fields        

        #endregion
       
        #region Attributes

        public virtual Int64 Id { get; set; }

        public virtual Int32 VersionNo { get; set; }
        //[Obsolete("Do not use this property, it is going to be deleted", false)]
        //public virtual DateTime TimeStamp { get; set; } // subject to remove

        //public virtual EntityVersionInfo VersionInfo { get; set; } // Map as a component // Version item does not work inside component!!!
        public virtual XmlNode Extra { get; set; }
        
        #endregion

        #region Associations

        #endregion

        #region Mathods

        /// <summary>
        /// In some cases like DataTuple, value of some properties of the object are persisted ax xml. in this case there is an in-memory version and an in-storage version
        /// Dematerialize method, creates the Xml version of those properties and stores them in the relevant persistence properties
        /// If you have put AutomaticMaterializationInfoAttribute attributes on your class and have not overriden this method, then it will try to transform the origianl variables into Xml counterparts 
        /// No need to override these functions, the base one performs the task for normal cases
        /// If you have a very special case or the performance of the generic one is not good, then override the methods
        /// </summary>
        public virtual void Dematerialize()
        {
            //XmlVariableValues = (XmlDocument)transformer.ExportTo(VariableValues, "VariableValues", 1); sample
            //XmlAmendments = (XmlDocument)transformer.ExportTo(Amendments, "Amendments", 1);
            foreach (AutomaticMaterializationInfoAttribute attr in this.GetType().GetCustomAttributes(typeof(AutomaticMaterializationInfoAttribute), true))
            {
                IObjectTransfromer transformer = new XmlObjectTransformer(); // provide configuration based plugability
                PropertyInfo sourceProperty = this.GetType().GetProperty(attr.SourcePropertyName);
                object sourceVal = sourceProperty.GetValue(this, null); // this is the original object based value of the Property

                object targetVal = transformer.ExportTo(attr.SourcePropertyType, sourceVal, attr.SourcePropertyName, "Content");
                if (targetVal != null)
                {
                    //obj = (new XmlDocument()).CreateNode(objType.Name.Replace("Xml", "").ToLower(), element.GetAttribute("Name"), null);
                    //if (!string.IsNullOrWhiteSpace(value))
                    //    ((XmlNode)obj).InnerXml = element.InnerXml;
                    //else
                    //    ((XmlNode)obj).InnerXml = value;

                    //XmlElement targetElement = ((XmlDocument)targetVal).DocumentElement;
                    XmlDocument targetElement = (XmlDocument)targetVal;
                    PropertyInfo targetProperty = this.GetType().GetProperty(attr.TargetPropertyName);
                    targetProperty.SetValue(this, targetElement, null);
                }
            }        
        }

        /// <summary>
        /// In some cases like DataTuple, value of some properties of the object are persisted ax xml. in this case there is an in-memory version and an in-storage version
        /// Materialize method, fills up the designated properties based on the Xml data that is inside the relevant persistence properties.
        /// No need to override these functions, the base one performs the task for normal cases
        /// If you have a very special case or the performance of the generic one is not good, then override the methods
        /// </summary>
        public virtual void Materialize()
        {
            IObjectTransfromer transformer = new XmlObjectTransformer();
            //VariableValues = transformer.ImportFrom<List<VariableValue>>(XmlVariableValues, 1, null);
            //Amendments = transformer.ImportFrom<List<Amendment>>(XmlAmendments, 1, null);
            foreach (AutomaticMaterializationInfoAttribute attr in this.GetType().GetCustomAttributes(typeof(AutomaticMaterializationInfoAttribute), true))
            {
                PropertyInfo targetProperty = this.GetType().GetProperty(attr.TargetPropertyName);
                object targetVal = targetProperty.GetValue(this, null); // this is the Xml representation of the Property

                if (targetVal != null) // check to see if this code is not preventing any scenario
                {
                    object sourceVal = transformer.ImportFrom(attr.SourcePropertyType, targetVal);

                    if (sourceVal != null)
                    {
                        PropertyInfo sourceProperty = this.GetType().GetProperty(attr.SourcePropertyName);
                        sourceProperty.SetValue(this, sourceVal, null);
                    }
                }
            }
        }
        
        #endregion

        
    }

    public abstract class BusinessEntity : BaseEntity, IStatefullEntity, IAuditableEntity
    {

        #region Attributes

        public virtual EntityStateInfo StateInfo { get; set; } // Map as a component
        public virtual EntityAuditInfo AuditInfo { get; set; } // Map as a component
        
        #endregion

        #region Associations

        #endregion

        #region Mathods

        public abstract void Validate();
        
        #endregion
    }
}
