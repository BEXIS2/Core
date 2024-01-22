using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BExIS.Utils.Config.Configurations
{
    public class LdapConfiguration
    {
        [JsonProperty("host")]
        public string Host { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("port")]
        public int Port { get; set; }

        [JsonProperty("version")]
        public int Version { get; set; }

        [JsonProperty("ssl")]
        public bool Ssl { get; set; }

        [JsonProperty("authType")]
        public int AuthType { get; set; }

        [JsonProperty("scope")]
        public int Scope { get; set; }

        [JsonProperty("baseDn")]
        public string BaseDn { get; set; }

        [JsonProperty("identifier")]
        public string Identifier { get; set; }

        [JsonProperty("enabled")]
        public bool Enabled { get; set; }
    }
}
