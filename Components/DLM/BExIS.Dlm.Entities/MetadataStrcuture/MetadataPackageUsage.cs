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
    public class MetadataPackageUsage : BaseUsage
    {
        #region Associations

        // its possible for a structure to be connected to a single package more than once, probably in different roles

        /// <summary>
        ///
        /// </summary>
        /// <remarks> intentionally used the full name. otherwise is not shown in the class diagram. </remarks>
        /// <seealso cref=""/>
        public virtual BEXIS2.Dlm.Entities.MetadataStructure.MetadataStructure MetadataStructure { get; set; }

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        public virtual MetadataPackage MetadataPackage { get; set; }

        #endregion Associations
    }
}