using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BExIS.Core.Data;

namespace BExIS.Dlm.Entities.DataStructure
{
    public enum MeasurementSystem
    {
        Metric, Imperial, Unknown
    }

    public class Unit : BaseEntity
    {
        #region Attributes

        public virtual string Name { get; set; }
        public virtual string Abbreviation { get; set; }
        public virtual string Description { get; set; }
        public virtual string Dimension { get; set; } // L: Length, M: Mass, A: Area, LT-1
        public virtual MeasurementSystem MeasurementSystem { get; set; } // Metric, Imperial, etc

        #endregion

        #region Associations

        public virtual ICollection<DataContainer> DataContainers { get; set; }
        public virtual ICollection<ConversionMethod> ConversionsIamTheSource{ get; set; } // ConversionMethod holds the relationship (FK)
        public virtual ICollection<ConversionMethod> ConversionsIamTheTarget { get; set; }
        public virtual ICollection<DataType> AssociatedDataTypes { get; set; } // datatype controls the relationship
        
        #endregion

        #region Mathods

        public Unit()
        {
            DataContainers = new List<DataContainer>();
            ConversionsIamTheSource = new List<ConversionMethod>();
            ConversionsIamTheTarget = new List<ConversionMethod>();
            MeasurementSystem = MeasurementSystem.Metric;
            AssociatedDataTypes = new List<DataType>();
        }

        #endregion

    }
}
