using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BExIS.Ddm.Providers.OpenSearch.Config.enums;
using Newtonsoft.Json;

namespace BExIS.Ddm.Providers.OpenSearch.Config
{
    public class GlobalSpatialSearchSettings
    {
        [JsonProperty("crs")]
        [JsonConverter(typeof(StringToEnumConverter<CrsType>))]
        public CrsType Crs { get; set; }
        [JsonProperty("axis_order")]
        public List<string> AxisOrder { get; set; }
        [JsonProperty("basemap")]
        public BasemapType Basemap { get; set; }
        [JsonProperty("start_extend")]
        public string StartExtend { get; set; }

        public GlobalSpatialSearchSettings()
        {
            AxisOrder = new List<string>();
        }
    }
}
