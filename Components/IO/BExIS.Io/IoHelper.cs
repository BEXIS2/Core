using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BExIS.IO
{
    public class IoHelper
    {

        public static string GetDynamicStorePath(long datasetId, long datasetVersionOrderNr, string title, string extention)
        {
            string storePath = Path.Combine("Datasets", datasetId.ToString(), "DatasetVersions");

            return Path.Combine(storePath, datasetId + "_" + datasetVersionOrderNr + "_" + title + extention);
        }

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
