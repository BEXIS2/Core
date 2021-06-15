using BExIS.Dlm.Entities.Data;

namespace BExIS.Modules.Sam.UI.Models
{
    public struct DatasetStatModel
    {
        public long Id { get; set; }
        public bool IsSynced { get; set; }
        public long NoOfCols { get; set; }
        public long NoOfRows { get; set; }
        public DatasetStatus Status { get; set; }
        public string Title { get; set; }
        public string ValidState { get; set; }
        public string CreationDate { get; set; }
        public string LastChange { get; set; }
        public string LastChangeType { get; set; }
        public string LastChangeDescription { get; set; }
        public string LastChangeAccount { get; set; }
        public string LastMetadataChange { get; set; }
        public string LastDataChange { get; set; }


    }
}