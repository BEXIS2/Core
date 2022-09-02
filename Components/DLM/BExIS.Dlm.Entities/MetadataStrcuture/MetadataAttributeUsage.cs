
using BExIS.Dlm.Entities.Common;

/// <summary>
///
/// </summary>        
namespace BExIS.Dlm.Entities.MetadataStructure
{
    /// <summary>
    ///
    /// </summary>
    /// <remarks></remarks>        
    public class MetadataAttributeUsage : BaseUsage
    {
        #region Attributes

        #endregion

        #region Associations

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>        
        public virtual MetadataPackage MetadataPackage { get; set; }

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>        
        public virtual MetadataAttribute MetadataAttribute { get; set; }

        #endregion

        #region Mathods

        #endregion

    }
}
