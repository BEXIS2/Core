using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BExIS.Ddm.Providers.OpenSearch.Config
{
    public class LocalPrimaryData : IPrimaryData
    {
        [JsonProperty("to_index")]
        public bool ToIndex { get; set; }

        [JsonProperty("calc")]
        public List<PrimaryDataAggregation> Calc { get; set; } = new List<PrimaryDataAggregation>();
    }
}
