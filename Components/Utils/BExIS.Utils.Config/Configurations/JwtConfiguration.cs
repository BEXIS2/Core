﻿using Newtonsoft.Json;

namespace BExIS.Utils.Config.Configurations
{
    public class JwtConfiguration
    {
        [JsonProperty("issuerSigningKey")]
        public string IssuerSigningKey { get; set; }

        [JsonProperty("requireExpirationTime")]
        public bool RequireExpirationTime { get; set; }

        [JsonProperty("validateAudience")]
        public bool ValidateAudience { get; set; }

        [JsonProperty("validateIssuer")]
        public bool ValidateIssuer { get; set; }

        [JsonProperty("validateIssuerSigningKey")]
        public bool ValidateIssuerSigningKey { get; set; }

        [JsonProperty("validateLifetime")]
        public bool ValidateLifetime { get; set; }

        [JsonProperty("validAudience")]
        public string ValidAudience { get; set; }

        [JsonProperty("validIssuer")]
        public string ValidIssuer { get; set; }

        [JsonProperty("validLifetime")]
        public int ValidLifetime { get; set; }
    }
}