using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BExIS.IO.Transform.Input
{
    public class ExcelHelper
    {

        public static double FromExcelSerialDate(double SerialDate)
        {
            if (SerialDate > 59) SerialDate -= 1; //Excel/Lotus 2/29/1900 bug   
            return new DateTime(1899, 12, 31).AddDays(SerialDate).ToOADate();
        }

        /// <summary>
        /// try to convert the incoming cellvalue to a double
        /// </summary>
        /// <param name="cellValue"></param>
        /// <param name="displayValue"></param>
        /// <param name="formatCode"></param>
        /// <returns></returns>
        public static double Convert(string cellValue)
        {
            double output = 0;

            if (string.IsNullOrEmpty(cellValue)) return 0;


            if (double.TryParse(cellValue, out output))
            {
                return output;
            }


            return output;
        }

        public static string ConvertDisplay(string displayValue)
        {
            double output = 0;

            if (string.IsNullOrEmpty(displayValue)) return "0";

            //if (double.TryParse(displayValue,NumberStyles.Any, CultureInfo.InvariantCulture, out output))
            if (double.TryParse(displayValue, out output))
            {
                return output.ToString();
            }
  
            return "0";
        }

        /// <summary>
        /// return a value that is converted by the formatcode from excel
        /// when the formatode is empty the max lenght of 11 chars change the value maybe 
        /// </summary>
        /// <param name="cellValue"></param>
        /// <param name="formatCode"></param>
        /// <returns></returns>
        public static string ConvertWithFormat(string cellValue, string formatCode)
        {
            double output = 0;
            int maxcharlength = 11;
   
            if (string.IsNullOrEmpty(cellValue)) return "0";

            if (double.TryParse(cellValue, out output))
            {
                // round if formatcode is empty
                if (string.IsNullOrEmpty(formatCode))
                {
                    long tmp = System.Convert.ToInt64(output);
                    int charLenght = tmp.ToString().Length;
                    int roundTo = maxcharlength - charLenght - 1; //max 11 - integer - 1 decimal char

                    output = Math.Round(output, roundTo);
                    return output.ToString();
                }
                else //formt is not empty
                {
                    //if its scientific data then return the cell value
                    if (formatCode.ToLower().Contains("e")) return cellValue;

                    return output.ToString(formatCode);
                }
            }

            return "0";
        }

    }
}
