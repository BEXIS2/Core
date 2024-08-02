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
    /// <remarks></remarks>
    public class DataType : BaseEntity
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
        ///
        /// </summary>
        /// <remarks> System.Int32, etc </remarks>
        /// <seealso cref=""/>
        public virtual string SystemType { get; set; }

        #endregion Attributes

        #region Associations

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
        public virtual ICollection<Unit> ApplicableUnits { get; set; }

        #endregion Associations

        #region Mathods

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param>NA</param>
        public DataType()
        {
            DataContainers = new List<DataContainer>();
            ApplicableUnits = new List<Unit>();
        }

        #endregion Mathods
    }
}