
using System.Collections.Generic;
using BExIS.Dlm.Entities.Meanings;
using System.Runtime.Remoting.Activation;
using BExIS.Modules.Rpm.UI.Models.Dimensions;
using BExIS.Modules.Rpm.UI.Models.Dimensions;
using System.Collections.Generic;


namespace BExIS.Modules.Rpm.UI.Models.Units
{
    public class UnitListItem
    {
        /// <summary>
        /// Name of the Unit
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
        /// Dimension
        /// </summary>
        public DimensionListItem Dimension { get; set; }

        /// <summary>
        /// Datatypes
        /// </summary>
        public List<DataTypeListItem> Datatypes { get; set; }

        /// <summary>
        /// MeasurementSystem
        /// </summary>
        public string MeasurementSystem { get; set; }

        public LinkItem Link{ get; set; }

        public bool InUse { get; set; }

        public UnitListItem()
        {
            Id = 0;
            Name = string.Empty;
            Description = string.Empty;
            Abbreviation = string.Empty;
            Dimension = new DimensionListItem();
            Datatypes = new List<DataTypeListItem>();
            InUse = false;
            Link = new LinkItem();
        }
    }

    public class LinkItem
    {
        public long Id { get; set; }
        public string URI { get; set; }
        public string Name { get; set; }

        public LinkItem()
        {
            Id = 0;
            URI = string.Empty;
            Name = string.Empty;            
        }
    }

    public class DataTypeListItem
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
        /// SystemType
        /// </summary>
        public string SystemType { get; set; }

        public DataTypeListItem()
        {
            Id = 0;
            Name = string.Empty;
            Description = string.Empty;
            SystemType = string.Empty;
        }
    }
}