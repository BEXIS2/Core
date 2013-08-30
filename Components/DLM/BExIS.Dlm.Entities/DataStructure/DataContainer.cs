using System.Collections.Generic;
using System.Linq;
using Vaiona.Entities.Common;

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
        
        public virtual bool IsBuiltIn { get; set; } // the build in containers can not be deleted, except by their owner
        // Owner of the container is the module who created it 
        // or the RPM (Research Planning Module) if its created via the web interface
        // or DCM (Data Collection Module) if its created during data submission
        public virtual string Owner { get; set; } 

        public virtual DataContainerType ContainerType { get; set; }
        public virtual MeasurementScale MeasurementScale { get; set; }

        public virtual string EntitySelectionPredicate { get; set; } // only if Type == ReferenceType

        /// <summary>
        /// This is a workaround according to NHibernate's Lazy loading proxy creation!
        /// It should not be mapped!
        /// </summary>
        public virtual DataContainer Self { get { return this; } }

        #endregion

        #region Associations

        public virtual DataType DataType { get; set; } //1
        public virtual Unit Unit { get; set; } // 0..1
        public virtual Methodology Methodology { get; set; } //0..1        

        public virtual ICollection<Constraint> Constraints { get; set; }
        public virtual ICollection<ExtendedProperty> ExtendedProperties { get; set; }
        public virtual ICollection<GlobalizationInfo> GlobalizationInfos { get; set; }
        public virtual ICollection<AggregateFunction> AggregateFunctions { get; set; }

        // not to map
        public virtual IList<DomainValueConstraint> DomainValues
        { 
            get 
            {
                return (this.Constraints.Distinct()
                            .Where(p => p.GetType().Equals(typeof(DomainValueConstraint)))
                            .ToList() as IList<DomainValueConstraint>
                        );
            }
        }

        // not to map
        public virtual IList<ValidatorConstraint> Validators
        {
            get
            {
                return (this.Constraints.Distinct()
                            .Where(p => p.GetType().Equals(typeof(ValidatorConstraint)))
                            .ToList() as IList<ValidatorConstraint>
                        );
            }
        }
        
        #endregion
        
        #region Methods
        
        public DataContainer()
        {
            IsBuiltIn = false;
            Owner = "RPM";
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

        public virtual bool ValidateDomainValues(object data)
        {
            // data must be of type DataType linked to the container
            // check whether data matches one of the domain values
            return false;
        }

        public virtual bool ValidateValidatores(object data)
        {
            // data must be of type DataType linked to the container
            // run the Evaulate method of all associated validator objects
            return false;
        }

        #endregion
    }

}
