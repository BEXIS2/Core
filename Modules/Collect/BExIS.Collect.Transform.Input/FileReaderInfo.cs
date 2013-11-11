
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



        
        public static string GetDecimalAsString(DecimalCharacter decimalCharacter)
        {
            switch (decimalCharacter)
            {
                case DecimalCharacter.comma:
                    return DecimalCharacter.comma.ToString();
                case DecimalCharacter.point:
                    return DecimalCharacter.point.ToString();
                default: return DecimalCharacter.point.ToString();
            }
        }

        public static DecimalCharacter GetDecimalCharacter(string decimalCharacter)
        {
            switch (decimalCharacter)
            {
                case "comma":
                    return DecimalCharacter.comma;
                case "point":
                    return DecimalCharacter.point;
                default: return DecimalCharacter.point;
            }
        }

        public static string GetOrientationAsString(Orientation orientation)
        {
            switch (orientation)
            {
                case Orientation.columnwise:
                    return Orientation.columnwise.ToString();
                case Orientation.rowwise:
                    return Orientation.rowwise.ToString();
                default: return Orientation.columnwise.ToString();
            }
        }

        public static Orientation GetOrientation(string orientation)
        {
            switch (orientation)
            {
                case "columnswise":
                    return Orientation.columnwise;
                case "rowwise":
                    return Orientation.rowwise;
                default: return Orientation.columnwise;
            }
        }


    }

  
    
}
