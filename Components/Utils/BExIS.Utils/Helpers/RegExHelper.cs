using System;
using System.Text.RegularExpressions;

namespace BExIS.Utils.Helpers
{
    public class RegExHelper
    {
        public static string FILENAME_REGEX = "[^\\/:*?<>|\"]+";
        public static string FILENAME_INVALID_CHARS_REGEX = "[\\\"/:*?<>|]+";

        public static string LUCENE_INVALID_CHARS_REGEX = "[\\-&&\\^+||!(){}_[\\]~*?:\"]";

        public static string DIMENSION_SPECIFICATION = @"L\([0-9],[0-9]\)M\([0-9],[0-9]\)T\([0-9],[0-9]\)I\([0-9],[0-9]\)\?\([0-9],[0-9]\)N\([0-9],[0-9]\)J\([0-9],[0-9]\)";

        // (["'])(?:(?=(\\?))\2.)*?\1
        public const string BETWEEN_QUOTES = "([\"'])(?:(?=(\\\\?))\\2.)*?\\1";

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