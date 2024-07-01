using System.Collections.Generic;
using Vaiona.Entities.Common;

/// <summary>
///
/// </summary>
namespace BExIS.Dlm.Entities.MetadataStructure
{
    /// <summary>
    ///
    /// </summary>
    /// <remarks></remarks>
    public class MetadataPackage : BusinessEntity
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
        /// <remarks></remarks>
        /// <seealso cref=""/>
        public virtual bool IsEnabled { get; set; }

        #endregion Attributes

        #region Associations

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        public virtual ICollection<MetadataPackageUsage> UsedIn { get; set; }

        /// <summary>
        ///
        /// </summary>
        /// <remarks> needs to preserve the order </remarks>
        /// <seealso cref=""/>
        public virtual ICollection<MetadataAttributeUsage> MetadataAttributeUsages { get; set; }

        #endregion Associations

        #region Mathods

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param>NA</param>
        public MetadataPackage()
        {
            UsedIn = new List<MetadataPackageUsage>();
            MetadataAttributeUsages = new List<MetadataAttributeUsage>();
        }

        #endregion Mathods
    }
}