using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace BExIS.IO.Transform.Input
{
    public class ExcelHelper
    {
        public static Dictionary<uint, string> FormatCodesDic = new Dictionary<uint, string>()
        {
            {0, ""},
            {1, "0"},
            {2, "0.00"},
            {3, "#,##0"},
            {4, "#,##0.00"},
            {9, "0%"},
            {10, "0.00%"},
            {11, "0.00E+00"},
            {12, "# ?/?"},
            {13, "# ??/??"},
            {14, "d/m/yyyy"},
            {15, "d-mmm-yy"},
            {16, "d-mmm"},
            {17, "mmm-yy"},
            {18, "h:mm tt"},
            {19, "h:mm:ss tt"},
            {20, "H:mm"},
            {21, "H:mm:ss"},
            {22, "m/d/yyyy H:mm"},
            {37, "#,##0;(#,##0)"},
            {38, "#,##0;[Red](#,##0)"},
            {39, "#,##0;(#,##0)"},
            {40, "#,##0.00;[Red](#,##0.00)"},
            {45, "mm:ss"},
            {46, "[h]:mm:ss"},
            {47, "mmss.0"},
            {48, "##0.0E+0"},
            {49, "@"}
        };

        public ExcelHelper()
        {
        }

        public static double FromExcelSerialDate(double SerialDate)
        {
            try
            {
                if (SerialDate > 59) SerialDate -= 1; //Excel/Lotus 2/29/1900 bug
                return new DateTime(1899, 12, 31).AddDays(SerialDate).ToOADate();
            }
            catch (Exception e)
            {
                return 0.0;
            }
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
            CultureInfo ci = CultureInfo.InvariantCulture;

            if (string.IsNullOrEmpty(cellValue)) return "0";

            //get culture information
            // because we take the cell values from excel,
            // there will be no 1000 place chars, so you can check directly for the decimal char
            if (cellValue.Contains('.')) ci = new CultureInfo("en-US");
            else if (cellValue.Contains(',')) ci = new CultureInfo("de-DE");

            if (double.TryParse(cellValue, NumberStyles.Any, ci, out output))
            {
                // round if formatcode is empty
                if (string.IsNullOrEmpty(formatCode))
                {
                    if (output.ToString().ToLower().Contains("e"))
                    {
                        return output.ToString("0.00E+00", ci);
                    }

                    long tmp = System.Convert.ToInt64(output);
                    int charLenght = tmp.ToString().Length;
                    int roundTo = maxcharlength - charLenght - 1; //max 11 - integer - 1 decimal char

                    if (roundTo > 0) output = Math.Round(output, roundTo);

                    return output.ToString();
                }
                else //formt is not empty
                {
                    //if its scientific data then return the cell value
                    //if (formatCode.ToLower().Contains("e")) return cellValue;

                    return output.ToString(formatCode, ci);
                }
            }

            return "0";
        }

        /*
         ID  Format Code
        0   General
        1   0
        2   0.00
        3   #,##0
        4   #,##0.00
        9   0%
        10  0.00%
        11  0.00E+00
        12  # ?/?
        13  # ??/??
        14  d/m/yyyy
        15  d-mmm-yy
        16  d-mmm
        17  mmm-yy
        18  h:mm tt
        19  h:mm:ss tt
        20  H:mm
        21  H:mm:ss
        22  m/d/yyyy H:mm
        37  #,##0 ;(#,##0)
        38  #,##0 ;[Red](#,##0)
        39  #,##0.00;(#,##0.00)
        40  #,##0.00;[Red](#,##0.00)
        45  mm:ss
        46  [h]:mm:ss
        47  mmss.0
        48  ##0.0E+0
        49  @
         */

        public static string GetFormatCode(uint id)
        {
            if (FormatCodesDic.ContainsKey(id))
            {
                return FormatCodesDic[id];
            }

            return "";
        }
    }
}