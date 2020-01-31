using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Text.RegularExpressions;

namespace IDIV.Modules.Mmm.UI.Converter
{
    public class MetadataConverter
    {
        public static double convertDegreeAngleToDouble(string cord)
        {
            int multiplier = (cord.Contains("S") || cord.Contains("W")) ? -1 : 1; //handle south and west

            cord = Regex.Replace(cord, "[^0-9. ]", ""); //remove the characters

            string[] cordArray = cord.Split(' '); //split the string.

            double degrees = Double.Parse(cordArray[0]);
            double minutes = Double.Parse(cordArray[1]) / 60;
            double seconds = Double.Parse(cordArray[2]) / 3600;

            return Math.Round((degrees + minutes + seconds) * multiplier, 7);
        }
    }
}