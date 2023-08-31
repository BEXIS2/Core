using BExIS.UI.Models;
using System.Collections.Generic;

namespace BExIS.Modules.Rpm.UI.Models.Dimensions
{
    public class ConstraitListItem
    {

        /// <summary>
        /// Name of the Dimension
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// Name of the Dimension
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Description of the Dimension
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Specification of the Dimension
        /// </summary>
        public string Specification { get; set; }

        public bool InUse { get; set; }

        public ConstraitListItem()
        {
            Id = 0;
            Name = string.Empty;
            Description = string.Empty;
            Specification = string.Empty;
            InUse = false;
        }
    }
}