using System.Collections.Generic;
using Vaiona.Entities.Common;
using BExIS.Dlm.Entities.Meanings;

/// <summary>
///
/// </summary>        
namespace BExIS.Dlm.Entities.DataStructure
{
    /// <summary>
    /// 
    /// </summary>
    public enum MeasurementSystem
    {
        Unknown, Metric, Imperial, Nautical, Natural
    }

    /// <summary>
    ///
    /// </summary>
    /// <remarks></remarks>        
    public class Unit : BaseEntity
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
        public virtual string Abbreviation { get; set; }

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>        
        public virtual string Description { get; set; }


        /// <summary>
        ///
        /// </summary>
        /// <remarks> Metric, Imperial, etc </remarks>
        /// <seealso cref=""/>        
        public virtual MeasurementSystem MeasurementSystem { get; set; }

        #endregion

        #region Associations

        /// <summary>
        ///
        /// </summary>
        /// <remarks> L: Length, M: Mass, A: Area, L(0)T(-1) </remarks>
        /// <seealso cref=""/>        
        public virtual Dimension Dimension { get; set; }

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>        
        public virtual ICollection<DataContainer> DataContainers { get; set; }

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>        
        public virtual ICollection<ConversionMethod> ConversionsIamTheSource { get; set; } // ConversionMethod holds the relationship (FK)

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>        
        public virtual ICollection<ConversionMethod> ConversionsIamTheTarget { get; set; }

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>        
        public virtual ICollection<DataType> AssociatedDataTypes { get; set; } // datatype controls the relationship

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>  
        public virtual ExternalLink ExternalLink { get; set; }

        #endregion

        #region Mathods

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param>NA</param>       
        public Unit()
        {
            DataContainers = new List<DataContainer>();
            ConversionsIamTheSource = new List<ConversionMethod>();
            ConversionsIamTheTarget = new List<ConversionMethod>();
            MeasurementSystem = MeasurementSystem.Unknown;
            AssociatedDataTypes = new List<DataType>();
        }

        #endregion

    }
}
