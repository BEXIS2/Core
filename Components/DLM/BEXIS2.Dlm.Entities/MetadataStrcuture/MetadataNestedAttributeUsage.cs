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
    public class MetadataNestedAttributeUsage : BaseUsage
    {
        #region Associations

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        public virtual MetadataCompoundAttribute Master { get; set; }

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        public virtual MetadataAttribute Member { get; set; }

        #endregion Associations
    }
}