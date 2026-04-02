using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BExIS.Modules.Smm.UI.Models
{
    public class HeaderMappingsModel
    {
        public List<MappingEntry> Mappings { get; set; } = new List<MappingEntry>();

        public long DatastructureId { get; set; }

        [Range(1, long.MaxValue, ErrorMessage = "DatasetId must be provided and greater than 0.")]
        public long DatasetId { get; set; }

        // Returns the VariableId of the mapping entry whose HeaderMapping equals
        // "scientificName". If no such entry exists the method returns null.
        public long? GetVariableIdForScientificName()
        {
            var entry = Mappings?.FirstOrDefault(m => string.Equals(m.HeaderMapping, "scientificName", StringComparison.OrdinalIgnoreCase));
            return entry?.VariableId;
        }
    }

    public class MappingEntry
    {
        public long VariableId { get; set; }

        public string VariableName { get; set; }

        public string HeaderMapping { get; set; }
    }

    public static class MappingValidator
    {
        private static readonly HashSet<string> ValidOptions = new HashSet<string>
        {
            "scientificName",
            "authorship",
            "rank",
            "kingdom",
            "IGNORE"
        };

        public static bool IsValid(string value)
        {
            return ValidOptions.Contains(value);
        }
    }
}