using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BExIS.Web.Shell.Areas.DIM.Models
{
    public class MetadataStructureModel
    {
        public string Displayname { get; set; }
        public long Id { get; set; }

        public string Description { get; set; }

        public List<DatasetVersionModel> DatasetVersions { get; set; }

        public MetadataStructureModel(long id, string displayName, string description, List<DatasetVersionModel> datasetVersions)
        {
            this.Id = id;
            this.Displayname = displayName;
            this.Description = description;
            this.DatasetVersions = datasetVersions;
        }

        public void AddMetadataPath(long id, string path)
        {
            this.DatasetVersions.Where(d => d.Id.Equals(id)).FirstOrDefault().MetadataDownloadPath = path;
        }
    }
}