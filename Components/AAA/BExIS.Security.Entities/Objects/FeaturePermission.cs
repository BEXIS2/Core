using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

/// <summary>
///
/// </summary>        
namespace BExIS.Security.Entities.Objects
{
    /// <summary>
    ///
    /// </summary>
    /// <remarks></remarks>        
    public class FeaturePermission : Permission
    {
        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>        
        public virtual Feature Feature { get; set; }
    }
}
