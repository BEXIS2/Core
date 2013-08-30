using System.Collections.Generic;
using System.Xml;
using BExIS.Dlm.Entities.Administration;
using BExIS.Dlm.Entities.Data;
using Vaiona.Entities.Common;

namespace BExIS.Dlm.Entities.DataStructure
{
    public abstract class DataStructure : BusinessEntity
    {
        #region Attributes

        public virtual string Name { get; set; }
        public virtual string Description { get; set; }
        public virtual string XsdFileName { get; set; } // this is used to validate xml representation of datasets, version sensitive
        public virtual string XslFileName { get; set; } // used in UI to show the belonging datasets using proper transformation, version sensitive
        public virtual XmlNode ConfigurationInfo { get; set; }

        /// <summary>
        /// This is a workaround according to NHibernate's Lazy loading proxy creation!
        /// It should not be mapped!
        /// </summary>        
        public virtual DataStructure Self { get { return this; } }

        // Also it is possible to use the following code, but it creates a dependency to NH
        //public static T CastEntity<T>(this T entity)
        //{
        //    INhibernateProxy proxy = entity as INHibernateProxy;
        //    if (proxy != null)
        //    {
        //        return (T)proxy.HibernateLazyInitializer.GetImplementation();
        //    }
        //    else
        //    {
        //        return (T)entity;
        //    }
        //}

        #endregion
    
        #region Associations

        public virtual ICollection<Dataset> Datasets { get; set; }
        public virtual ICollection<DataView> Views { get; set; }
        public virtual ICollection<ResearchPlan> ResearchPlans { get; set; }

        #endregion
    
        #region Mathods
        #endregion

    }

    public enum DataStructureCategory
    {
        Generic, Sampling, Coverage, TimeSeries
    }
}
