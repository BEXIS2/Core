using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace BExIS.Ddm.Providers.OpenSearch.Config
{
    public class GlobalComponent
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("component_name")]
        public string ComponentName { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("placeholder")]
        public string Placeholder { get; set; }

        [JsonProperty("header_item")]
        public bool HeaderItem { get; set; }

        [JsonProperty("default_header_item")]
        public bool DefaultHeaderItem { get; set; }
    }
}
