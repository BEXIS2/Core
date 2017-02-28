using BExIS.Dlm.Entities.DataStructure;
using System.Collections.Generic;

/// <summary>
///
/// </summary>        
namespace BExIS.Dlm.Entities.MetadataStructure
{
    /// <summary>
    ///
    /// </summary>
    /// <remarks></remarks>        
    public class MetadataAttribute : DataContainer
    {
        #region Attributes
        // all the required attributes are inherited!
        #endregion

        #region Associations

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>        
        public virtual ICollection<MetadataAttributeUsage> UsedIn { get; set; }

        #endregion

        #region Mathods

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param></param>       
        public MetadataAttribute()
        {
            UsedIn = new List<MetadataAttributeUsage>();
        }

        #endregion

    }
}
