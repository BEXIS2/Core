using Newtonsoft.Json;

namespace BExIS.Utils.Config.Configurations
{
    public class SmtpConfiguration
    {
        [JsonProperty("hostName")]
        public string HostName { get; set; }

        [JsonProperty("hostPort")]
        public int HostPort { get; set; }

        [JsonProperty("hostAnonymous")]
        public bool HostAnonymous { get; set; }

        [JsonProperty("hostSecureSocketOptions")]
        public int HostSecureSocketOptions { get; set; }

        [JsonProperty("hostCertificateRevocation")]
        public bool HostCertificateRevocation { get; set; }

        [JsonProperty("accountName")]
        public string AccountName { get; set; }

        [JsonProperty("accountPassword")]
        public string AccountPassword { get; set; }

        [JsonProperty("fromName")]
        public string FromName { get; set; }

        [JsonProperty("fromAddress")]
        public string FromAddress { get; set; }
    }
}