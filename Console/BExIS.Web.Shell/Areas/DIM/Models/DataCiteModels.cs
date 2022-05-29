using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BExIS.Modules.Dim.UI.Models
{
    public class DataCiteSettings
    {
        public List<DataCiteMapping> Mappings { get; set; }
    }

    public class DataCiteMapping
    {
        public string Key { get; set; }
        public string Type { get; set; }
        public string Value { get; set; }
        public string Party { get; set; }

        public DataCiteMapping(string key, string type, string value, string party = null)
        {
            Key = key;
            Type = type;
            Value = value;
            Party = party;
        }
    }
}