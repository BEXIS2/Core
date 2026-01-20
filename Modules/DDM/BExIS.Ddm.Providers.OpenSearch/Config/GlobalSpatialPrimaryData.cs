using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace BExIS.Ddm.Providers.OpenSearch.Config
{
    public class GlobalSpatialPrimaryData
    {
        [JsonProperty("allowed_data_type_ids")]
        public List<int> AllowedDataTypeIds { get; set; }
        [JsonProperty("lat_meaning")]
        public int LatMeaning { get; set; }
        [JsonProperty("long_meaning")]
        public int LongMeaning { get; set; }

        public GlobalSpatialPrimaryData()
        {
            AllowedDataTypeIds = new List<int>();
        }
    }
}
