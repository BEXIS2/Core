using System;
using System.Collections.Generic;
using System.Globalization;

namespace BExIS.IO
{
    public class IOUtility
    {


        public bool IsDate(string value)
        {
            DateTime dateTime;

            if (DateTime.TryParse(value, out dateTime))
            {
                return true;
            }

            if (DateTime.TryParse(value, new CultureInfo("de-DE", false), DateTimeStyles.None, out dateTime))
            {
                return true;
            }

            if (DateTime.TryParse(value, new CultureInfo("en-US", false), DateTimeStyles.None, out dateTime))
            {
                return true;
            }

            if (DateTime.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.None, out dateTime))
            {
                return true;
            }

            return false;
        }

        public bool IsDate(string value, out DateTime dateTime)
        {

            if (DateTime.TryParse(value, out dateTime))
            {
                return true;
            }

            if (DateTime.TryParse(value, new CultureInfo("en-US", false), DateTimeStyles.None, out dateTime))
            {
                return true;
            }

            if (DateTime.TryParse(value, new CultureInfo("de-DE", false), DateTimeStyles.None, out dateTime))
            {
                return true;
            }

            if (DateTime.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.None, out dateTime))
            {
                return true;
            }

            double valueAsDouble;
            if (double.TryParse(value, out valueAsDouble))
            {
                try
                {
                    dateTime = DateTime.FromOADate(valueAsDouble);
                    if (dateTime != null) return true;
                }
                catch (ArgumentException e)
                {
                }
            }

