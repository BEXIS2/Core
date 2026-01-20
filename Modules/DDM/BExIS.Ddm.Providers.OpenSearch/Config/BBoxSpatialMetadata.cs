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
        [JsonProperty("west_bound_longitude")]
        public double WestBoundLongitude { get; set; }
        [JsonProperty("east_bound_longitude")]
        public double EastBoundLongitude { get; set; }
        [JsonProperty("south_bound_latitude")]
        public double SouthBoundLatitude { get; set; }
        [JsonProperty("north_bound_latitude")]
        public double NorthBoundLatitude { get; set; }
    }
}
