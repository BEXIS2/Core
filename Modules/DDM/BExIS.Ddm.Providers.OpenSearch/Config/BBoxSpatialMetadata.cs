using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BExIS.Ddm.Providers.OpenSearch.Config.enums;
using Newtonsoft.Json;

namespace BExIS.Ddm.Providers.OpenSearch.Config
{
    public sealed class BBoxSpatialMetadata : SpatialMetadata
    {
        // bbox
        [JsonProperty("WestBoundLongitude")]
        public string WestBoundLongitude { get; set; }
        [JsonProperty("EastBoundLongitude")]
        public string EastBoundLongitude { get; set; }
        [JsonProperty("SouthBoundLatitude")]
        public string SouthBoundLatitude { get; set; }
        [JsonProperty("NorthBoundLatitude")]
        public string NorthBoundLatitude { get; set; }
    }
}