            return false;
        }

        /// <summary>
        /// try to convert a string with a pattern to a datetime
        /// </summary>
        /// <param name="dateAsString"></param>
        /// <param name="pattern"></param>
        /// <param name="dateTime"></param>
        /// <param name="cultureInfo"></param>
        /// <returns>true or false and out datetime</returns>
        public bool IsDate(string dateAsString, string pattern, out DateTime dateTime, CultureInfo cultureInfo = null)
        {
            if (cultureInfo == null) cultureInfo = CultureInfo.InvariantCulture;

            if (ConvertToDate(dateAsString, pattern, out dateTime, cultureInfo))
            {
                return true;
            }

            return false;
        }


        /// <summary>
        /// Convert a Datetime as string to a datetime as string from a other culture.
        /// </summary>
        /// <param name="dateAsString"></param>
        /// <param name="culture">new CultureInfo("en-US", false)</param>
        /// <returns></returns>
        public virtual string ConvertDateToCulture(string dateAsString)
        {
            DateTime dateTime;

            if (DateTime.TryParse(dateAsString, out dateTime))
            {
                //return dateTime.ToString(CultureInfo.InvariantCulture);
                return dateTime.ToString(new CultureInfo("en-US"));
            }

            if (DateTime.TryParse(dateAsString, new CultureInfo("de-DE", false), DateTimeStyles.None, out dateTime))
            {
                return dateTime.ToString(new CultureInfo("en-US"));
            }

            if (DateTime.TryParse(dateAsString, new CultureInfo("en-US", false), DateTimeStyles.None, out dateTime))
            {
                return dateTime.ToString(new CultureInfo("en-US"));
            }

            if (DateTime.TryParse(dateAsString, CultureInfo.InvariantCulture, DateTimeStyles.None, out dateTime))
            {
                return dateTime.ToString(new CultureInfo("en-US"));
            }

            //Date might still be in OA-Date-Format (happens for Libre-Office dates)
            double valueAsDouble;
            if (double.TryParse(dateAsString, out valueAsDouble))
            {
                try
                {
                    dateTime = DateTime.FromOADate(valueAsDouble);
                    return dateTime.ToString(new CultureInfo("en-US"));
                }
                catch (ArgumentException e)
                {
                }
            }

            return "";
        }

        /// <summary>
        /// Convert a Datetime as string to a datetime .
        /// </summary>
        /// <param name="dateAsString"></param>
        /// <returns></returns>
        public virtual bool TryConvertDate(string dateAsString, out DateTime dateTime)
        {


            if (DateTime.TryParse(dateAsString, out dateTime))
            {
                //return dateTime.ToString(CultureInfo.InvariantCulture);
                return true;
            }

            if (DateTime.TryParse(dateAsString, new CultureInfo("de-DE", false), DateTimeStyles.None, out dateTime))
            {
                return true;
            }

            if (DateTime.TryParse(dateAsString, new CultureInfo("en-US", false), DateTimeStyles.None, out dateTime))
            {
                return true;
            }

            if (DateTime.TryParse(dateAsString, CultureInfo.InvariantCulture, DateTimeStyles.None, out dateTime))
            {
                return true;
            }

            //Date might still be in OA-Date-Format (happens for Libre-Office dates)
            double valueAsDouble;
            if (double.TryParse(dateAsString, out valueAsDouble))
            {
                try
                {
                    dateTime = DateTime.FromOADate(valueAsDouble);
                    return true;
                }
                catch (ArgumentException e)
                {
                }
            }

            return false;
        }


        /// <summary>
        /// try to convert a string with a pattern to a datetime
        /// </summary>
        /// <param name="dateAsString"></param>
        /// <param name="pattern"></param>
        /// <returns></returns>
        public string ConvertToDateUS(string dateAsString, string pattern, CultureInfo cultureInfo = null)
        {
            DateTime tmp;
            if (cultureInfo == null) cultureInfo = CultureInfo.InvariantCulture;

            if (ConvertToDate(dateAsString, pattern, out tmp, cultureInfo))
            {
                return tmp.ToString(new CultureInfo("en-us"));
            }

            return "";
        }

        public bool ConvertToDate(string dateAsString, string pattern, out DateTime dateTime, CultureInfo cultureInfo = null)
        {
            if (cultureInfo == null) cultureInfo = CultureInfo.InvariantCulture;

            if (DateTime.TryParseExact(dateAsString, pattern, cultureInfo, DateTimeStyles.NoCurrentDateDefault, out dateTime))
            {
                return true;
            }

            //Date might still be in OA-Date-Format (happens for Libre-Office dates)
            double valueAsDouble;
            if (double.TryParse(dateAsString, out valueAsDouble))
            {
                try
                {
                    dateTime = DateTime.FromOADate(valueAsDouble);
                    if (dateTime.Millisecond >= 500) dateTime.AddSeconds(1);

                    return true;
                }
                catch (ArgumentException e)
                {
                }
            }


            return false;
        }

        public string ExportDateTimeString(string dateAsString, string pattern, out DateTime dateTime, CultureInfo cultureInfo = null)
        {

            if (DateTime.TryParse(dateAsString, new CultureInfo("en-US", false), DateTimeStyles.NoCurrentDateDefault, out dateTime))
            {
                return dateTime.ToString(pattern);
            }

            return "";
        }

        /// <summary>
        /// returns a list of supported ascii filetypes
        /// "e.g. .csv"
        /// </summary>
        /// <returns></returns>
        public Dictionary<string, string> GetSupportedAsciiFiles()
        {
            Dictionary<string, string> tmp = new Dictionary<string, string>();
            tmp.Add("Comma Separated", ".csv");
            tmp.Add("Tab Separated", ".tsv");
            tmp.Add("Semicolon Separated", ".txt");

            return tmp;
        }

        /// <summary>
        /// returns true if the file is supported 
        /// "e.g. .csv"
        /// </summary>
        /// <param name="extention"></param>
        /// <returns></returns>
        public bool IsSupportedAsciiFile(string extention)
        {
            if (extention.Equals(".csv") ||
            extention.Equals(".tsv") ||
            extention.Equals(".txt"))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// returns true if the file is supported 
        /// "e.g. .csv"
        /// </summary>
        /// <param name="extention"></param>
        /// <returns></returns>
        public Dictionary<string, string> GetSupportedExcelFiles()
        {
            Dictionary<string, string> tmp = new Dictionary<string, string>();
            tmp.Add("Excel up to 2013", ".xlsx");

            return tmp;
        }

        public bool IsSupportedExcelFile(string extention)
        {
            if (extention.Equals(".xls") ||
            extention.Equals(".xlsx"))
            {
                return true;
            }

            return false;
        }

    }
}
