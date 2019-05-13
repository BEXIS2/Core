using BExIS.IO;

namespace BExIS.Modules.Dim.UI.Models.API
{
    public enum UpdateMethod
    {
        Append,
        Update
    }

    public class PushDataModel
    {
        public long DatasetId { get; set; }
        public UpdateMethod UpdateMethod { get; set; }
        public int Count { get; set; }
        public DecimalCharacter DecimalCharacter { get; set; }
        public string[] Columns { get; set; }
        public string[] PrimaryKeys { get; set; }
        public string[][] Data { get; set; }
    }
}