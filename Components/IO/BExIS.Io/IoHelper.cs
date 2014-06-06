using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BExIS.Io
{
    public class IoHelper
    {

    }

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

    /// <summary>
    /// TextSeperator is a list of different characters which used as seperator in ascii files
    /// </summary>
    /// <remarks></remarks>        
    public enum TextSeperator
    {
        tab,
        comma,
        semicolon,
        space
    }

    /// <summary>
    /// TextMarker is a list of different characters which used as textmarker in ascii files
    /// </summary>
    /// <remarks></remarks>
    public enum TextMarker
    {
        quotes,
        doubleQuotes
    }

}
