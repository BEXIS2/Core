using BExIS.Dlm.Entities.Data;
using System.Collections.Generic;
using Vaiona.Entities.Common;

/// <summary>
///
/// </summary>
namespace BExIS.Dlm.Entities.Administration
{
    /// <summary>
    ///
    /// </summary>
    /// <remarks></remarks>
    public class ResearchPlan : BaseEntity
    {
        public ResearchPlan()
        {
            this.Datasets = new List<Dataset>();
        }

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        public virtual string Title { get; set; }

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
        public virtual ICollection<Dataset> Datasets { get; set; }

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        public virtual ICollection<DataStructure.DataStructure> DataStructures { get; set; }

        // Accessibility Policy
        // Execution Unit
        // Metadata Structure
    }
}