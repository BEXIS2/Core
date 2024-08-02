using System.Collections.Generic;
using Vaiona.Entities.Common;

/// <summary>
///
/// </summary>
namespace BExIS.Dlm.Entities.DataStructure
{
    /// <summary>
    ///
    /// </summary>
    /// <remarks></remarks>
    public class Classifier : BaseEntity
    {
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
        public virtual Classifier Parent { get; set; }

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        public virtual ICollection<Classifier> Children { get; set; }

        //public virtual string Lineage { get; set; }
        //public virtual string LineageId { get; set; } // needs more design!
    }
}