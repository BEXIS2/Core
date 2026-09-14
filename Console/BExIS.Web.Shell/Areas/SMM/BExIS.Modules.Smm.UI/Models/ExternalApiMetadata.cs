using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Linq;
using System.Web;

namespace BExIS.Modules.Smm.UI.Models
{
    public class ExternalApiMetadata
    {
        [JsonProperty("clb")]
        public ExternalApiSource Clb { get; set; }
    }

    public class ExternalApiSource
    {
        [JsonProperty("sourceKeyInfo")]
        public List<SourceKeyInfoItem> SourceKeyInfo { get; set; }
    }

    public class SourceKeyInfoItem
    {
        [JsonProperty("sourceKey")]
        public string SourceKey { get; set; }
        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("alias")]
        public string Alias { get; set; }
    }
}