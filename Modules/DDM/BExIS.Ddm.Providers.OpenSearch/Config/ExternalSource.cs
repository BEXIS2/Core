using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace BExIS.Ddm.Providers.OpenSearch.Config
{
    public class ExternalSource
    {
        [JsonProperty("source")]
        public string Source { get; set; }

        [JsonProperty("local_path")]
        public string LocalPath { get; set; }

        [JsonProperty("external_name")]
        public string ExternalName { get; set; }
    }

}
