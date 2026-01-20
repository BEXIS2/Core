using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace BExIS.Ddm.Providers.OpenSearch.Config
{
    public class GlobalSearchComponent
    {
        [JsonProperty("facets_to_index")]
        public bool FacetsToIndex { get; set; }
        [JsonProperty("categories_to_index")]
        public bool CategoriesToIndex { get; set; }
        [JsonProperty("properties_to_index")]
        public bool PropertiesToIndex { get; set; }
        [JsonProperty("general_to_index")]
        public bool GeneralsToIndex { get; set; }

        [JsonProperty("facets")]
        public List<GlobalComponent> Facets { get; set; }
        [JsonProperty("categories")]
        public List<GlobalComponent> Categories { get; set; }
        [JsonProperty("properties")]
        public List<GlobalComponent> Properties { get; set; }
        [JsonProperty("general")]
        public List<GlobalComponent> General { get; set; }

        public GlobalSearchComponent()
        {
            Facets = new List<GlobalComponent>();
            Categories = new List<GlobalComponent>();
            Properties = new List<GlobalComponent>();
            General = new List<GlobalComponent>();
        }
    }

}
