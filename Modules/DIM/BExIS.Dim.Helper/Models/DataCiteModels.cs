using System.Collections.Generic;

namespace BExIS.Dim.Helpers.Models
{
    public class DataCiteDOISettings
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public string Host { get; set; }

        public List<DataCiteDOISettingsItem> Mappings { get; set; }
        public List<DataCiteDOISettingsItem> Placeholders { get; set; }
    }

    public class DataCiteDOISettingsItem
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public string Value { get; set; }
        public string Extra { get; set; }

        public DataCiteDOISettingsItem(string name, string type, string value, string extra = null)
        {
            Name = name;
            Type = type;
            Value = value;
            Extra = extra;
        }
    }

    //public class DataCiteMapping
    //{
    //    public string Name { get; set; }
    //    public string Type { get; set; }
    //    public string Value { get; set; }
    //    public bool UseParty { get; set; }
    //    public Dictionary<string, string> PartyAttributes { get; set; }

    //    public DataCiteMapping(string name, string type, string value, bool useParty = false, Dictionary<string, string> partyAttributes = null)
    //    {
    //        Name = name;
    //        Type = type;
    //        Value = value;
    //        UseParty = useParty;
    //        PartyAttributes = partyAttributes;
    //    }
    //}
}
