using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BExIS.Modules.Ddm.UI.Models
{
    public class TagInfoModel
    {
        public long VersionId { get; set; }
        public long VersionNr { get; set; }
        public long TagId { get; set; }
        public double TagNr { get; set; }
        public bool Show { get; set; }
        public bool Publish { get; set; }
        public string ReleaseNote { get; set; }
        public DateTime ReleaseDate { get; set; }
        public string SystemDescription { get; set; }
        public string SystemAuthor { get; set; }
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