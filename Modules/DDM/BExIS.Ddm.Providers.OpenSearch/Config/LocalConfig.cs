using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace BExIS.Ddm.Providers.OpenSearch.Config
{
    public class LocalConfig
    {
        [JsonProperty("entity_template_id")]
        public int EntityTemplateId { get; set; }
        [JsonProperty("index_not_completed_metadata")]
        public bool IndexNotCompletedMetadata { get; set; }
        [JsonProperty("search_components")]
        public LocalSearchComponent SearchComponents { get; set; }
        [JsonProperty("spatial_data")]
        public LocalSpatialData SpatialData { get; set; }
        [JsonProperty("primary_data")]
        public LocalPrimaryData PrimaryData { get; set; }
        [JsonProperty("external_sources")]
        public ExternalSource ExternalSource { get; set; }
    }

}
