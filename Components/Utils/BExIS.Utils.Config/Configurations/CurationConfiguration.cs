using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace BExIS.Utils.Config.Configurations
{
    public class CurationLabel
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("color")]
        public string Color { get; set; }

        public CurationLabel(string name, string color)
        {
            this.Name = name;
            this.Color = color;
        }
    }

    public class CurationConfiguration
    {
        [JsonProperty("curationLabels")]
        public List<CurationLabel> CurationLabels { get; set; }
    }
}
