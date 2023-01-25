
using BExIS.Dlm.Entities.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
    public class MetadataParameterUsage: BaseUsage
    {
        #region Attributes

        #endregion

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

        #endregion

        #region Mathods

        #endregion

    }
}
