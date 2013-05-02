using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BExIS.Core.Data;
using System.Diagnostics.Contracts;
using System.Xml;
using BExIS.Dlm.Entities.Data;

namespace BExIS.Dlm.Entities.DataStructure
{
    public abstract class DataStructure : BusinessEntity
    {
        #region Mathods
        #endregion
        
        #region Attributes

        public virtual string Name { get; set; }
        public virtual string Description { get; set; }
        public virtual string XsdFileName { get; set; } // this is used to validate xml representation of datasets, version sensetive
        public virtual string XslFileName { get; set; } // used in UI to show the belonging datasets using proper transformation, version sensetive
        public virtual XmlNode ConfigurationInfo { get; set; }

        /// <summary>
        /// This is a workaroung according to NHibernate's Lazy loading proxy creation!
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

        #endregion

    }

    public enum DataStructureCategory
    {
        Generic, Sampling, Coverage, TimeSeries
    }
}
