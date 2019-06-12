using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BExIS.Modules.Dim.UI.Models.Api
{
    public class ApiSimpleDatasetModel
    {
        public long Id { get; set; }
        public List<ApiSimpleDatasetVersionModel> Versions { get; set; }

        public ApiSimpleDatasetModel()
        {
            Versions = new List<ApiSimpleDatasetVersionModel>();
        }
    }

    public class ApiSimpleDatasetVersionModel
    {
        public long Id { get; set; }
        public long Number { get; set; }
    }

    public class ApiDatasetModel
    {
        public long Id { get; set; }
        public long Version { get; set; }
        public long VersionId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public long DataStructureId { get; set; }
        public long MetadataStructureId { get; set; }
        public Dictionary<string, string> AdditionalInformations { get; set; }

        public ApiDatasetModel()
        {
            AdditionalInformations = new Dictionary<string, string>();
        }
    }
}