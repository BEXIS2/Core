using System;
using System.Globalization;

namespace BExIS.IO
{
    public class IOUtility
    {

        public static bool IsDate(string value)
        {
            DateTime dateTime;

            if(DateTime.TryParse(value,out dateTime))
            {
                return true;
            }

            if(DateTime.TryParse(value,new CultureInfo("de-DE", false),DateTimeStyles.None,out dateTime))
            {
                return true;
            }

            if(DateTime.TryParse(value,new CultureInfo("en-US", false),DateTimeStyles.None,out dateTime))
            {
                return true;
            }

            if (DateTime.TryParse(value,CultureInfo.InvariantCulture, DateTimeStyles.None, out dateTime))
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
        public static string ConvertDateToCulture(string dateAsString)
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
            if(double.TryParse(dateAsString, out valueAsDouble))
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

            return null;
        }

    }
}
