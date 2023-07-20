using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Vaiona.Utils.Cfg;

namespace BExIS.Web.Shell.Models
{
    public class ReadSettingModel
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("entries")]
        public List<Entry> Entries { get; set; }

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
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("entries")]
        public List<Entry> Entries { get; set; }

        public static UpdateSettingModel Convert(JsonSettings settings)
        {
            return new UpdateSettingModel()
            {
                Id= settings.Id,
                Name = settings.Name,
                Description = settings.Description,
                Entries = settings.Entries
            };
        }
    }

    public class ModulModel
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }
    }
}