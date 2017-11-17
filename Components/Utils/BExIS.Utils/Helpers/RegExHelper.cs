using System;
using System.Text.RegularExpressions;

namespace BExIS.Utils.Helpers
{
    public class RegExHelper
    {
        public static string FILENAME_REGEX = "[^\\/:*?<>|\"]+";
        public static string FILENAME_INVALID_CHARS_REGEX = "[\\\"/:*?<>|]+";

        public static string LUCENE_INVALID_CHARS_REGEX = "[\\-&&\\^+||!(){}[\\]~*?:\"]";

        public static bool IsFilenameValid(string name)
        {

            if (string.IsNullOrEmpty(name)) return false;
            Regex r = new Regex(FILENAME_INVALID_CHARS_REGEX);

            return !r.IsMatch(name);
        }

        public static string GetCleanedFilename(string name)
        {
            return Regex.Replace(name, FILENAME_INVALID_CHARS_REGEX, "_");
        }

        public static bool IsMatch(string value, string regex)
        {
            try
            {
                Regex r = new Regex(regex);

                return r.IsMatch(value);
            }
            catch (Exception ex)
            {
                return false;
            }
        }
    }
}
