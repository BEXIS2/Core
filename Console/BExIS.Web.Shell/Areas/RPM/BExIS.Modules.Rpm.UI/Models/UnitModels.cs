using BExIS.UI.Models;
using System.Collections.Generic;

namespace BExIS.Modules.Rpm.UI.Models.Units
{
    public class UnitListItem
    {

        /// <summary>
        /// Name of the Entity Template
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// Name of the Entity Template
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Description of the Entity Template
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Abbreviation
        /// </summary>
        public string Abbreviation { get; set; }

        /// <summary>
        /// Abbreviation
        /// </summary>
        public string Dimension { get; set; }

        /// <summary>
        /// Abbreviation
        /// </summary>
        public string Datatypes { get; set; }

        /// <summary>
        /// Abbreviation
        /// </summary>
        public string MeasurementSystem { get; set; }

        public UnitListItem()
        {
            Id = 0;
            Name =  string.Empty;
            Description = string.Empty;
            Abbreviation = string.Empty;
            Dimension = string.Empty;
            Datatypes = string.Empty;
        }
    }
}