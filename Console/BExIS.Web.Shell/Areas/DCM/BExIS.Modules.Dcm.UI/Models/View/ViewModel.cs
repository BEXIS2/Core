using BExIS.UI.Hooks;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace BExIS.Modules.Dcm.UI.Models.View
{
    public class ViewSettingsModel
    {
        public long Id { get; set; }
        public long VersionId { get; set; }
        public int Version { get; set; }
        public string Title { get; set; }

        public bool UseTags { get; set; } // use tags, e.g., 1.0, 2.0, 3.0
        public bool UseMinor { get; set; } // use minor tags, e.g., 1.1, 1.2, 1.3
        public bool HasData { get; set; }

        public string DataAggrement { get; set; }

        public List<Hook> Hooks { get; set; }

        public Dictionary<string, string> Labels { get; set; }

        public ViewSettingsModel()
        {
            Id = 0;
            Version = 0;
            VersionId = 0;
            Title = "";
            Hooks = new List<Hook>();
            UseTags = false;
            HasData = false;
            DataAggrement = "";
            Labels = new Dictionary<string, string>();
        }
    }

    public class TagInfoViewModel
    {
        [JsonProperty("version")]
        public double Version { get; set; }
        [JsonProperty("releaseNotes")]
        public List<string> ReleaseNotes { get; set; }
        [JsonProperty("releaseDate")]
        public DateTime ReleaseDate { get; set; }

        public TagInfoViewModel()
        {
            Version = 0;
            ReleaseNotes = new List<string>();
        }

    }

    public class TagInfoEditModel
    {
        [JsonProperty("versionId")]
        public long VersionId { get; set; }

        [JsonProperty("versionNr")]
        public long VersionNr { get; set; }


        [JsonProperty("releaseNote")]
        public string ReleaseNote { get; set; }
        [JsonProperty("show")]
        public bool Show { get; set; }

        [JsonProperty("tagId")]
        public long TagId { get; set; }

        [JsonProperty("tagNr")]
        public double TagNr { get; set; }

        [JsonProperty("publish")]
        public bool Publish { get; set; }

        [JsonProperty("releaseDate")]
        public DateTime ReleaseDate { get; set; }
        [JsonProperty("systemDescription")]
        public string SystemDescription { get; set; }
        [JsonProperty("systemAuthor")]
        public string SystemAuthor { get; set; }
        [JsonProperty("systemDate")]
        public DateTime SystemDate { get; set; }

        [JsonProperty("link")]
        public string Link { get; set; }

        public TagInfoEditModel()
        {
            VersionId = 0;
            VersionNr = 0;
            TagId = 0;
            TagNr = 0;
            ReleaseNote = "";
            SystemDescription = "";
            SystemAuthor = "";
            Link = "";
        }

        public TagInfoEditModel(long versionId, long versionNr, double tagNr, string releaseNote, DateTime releaseDate, string systemDescription, string systemAuthor, DateTime systemDate, string link)
        {
            VersionId = versionId;
            VersionNr = versionNr;
            TagNr = tagNr;
            ReleaseNote = releaseNote;
            ReleaseDate = releaseDate;
            if (!string.IsNullOrEmpty(systemAuthor)) SystemAuthor = systemAuthor;
            if (!string.IsNullOrEmpty(systemDescription)) SystemAuthor = systemDescription;
            SystemDate = systemDate;
            Link = link;
        }
    }
}