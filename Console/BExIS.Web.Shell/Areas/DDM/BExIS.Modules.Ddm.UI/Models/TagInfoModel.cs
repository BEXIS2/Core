using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BExIS.Modules.Ddm.UI.Models
{
    public class TagInfoModel
    {
        [JsonProperty("versionId")]
        public long VersionId { get; set; }

        [JsonProperty("versionNr")]
        public long VersionNr { get; set; }
        [JsonProperty("show")]
        public bool Show { get; set; }
        [JsonProperty("publish")]
        public bool Publish { get; set; }

        [JsonProperty("tagId")]
        public long TagId { get; set; }
        [JsonProperty("tagNr")]
        public double TagNr { get; set; }

        [JsonProperty("releaseNote")]
        public string ReleaseNote { get; set; }
        [JsonProperty("releaseDate")]
        public DateTime ReleaseDate { get; set; }
        [JsonProperty("systemDescription")]
        public string SystemDescription { get; set; }
        [JsonProperty("systemAuthor")]
        public string SystemAuthor { get; set; }
        [JsonProperty("systemDate")]

        public DateTime SystemDate { get; set; }



        public TagInfoModel()
        {
            VersionId = 0;
            VersionNr = 0;
            TagId = 0;
            TagNr = 0;
            ReleaseNote = "";
            SystemDescription = "";
            SystemAuthor = "";
        }   

        public TagInfoModel(long versionId, long versionNr, double tagNr, string releaseNote, DateTime releaseDate, string systemDescription, string systemAuthor, DateTime systemDate)
        {
            VersionId = versionId;
            VersionNr = versionNr;
            TagNr = tagNr;
            ReleaseNote = releaseNote;
            ReleaseDate = releaseDate;
            SystemDescription = systemDescription;
            SystemAuthor = systemAuthor;
            SystemDate = systemDate;
        }
    }
}