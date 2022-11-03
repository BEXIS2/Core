using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BExIS.Dim.Helpers.Models
{
    public class DataCiteSettings
    {
        public List<DataCiteSettingsItem> Mappings { get; set; }
        public List<DataCiteSettingsItem> Placeholders { get; set; }
    }

    public class DataCiteSettingsItem
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public string Value { get; set; }
        public string Extra { get; set; }

        public DataCiteSettingsItem(string name, string type, string value, string extra = null)
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
