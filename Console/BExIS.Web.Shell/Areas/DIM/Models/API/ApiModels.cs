using BExIS.IO;

namespace BExIS.Modules.Dim.UI.Models.API
{
    public class DataApiModel
    {
        public long DatasetId { get; set; }
        public DecimalCharacter DecimalCharacter { get; set; }
        public string[] Columns { get; set; }
        public string[][] Data { get; set; }
    }

    public class PushDataApiModel : DataApiModel
    {

    }

    public class PutDataApiModel : DataApiModel
    {
        public string[] PrimaryKeys { get; set; }

    }

}