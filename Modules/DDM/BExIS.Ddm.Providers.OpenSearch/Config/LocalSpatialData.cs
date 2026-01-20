using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace BExIS.Ddm.Providers.OpenSearch.Config
{
    public class LocalSpatialData
    {
        [JsonProperty("spatial_metadata")]
        [JsonConverter(typeof(SpatialMetadataConverter))]
        public SpatialMetadata SpatialMetadata { get; set; }
    }
}
