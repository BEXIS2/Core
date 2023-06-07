using BExIS.UI.Models;
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
        public DimensionItem Dimension { get; set; }

        /// <summary>
        /// Datatypes
        /// </summary>
        public List<DataTypeListItem> Datatypes { get; set; }

        /// <summary>
        /// MeasurementSystem
        /// </summary>
        public string MeasurementSystem { get; set; }

        public UnitListItem()
        {
            Id = 0;
            Name = string.Empty;
            Description = string.Empty;
            Abbreviation = string.Empty;
            Dimension = new DimensionItem();
            Datatypes = new List<DataTypeListItem>();
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

    public class DimensionItem
    {

        /// <summary>
        /// Name of the Entity Template
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// Name of the Entity Template
        /// </summary>
        public string Name { get; set; }

        public DimensionItem()
        {
            Id = 0;
            Name = string.Empty;
        }
    }

    public class ValidationResult
    {
        public bool IsValid { get; set; }
        public List<ValidationItem> ValidationItems { get; set; }
        public ValidationResult()
        {
            IsValid = true;
            ValidationItems = new List<ValidationItem>();
        }
    }
    public class ValidationItem
    {
        public string Name { get; set; }
        public string Message { get; set; }
        public ValidationItem()
        {
            Name = string.Empty;
            Message = string.Empty;
        }
    }
}