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
    public class MetadataParameterUsage : BaseUsage
    {
        #region Associations

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        public virtual MetadataAttribute Master { get; set; }

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        public virtual MetadataParameter Member { get; set; }

        #endregion Associations

        public MetadataParameterUsage()
        {
            DefaultValue = "";
            FixedValue = "";
        }
    }
}