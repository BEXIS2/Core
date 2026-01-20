using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace BExIS.Ddm.Providers.OpenSearch.Config
{
    public class GlobalGeneral
    {
        [JsonProperty("show_only_completed_metadata")]
        public bool ShowOnlyCompletedMetadata { get; set; }

        [JsonProperty("auto_complete_trigger")]
        public int AutoCompleteTrigger { get; set; }

        [JsonProperty("show_empty_facets")]
        public bool ShowEmptyFacets { get; set; }
    }

}
