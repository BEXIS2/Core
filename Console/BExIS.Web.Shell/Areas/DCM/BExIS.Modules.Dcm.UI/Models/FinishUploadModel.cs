namespace BExIS.Modules.Dcm.UI.Models
{
    public class FinishUploadModel
    {
        public long DatasetId { get; set; }
        public string DatasetTitle { get; set; }
        public string Filename { get; set; }

        public FinishUploadModel()
        {
            DatasetTitle = "";
            Filename = "";
        }
    }
}