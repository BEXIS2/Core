using System.Collections.Generic;

namespace BExIS.Dim.Helpers.Models
{
    public class DataCiteDOISettings
    {
        public string Host { get; set; }
        public List<DataCiteDOISettingsItem> Mappings { get; set; }
        public string Password { get; set; }
        public List<DataCiteDOISettingsItem> Placeholders { get; set; }
        public string Username { get; set; }
    }

    public class DataCiteDOISettingsItem
    {
        public DataCiteDOISettingsItem(string name, string type, string value, string extra = null)
        {
            Name = name;
            Type = type;
            Value = value;
            Extra = extra;
        }

        public string Extra { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public string Value { get; set; }
    }
}