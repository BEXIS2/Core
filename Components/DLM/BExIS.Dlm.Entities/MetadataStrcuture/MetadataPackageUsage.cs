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
    public class MetadataPackageUsage : BaseUsage
    {
        #region Associations

        // its possible for a structure to be connected to a single package more than once, probably in different roles

        /// <summary>
        ///
        /// </summary>
        /// <remarks> intentionally used the full name. otherwise is not shown in the class diagram. </remarks>
        /// <seealso cref=""/>
        public virtual BExIS.Dlm.Entities.MetadataStructure.MetadataStructure MetadataStructure { get; set; }

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        public virtual MetadataPackage MetadataPackage { get; set; }

        #endregion Associations
    }
}