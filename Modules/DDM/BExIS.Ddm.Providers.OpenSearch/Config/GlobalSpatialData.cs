using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace BExIS.Ddm.Providers.OpenSearch.Config
{
    public class GlobalSpatialData
    {
        [JsonProperty("spatial_search")]
        public bool SpatialSearch { get; set; }
        [JsonProperty("spatial_search_settings")]
        public GlobalSpatialSearchSettings SpatialSearchSettings { get; set; }
    }
}
