/// <summary>
///
/// </summary>
namespace BExIS.IO.Transform.Input
{
    /// <summary>
    ///
    /// </summary>
    /// <remarks></remarks>
    public class ExcelFileReaderInfo : FileReaderInfo
    {
        public int VariablesStartRow { get; set; }

        public int VariablesEndRow { get; set; }

        public int VariablesStartColumn { get; set; }

        public int VariablesEndColumn { get; set; }

        public int DataStartRow { get; set; }

        public int DataEndRow { get; set; }

        public int DataStartColumn { get; set; }

        public int DataEndColumn { get; set; }

        public string WorksheetUri { get; set; }

        public ExcelFileReaderInfo()
        {
            WorksheetUri = "";
        }
    }
}