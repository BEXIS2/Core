using BExIS.Dlm.Entities.Data;
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
    public class DatasetView : BaseEntity
    {
        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        public virtual string Name { get; set; }

        /// <summary>
        /// to select which tuples are visible trough the view
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        public virtual string ContentSelectionCriterion { get; set; }

        /// <summary>
        /// to select which variables or parameters are visisble through the view
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        public virtual string ContainerSelectionCriterion { get; set; }

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        public virtual Dataset Dataset { get; set; }

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        public virtual ICollection<DataStructure> DataStructures { get; set; }
    }
}