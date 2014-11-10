using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BExIS.Web.Shell.Areas.DIM.Models
{
    public class DatasetVersionModel
    {
        public long DatasetVersionId { get; set; }
        public long DatasetId { get; set; }
        public string Title { get; set; }
        public string MetadataDownloadPath { get; set; }

        public DatasetVersionModel()
        {
            DatasetId = 0;
            DatasetVersionId = 0;
            Title = "";
            MetadataDownloadPath = "";
        }
    }
}