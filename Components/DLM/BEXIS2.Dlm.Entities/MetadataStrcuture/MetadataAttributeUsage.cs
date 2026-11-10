using BEXIS2.Dlm.Entities.Common;

/// <summary>
///
/// </summary>
namespace BEXIS2.Dlm.Entities.MetadataStructure
{
    /// <summary>
    ///
    /// </summary>
    /// <remarks></remarks>
    public class MetadataAttributeUsage : BaseUsage
    {
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

        #endregion Associations
    }
}