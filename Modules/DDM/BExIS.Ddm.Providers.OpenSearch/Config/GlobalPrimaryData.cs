using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace BExIS.Ddm.Providers.OpenSearch.Config
{
    public sealed class GlobalPrimaryData : IPrimaryData
    {
        [JsonProperty("to_index")]
        public bool ToIndex { get; set; }

        [JsonProperty("calc")]
        public List<PrimaryDataAggregation> Calc { get; set; } = new List<PrimaryDataAggregation>();

        [JsonProperty("spatial_data")]
        public GlobalSpatialPrimaryData SpatialData { get; set; }
    }
}
