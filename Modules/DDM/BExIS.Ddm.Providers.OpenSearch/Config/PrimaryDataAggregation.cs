using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BExIS.Ddm.Providers.OpenSearch.Config.enums;
using Newtonsoft.Json;

namespace BExIS.Ddm.Providers.OpenSearch.Config
{
    public class PrimaryDataAggregation
    {
        [JsonProperty("operation")]
        [JsonConverter(typeof(AggregationOperatorConverter))]
        public AggregationOperator Operation { get; set; }
        [JsonProperty("allowed_meanings")]
        public List<int> AllowedMeanings { get; set; } = new List<int>();
    }
}
