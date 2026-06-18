using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.ComponentModel.DataAnnotations;

namespace BExIS.Modules.Smm.UI.Helpers.MatchingAPIs
{
    public interface IApiOptions { }

    public class ClbOptions : IApiOptions
    {
        [JsonProperty("sourceKey")]
        [Required]
        public string SourceKey { get; set; }

        [JsonProperty("synonyms")]
        public bool Synonyms { get; set; }
    }

    // TODO: - remove (just an example)
    public class GbifOptions : IApiOptions
    {
        [JsonProperty("parameter1")]
        public string Parameter1 { get; set; }

        [JsonProperty("parameter2")]
        public string Parameter2 { get; set; }
    }

    public class GenericOptions : IApiOptions
    {
        public JObject Raw { get; set; }
    }
}
