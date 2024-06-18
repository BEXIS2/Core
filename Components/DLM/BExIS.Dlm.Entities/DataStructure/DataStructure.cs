using BExIS.Dlm.Entities.Administration;
using BExIS.Dlm.Entities.Data;
using System.Collections.Generic;
using System.Xml;
using Vaiona.Entities.Common;

/// <summary>
///
/// </summary>
namespace BExIS.Dlm.Entities.DataStructure
{
    /// <summary>
    ///
    /// </summary>
    /// <remarks></remarks>
    public abstract class DataStructure : BusinessEntity
    {
        #region Attributes

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        public virtual string Name { get; set; }

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        public virtual string Description { get; set; }

        /// <summary>
        /// this is used to validate xml representation of datasets, version sensitive
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        public virtual string XsdFileName { get; set; }

        /// <summary>
        /// used in UI to show the belonging datasets using proper transformation, version sensitive
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        public virtual string XslFileName { get; set; }

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        public virtual XmlNode ConfigurationInfo { get; set; }

        /// <summary>
        /// This is a workaround according to NHibernate's Lazy loading proxy creation!
        /// It should not be mapped!
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        public virtual DataStructure Self
        { get { return this; } }

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

        #endregion Attributes

        #region Associations

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        public virtual ICollection<Dataset> Datasets { get; set; }

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        public virtual ICollection<DatasetView> Views { get; set; }

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        public virtual ICollection<ResearchPlan> ResearchPlans { get; set; }

        #endregion Associations
    }

    /// <summary>
    ///
    /// </summary>
    public enum DataStructureCategory
    {
        Generic, Sampling, Coverage, TimeSeries
    }
}