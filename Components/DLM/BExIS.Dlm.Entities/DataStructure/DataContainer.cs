using System.Collections.Generic;
using Vaiona.Entities.Common;

/// <summary>
///
/// </summary>
namespace BExIS.Dlm.Entities.DataStructure
{
    /// <summary>
    ///
    /// </summary>
    public enum DataContainerType
    {
        ValueType, ReferenceType
    }

    /// <summary>
    ///
    /// </summary>
    public enum MeasurementScale
    {
        Nominal, Ordinal, Interval, Ratio, DateTime, Categorial
    }

    /// <summary>
    ///
    /// </summary>
    /// <remarks></remarks>
    public abstract class DataContainer : BusinessEntity
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
        public virtual string ShortName { get; set; } // Abbrev.

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        public virtual string Description { get; set; }

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        public virtual bool IsMultiValue { get; set; }

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        public virtual bool IsBuiltIn { get; set; } // the build in containers can not be deleted, except by their scope

        // Scope of the container is the module who created it
        // or the RPM (Research Planning Module) if its created via the web interface
        // or DCM (Data Collection Module) if its created during data submission

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        public virtual string Scope { get; set; }

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        public virtual DataContainerType ContainerType { get; set; }

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        public virtual MeasurementScale MeasurementScale { get; set; }

        /// <summary>
        ///
        /// </summary>
        /// <remarks> only if Type == ReferenceType </remarks>
        /// <seealso cref=""/>
        public virtual string EntitySelectionPredicate { get; set; }

        /// <summary>
        /// This is a workaround according to NHibernate's Lazy loading proxy creation!
        /// It should not be mapped!
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        public virtual DataContainer Self
        { get { return this; } }

        #endregion Attributes

        #region Associations

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        public virtual DataType DataType { get; set; } //1

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        public virtual Unit Unit { get; set; } // 0..1

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        public virtual Methodology Methodology { get; set; } //0..1

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        public virtual ICollection<Constraint> Constraints { get; set; }

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        public virtual ICollection<ExtendedProperty> ExtendedProperties { get; set; }

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        public virtual ICollection<GlobalizationInfo> GlobalizationInfos { get; set; }

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        public virtual ICollection<AggregateFunction> AggregateFunctions { get; set; }

        //// not to map
        //public virtual IList<DomainConstraint> DomainConstraints
        //{
        //    get
        //    {
        //        return (this.Constraints.Distinct()
        //                    .Where(p => p.GetType().Equals(typeof(DomainConstraint)))
        //                    .ToList() as IList<DomainConstraint>
        //                );
        //    }
        //}

        #endregion Associations

        #region Methods

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param>NA</param>
        public DataContainer()
        {
            IsBuiltIn = false;
            Scope = "DLM";
            Constraints = new List<Constraint>();
            ExtendedProperties = new List<ExtendedProperty>();
            GlobalizationInfos = new List<GlobalizationInfo>();
            AggregateFunctions = new List<AggregateFunction>();
            IsMultiValue = false;
            ContainerType = DataContainerType.ValueType;
            EntitySelectionPredicate = string.Empty;
            MeasurementScale = MeasurementScale.Nominal;
        }

        //public override void Validate()
        //{
        //    // No dupplicate extended property
        //    // if Type == ReferenceType, EntitySelectionpredicate can not be null
        //    // must have one data type
        //    //ValidateDomainValues
        //    //ValidateValidatores

        //    throw new NotImplementedException();
        //}

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param name="data"></param>
        public virtual bool ValidateDomainValues(object data)
        {
            // data must be of type DataType linked to the container
            // check whether data matches one of the domain values
            return false;
        }

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param name="data"></param>
        public virtual bool ValidateValidatores(object data)
        {
            // data must be of type DataType linked to the container
            // run the Evaulate method of all associated validator objects
            return false;
        }

        #endregion Methods
    }
}