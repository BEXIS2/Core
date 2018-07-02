﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml;

namespace BExIS.Modules.Ddm.UI.Models
{
    public class ShowDataModel
    {
        public long Id { get; set; }

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
    }
}