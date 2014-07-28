using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using BExIS.Security.Entities.Subjects;
using Vaiona.Entities.Common;

/// <summary>
///
/// </summary>        
namespace BExIS.Security.Entities.Objects
{
    /// <summary>
    ///
    /// </summary>
    /// <remarks></remarks>        
    public abstract class Permission : BaseEntity
    {
        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>        
        public virtual Subject Subject { get; set; }
    }
}
