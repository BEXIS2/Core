using System;
using System.Reflection;
using System.Xml;
using Vaiona.Core.Serialization;

namespace Vaiona.Entities.Common
{
    [Serializable]
    public abstract class BaseEntity : ISystemVersionedEntity //EntityWithTypedId<Int64>
    {
        #region Attributes

        public virtual Int64 Id { get; set; }

        public virtual Int32 VersionNo { get; set; }
        //public virtual DateTime? TimeStamp { get; set; }

        //public virtual EntityVersionInfo VersionInfo { get; set; } // Map as a component // Version item does not work inside component!!!
        public virtual XmlNode Extra { get; set; }

        #endregion Attributes

        #region Mathods

        //public override bool Equals(object obj)
        //{
        //    if (base.Equals(obj))
        //        return (true);
        //    if ( ((BaseEntity)obj).Id == 0 || this.Id == 0)
        //        return (false);
        //    if (this.Id == ((BaseEntity)obj).Id)
        //        return (true);
        //    else
        //        return (false);
        //}

        //public override int GetHashCode()
        //{
        //    if (Id <= 0)
        //        return base.GetHashCode();
        //    return (Id.GetHashCode());
        //}

        /// <summary>
        /// In some cases like DataTuple, value of some properties of the object are persisted ax xml. in this case there is an in-memory version and an in-storage version
        /// Dematerialize method, creates the Xml version of those properties and stores them in the relevant persistence properties
        /// If you have put AutomaticMaterializationInfoAttribute attributes on your class and have not overridden this method, then it will try to transform the original variables into Xml counterparts
        /// No need to override these functions, the base one performs the task for normal cases
        /// If you have a very special case or the performance of the generic one is not good, then override the methods
        /// </summary>
        public virtual void Dematerialize(bool includeChildren = true)
        {
            //XmlVariableValues = (XmlDocument)transformer.ExportTo(VariableValues, "VariableValues", 1); sample
            //XmlAmendments = (XmlDocument)transformer.ExportTo(Amendments, "Amendments", 1);
            foreach (AutomaticMaterializationInfoAttribute attr in this.GetType().GetCustomAttributes(typeof(AutomaticMaterializationInfoAttribute), true))
            {
                IObjectTransfromer transformer = new XmlObjectTransformer(); // provide configuration based plug ability
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
        public virtual void Materialize(bool includeChildren = true)
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

        #endregion Mathods
    }

    public abstract class BusinessEntity : BaseEntity, IStatefullEntity, IAuditableEntity
    {
        #region Attributes

        public virtual EntityStateInfo StateInfo { get; set; } // Map as a component
        public virtual EntityAuditInfo CreationInfo { get; set; } // Map as a component
        public virtual EntityAuditInfo ModificationInfo { get; set; } // Map as a component

        #endregion Attributes

        #region Mathods

        public BusinessEntity()
        {
            StateInfo = new EntityStateInfo();
            CreationInfo = new EntityAuditInfo();
            ModificationInfo = new EntityAuditInfo();
        }

        //public abstract void Validate();

        #endregion Mathods
    }
}