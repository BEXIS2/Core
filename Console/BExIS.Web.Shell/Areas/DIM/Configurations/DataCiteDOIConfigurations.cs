using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BExIS.Modules.Dim.UI.Configurations
{
    public class DataCiteDOICredentials
    {
        [JsonProperty("username")]
        public string Username { get; set; }
        [JsonProperty("password")]

        public string Password { get; set; }
        [JsonProperty("host")]

        public string Host { get; set; }
    }

    public class DataCiteDOIConfigurastionItem
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public string Value { get; set; }
        public string Extra { get; set; }
    }
}