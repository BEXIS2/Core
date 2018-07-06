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
    }
}