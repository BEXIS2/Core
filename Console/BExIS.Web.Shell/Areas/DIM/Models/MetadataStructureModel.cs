using System.Collections.Generic;
using System.Linq;

namespace BExIS.Modules.Dim.UI.Models
{
    public class MetadataStructureModel
    {
        public string Displayname { get; set; }
        public long Id { get; set; }

        public string Description { get; set; }
        public bool ExportIsAvailable { get; set; }

        public List<DatasetVersionModel> DatasetVersions { get; set; }

        public MetadataStructureModel(long id, string displayName, string description, List<DatasetVersionModel> datasetVersions, bool exportIsAvailable)
        {
            this.Id = id;
            this.Displayname = displayName;
            this.Description = description;
            this.DatasetVersions = datasetVersions;
            this.ExportIsAvailable = exportIsAvailable;
        }

        public void AddMetadataPath(long id, string path)
        {
            this.DatasetVersions.Where(d => d.DatasetVersionId.Equals(id)).FirstOrDefault().MetadataDownloadPath = path;
        }
    }
}