using System;
using System.Text.RegularExpressions;

namespace BExIS.Dim.Helpers
{
    public class WebServiceHelper
    {
        /// <summary>
        /// Convert all none alphanumeric characters in hex
        /// and produce a valid html url string
        /// </summary>
        /// <param name="source"></param>
        public static string Encode(string source)
        {
            string tmp = "";
            string pattern = @"(?:[a-z][a-z]*[0-9]+[a-z0-9]*)";
            Regex rg = new Regex(pattern, RegexOptions.IgnoreCase);

            foreach (char character in source)
            {
                byte charInByte = Convert.ToByte(character);
                string hex = charInByte.ToString("x");
                hex = "%" + hex.ToUpper();
                string match = new string(character, 1);

                tmp += rg.Match(match).Success ? match : hex;
            }

            return tmp;
        }
    }
}