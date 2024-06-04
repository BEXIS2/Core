using Newtonsoft.Json;

namespace BExIS.Web.Shell.Models
{
    public class ReadVersionsModel
    {
        [JsonProperty("site")]
        public string Site { get; set; }

        [JsonProperty("workspace")]
        public string Workspace { get; set; }

        [JsonProperty("database")]
        public string Database { get; set; }
    }
}