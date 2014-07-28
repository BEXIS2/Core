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
    public class MetadataPackageUsage: BaseEntity
    {
        #region Attributes

        /// <summary>
        ///
        /// </summary>
        /// <remarks> Min cardinality 0 is interpreted as optional usage, Min can not be negative. not supported in NH </remarks>
        /// <seealso cref=""/>        
        public virtual int MinCardinality { get; set; }

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>        
        public virtual int MaxCardinality { get; set; }

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>        
        public virtual string Label { get; set; }

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>        
        public virtual string Description { get; set; }

        #endregion

        #region Associations

        // its possible for a structure to be connected to a single package more than once, probably in different roles

        /// <summary>
        ///
        /// </summary>
        /// <remarks> intentionally used the full name. otherwise is not shown in the class diagram. </remarks>
        /// <seealso cref=""/>        
        public virtual BExIS.Dlm.Entities.MetadataStructure.MetadataStructure   MetadataStructure   { get; set; }

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>        
        public virtual MetadataPackage                                          MetadataPackage     { get; set; }

        #endregion

        #region Mathods

        #endregion

    }
}
