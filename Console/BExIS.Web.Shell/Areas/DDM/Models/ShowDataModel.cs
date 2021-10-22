﻿namespace BExIS.Modules.Ddm.UI.Models
{
    public class ShowDataModel
    {
        public long Id { get; set; }
        public long VersionId { get; set; }
        public int Version { get; set; }
        public int VersionSelect { get; set; }
        public bool LatestVersion { get; set; }

        public long MetadataStructureId { get; set; }
        public long DataStructureId { get; set; }
        public long ResearchPlanId { get; set; }

        public string Title { get; set; }
        public bool ViewAccess { get; set; }
        public bool GrantAccess { get; set; }
        public bool IsCheckedIn { get; set; }

        public string DataStructureType { get; set; }
        public bool DownloadAccess { get; set; }

        public bool RequestExist { get; set; }
        public bool RequestAble { get; set; }
        public bool HasRequestRight { get; set; }

        public bool IsPublic { get; set; }
    }
}
