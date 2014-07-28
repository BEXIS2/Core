

/// <summary>
///
/// </summary>        
namespace BExIS.Io.Transform.Input
{

    /// <summary>
    ///
    /// </summary>
    /// <remarks></remarks>            
    public class ExcelFileReaderInfo:FileReaderInfo
    {
        /// <summary>
        /// 
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>        
        //public Orientation Orientation { get; set; }

        public ExcelFileReaderInfo()
        {
            this.Decimal = DecimalCharacter.point;
        }

    }
}
