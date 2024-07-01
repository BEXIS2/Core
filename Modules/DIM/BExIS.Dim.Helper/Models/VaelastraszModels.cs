using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BExIS.Dim.Helpers.Models
{
    public class VaelastraszConfigurationItem
    {
        public VaelastraszConfigurationItem(string name, string type, string value, string extra = null)
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
