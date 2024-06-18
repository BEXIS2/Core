using System;
using System.Globalization;

namespace Vaiona.Utils.Persian
{
    public static class DateTimeExtension
    {
        public static DateTime PersianToGregorian(this DateTime persianDateTime)
        {
            DateTime dt;
            PersianCalendar objPersianCalendar = new PersianCalendar();
            dt = objPersianCalendar.ToDateTime(
                persianDateTime.Year,
                persianDateTime.Month,
                persianDateTime.Day,
                persianDateTime.Hour,
                persianDateTime.Minute,
                persianDateTime.Second,
                persianDateTime.Millisecond);
            return (dt);
        }

        public static string PersianToString(this DateTime persianDateTime, char separator = '/')
        {
            //PersianCalendar objPersianCalendar = new PersianCalendar();

            return (string.Format("{0:00}{3}{1:00}{3}{2:00}", persianDateTime.Year, persianDateTime.Month, persianDateTime.Day, separator));
        }

        public static string PersianToStringFull(this DateTime persianDateTime, char separator = '/')
        {
            return (string.Format("{0:00}{3}{1:00}{3}{2:00} {4:00}:{5:00}:{6:00}", persianDateTime.Year, persianDateTime.Month, persianDateTime.Day, separator, persianDateTime.Hour, persianDateTime.Minute, persianDateTime.Second));
        }

        public static DateTime GregorianToPersian(this DateTime gregorianDateTime)
        {
            string result = "";
            if (gregorianDateTime != null)
            {
                PersianCalendar objPersianCalendar = new PersianCalendar();
                int year = objPersianCalendar.GetYear(gregorianDateTime);
                int month = objPersianCalendar.GetMonth(gregorianDateTime);
                int day = objPersianCalendar.GetDayOfMonth(gregorianDateTime);
                int hour = objPersianCalendar.GetHour(gregorianDateTime);
                int min = objPersianCalendar.GetMinute(gregorianDateTime);
                int sec = objPersianCalendar.GetSecond(gregorianDateTime);
                result = year.ToString().PadLeft(4, '0') + "/" +
                         month.ToString().PadLeft(2, '0') + "/" +
                day.ToString().PadLeft(2, '0') + " " +
                hour.ToString().PadLeft(2, '0') + ":" +
                min.ToString().PadLeft(2, '0') + ":" + sec.ToString().PadLeft(2, '0');
            }
            return (DateTime.Parse(result));
        }
    }
}