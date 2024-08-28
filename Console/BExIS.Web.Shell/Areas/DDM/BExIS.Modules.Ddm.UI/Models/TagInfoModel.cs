using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BExIS.Modules.Ddm.UI.Models
{
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
            SystemDescription = systemDescription;
            SystemAuthor = systemAuthor;
            SystemDate = systemDate;
            Link = link;
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
}