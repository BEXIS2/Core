using System;
using BExIS.Dlm.Entities.DataStructure;

/// <summary>
///
/// </summary>        
namespace BExIS.Dlm.Entities.Data
{
    /// <summary>
    ///
    /// </summary>
    /// <remarks></remarks>        
    public abstract class DataValue //: BaseEntity
    {
        #region Attributes

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>        
        public object Value { get; set; }

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>        
        public DateTime SamplingTime { get; set; }

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>        
        public DateTime ResultTime { get; set; }

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>        
        public ObtainingMethod ObtainingMethod { get; set; }

        /// <summary>
        /// any free note. especially in case of ObtainingMethod == Processing or Simulation, the process, formula or simulation model can be described here
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>        
        public string Note { get; set; }

        #endregion

        #region Associations

        #endregion

        #region Methods

        #endregion

    }
}
