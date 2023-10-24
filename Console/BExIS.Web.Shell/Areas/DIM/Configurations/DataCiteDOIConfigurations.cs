using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BExIS.Modules.Dim.UI.Configurations
{
    public class DataCiteDOICredentials
    {
        [JsonProperty("host")]
        public string Host { get; set; }

        [JsonProperty("password")]
        public string Password { get; set; }

        [JsonProperty("username")]
        public string Username { get; set; }
    }

    public class DataCiteDOIMappings
    {
        [JsonProperty("url")]
        public string URL { get; set; }

        [JsonProperty("version")]
        public string Version { get; set; }
    }

    public class DataCiteDOIPlaceholders
    {
        [JsonProperty("datasetId")]
        public string DatasetId { get; set; }

        [JsonProperty("versionId")]
        public string VersionId { get; set; }

        [JsonProperty("versionName")]
        public string VersionName { get; set; }

        [JsonProperty("versionNumber")]
        public string VersionNumber { get; set; }
    }
}