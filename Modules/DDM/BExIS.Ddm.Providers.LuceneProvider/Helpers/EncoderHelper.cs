using BExIS.Utils.Helpers;
using System.Diagnostics;
using System.Text.RegularExpressions;

/// <summary>
///
/// </summary>
namespace BExIS.Ddm.Providers.LuceneProvider.Helpers
{
    /// <summary>
    /// Lucene use some spacial Characters for query
    /// This Encoder find and replace this special Character for searching
    /// </summary>
    /// <remarks></remarks>
    public static class EncoderHelper
    {
        private static string[] specialCharacterArray = new string[] { "\\", "^", "\"", "+", "-", "&&", "||", "!", "(", ")", "{", "}", "[", "]", "~", "*", "?", ":", "<" };
        private static string[] invalidCharacterArray = new string[] { "_" };

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string Encode(string value)
        {
            string encodedValue = value;

            // has special characters
            if (RegExHelper.IsMatch(value, RegExHelper.LUCENE_INVALID_CHARS_REGEX))
            {
                encodedValue = replaceSpecialCharacters(value);
                encodedValue = removeInvalidCharacters(encodedValue);
            }

            return encodedValue;
        }

        /// <summary>
        /// compare the list of Special Characters with the
        /// incoming value and add a \ before it
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param name="value"></param>
        /// <returns></returns>
        private static string replaceSpecialCharacters(string value, bool encodeSpace = false)
        {
            foreach (string specailCharacter in specialCharacterArray)
            {
                if (value.Contains(specailCharacter)) value = value.Replace(specailCharacter, @"\" + specailCharacter);
            }

            Debug.WriteLine(value);

            return value;
        }

        /// <summary>
        /// compare the list of Special Characters with the
        /// incoming value and add a \ before it
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param name="value"></param>
        /// <returns></returns>
        private static string removeInvalidCharacters(string value, bool encodeSpace = false)
        {
            foreach (string invalidCHar in invalidCharacterArray)
            {
                if (value.Contains(invalidCHar)) value = value.Replace(invalidCHar, " ");
            }

            Debug.WriteLine(value);

            return value;
        }

        /// <summary>
        /// Check if SpecialCharacter is in the incoming value string
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param name="value"></param>
        /// <returns></returns>
        private static bool specialCharactrersInValue(string value)
        {
            Regex rgx = new Regex(RegExHelper.LUCENE_INVALID_CHARS_REGEX, RegexOptions.IgnoreCase);
            Match m = rgx.Match(value);
            if (m.Success)
            {
                return true;
            }

            return false;
        }
    }
}