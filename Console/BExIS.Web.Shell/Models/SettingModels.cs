using Newtonsoft.Json;
using System.Collections.Generic;
using Vaiona.Utils.Cfg;

namespace BExIS.Web.Shell.Models
{
    public class ModulModel
    {
        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }
    }

    public class ReadSettingModel
    {
        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("entries")]
        public List<Entry> Entries { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        public static ReadSettingModel Convert(JsonSettings settings)
        {
            return new ReadSettingModel()
            {
                Id = settings.Id,
                Name = settings.Name,
                Description = settings.Description,
                Entries = settings.Entries
            };
        }
    }

    public class UpdateSettingModel
    {
        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("entries")]
        public List<Entry> Entries { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        public static UpdateSettingModel Convert(JsonSettings settings)
        {
            return new UpdateSettingModel()
            {
                Name = settings.Name,
                Description = settings.Description,
                Entries = settings.Entries
            };
        }
    }
}