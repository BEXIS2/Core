
namespace BExIS.DCM.Transform.Input
{

    public enum DecimalCharacter
    {
        point,
        comma
    }

    public enum Orientation
    {
        columnwise,
        rowwise
    }


    public class FileReaderInfo
    {

        /// <summary>
        /// representation of decimal 
        /// </summary>
        public DecimalCharacter Decimal { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public Orientation Orientation { get; set; }
        /// <summary>
        /// Number of empty columns (columnwise) or rows (rowwise) before the variables are specified.
        /// </summary>
        public int Offset { get; set; }
        /// <summary>
        /// Row/Column in which the variables are.
        /// </summary>
        public int Variables { get; set; }
        /// <summary>
        /// Row/Column in which the Data are.
        /// </summary>
        public int Data { get; set; }
        /// <summary>
        /// Format of the Date
        /// </summary>
        public string Dateformat { get; set; }
  
    }
}
