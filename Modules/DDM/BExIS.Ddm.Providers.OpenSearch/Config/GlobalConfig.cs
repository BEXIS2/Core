using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace BExIS.Ddm.Providers.OpenSearch.Config
{
    public class GlobalConfig
    {
        [JsonProperty("search_components")]
        public GlobalSearchComponent SearchComponents { get; set; }
        [JsonProperty("primary_data")]
        public GlobalPrimaryData PrimaryData { get; set; }

        [JsonProperty("spatial_data")]
        public GlobalSpatialData SpatialData { get; set; }

        [JsonProperty("general")]
        public GlobalGeneral General { get; set; }
    }

}
