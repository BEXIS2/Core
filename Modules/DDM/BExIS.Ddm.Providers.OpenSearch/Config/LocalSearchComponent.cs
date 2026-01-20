using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace BExIS.Ddm.Providers.OpenSearch.Config
{
    public class LocalSearchComponent
    {
        [JsonProperty("facets")]
        public List<LocalComponent> Facets { get; set; }
        [JsonProperty("categories")]
        public List<LocalComponent> Categories { get; set; }
        [JsonProperty("properties")]
        public List<LocalComponent> Properties { get; set; }
        [JsonProperty("general")]
        public List<LocalComponent> General { get; set; }

        public LocalSearchComponent()
        {
            Facets = new List<LocalComponent>();
            Categories = new List<LocalComponent>();
            Properties = new List<LocalComponent>();
            General = new List<LocalComponent>();
        }
    }


}
