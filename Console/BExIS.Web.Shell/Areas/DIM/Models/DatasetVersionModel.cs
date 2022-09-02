namespace BExIS.Modules.Dim.UI.Models
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