namespace IDIV.Modules.Mmm.UI.Models
{
    public class DatasetInfo
    {
        public long DatasetId { get; set; }
        public long DatasetVersionId { get; set; }
        public bool IsLatestVersion { get; set; }
        public bool Downloadable { get; set; }
        public bool Deleteable { get; set; }

        public DatasetInfo()
        {
            this.DatasetId = 0;
            this.DatasetVersionId = 0;
            this.IsLatestVersion = false;
            this.Downloadable = false;
            this.Deleteable = false;
        }

        public DatasetInfo(long datasetId, long datasetVersionId, bool isLatestVersion, bool downloadable, bool deleteable)
        {
            this.DatasetId = datasetId;
            this.DatasetVersionId = datasetVersionId;
            this.IsLatestVersion = isLatestVersion;
            this.Downloadable = downloadable;
            this.Deleteable = deleteable;
        }
    }
}