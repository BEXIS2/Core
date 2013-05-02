using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BExIS.Core.Data;

namespace BExIS.Dlm.Entities.DataStructure
{
    public enum DataContainerType
    {
        ValueType, ReferenceType
    }

    public enum MeasurementScale
    {
        Nominal, Ordinal, Interval, Ratio, DateTime, Categorial
    }

    public abstract class DataContainer: BusinessEntity
    {
        #region Attributes

        public virtual string Name { get; set; }
        public virtual string ShortName { get; set; } // Abbrev.
        public virtual string Description { get; set; }
        public virtual bool IsMultiValue { get; set; }

        public virtual DataContainerType ContainerType { get; set; }
        public virtual string EntitySelectionPredicate { get; set; } // only if Type == ReferenceType

        /// <summary>
        /// This is a workaroung according to NHibernate's Lazy loading proxy creation!
        /// It should not be mapped!
        /// </summary>
        public virtual DataContainer Self { get { return this; } }

        #endregion

        #region Associations

        public virtual DataType DataType { get; set; } //1
        public virtual Unit Unit { get; set; } // 0..1
        public virtual MeasurementScale MeasurementScale { get; set; }
        public virtual Methodology Methodology { get; set; } //0..1        

        public virtual ICollection<Constraint> Constraints { get; set; }
        public virtual ICollection<ExtendedProperty> ExtendedProperties { get; set; }
        public virtual ICollection<GlobalizationInfo> GlobalizationInfos { get; set; }
        public virtual ICollection<AggregateFunction> AggregateFunctions { get; set; }

        #endregion
        
        #region Methods
        
        public DataContainer()
        {
            Constraints = new List<Constraint>();
            ExtendedProperties = new List<ExtendedProperty>();
            GlobalizationInfos = new List<GlobalizationInfo>();
            AggregateFunctions = new List<AggregateFunction>();
            IsMultiValue = false;
            ContainerType = DataContainerType.ValueType;
            EntitySelectionPredicate = string.Empty;
            MeasurementScale = MeasurementScale.Nominal;
        }

        public override void Validate()
        {
            // No dupplicate extended property
            // if Type == ReferenceType, EntitySelectionpredicate can not be null
            // must have one data type


            throw new NotImplementedException();
        }

        #endregion
    }

}
