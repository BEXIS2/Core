using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Vaiona.Utils.Cfg;

namespace BExIS.Ddm.Providers.OpenSearch.Config
{
    public class SearchConfig
    {
        [JsonProperty("global")]
        public GlobalConfig Global { get; set; }
        [JsonProperty("local")]
        public List<LocalConfig> Local { get; set; }

        public SearchConfig()
        {
            Local = new List<LocalConfig>();
        }

        public string ToJson()
        {
            var settings = new JsonSerializerSettings
            {
                Formatting = Formatting.Indented,
                Converters =
            {
                new StringEnumConverter()
            },
                NullValueHandling = NullValueHandling.Ignore
            };

            return JsonConvert.SerializeObject(this, settings);
        }
    }
}
