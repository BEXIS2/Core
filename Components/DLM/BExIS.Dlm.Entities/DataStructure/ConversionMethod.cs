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
    public class ConversionMethod : BaseEntity
    {
        #region Attributes

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        public virtual string Description { get; set; }

        /// <summary>
        ///
        /// </summary>
        /// <remarks> 100*x => Target = 100*Source </remarks>
        /// <seealso cref=""/>
        public virtual string Formula { get; set; }

        #endregion Attributes

        #region Associations

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        public virtual Unit Source { get; set; }

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        public virtual Unit Target { get; set; }

        #endregion Associations
    }
}