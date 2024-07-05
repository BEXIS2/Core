using System.Collections.Generic;

/// <summary>
///
/// </summary>
namespace BExIS.IO.Transform.Output
{
    /// <summary>
    ///
    /// </summary>
    /// <remarks></remarks>
    public class AsciiHelper
    {
        /// <summary>
        /// Get TextSeperator as string
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param name="seperator">TextSeperator enum Type</param>
        /// <returns>TextSeperator as string</returns>
        public static string GetSeperatorAsString(TextSeperator sep)
        {
            switch (sep)
            {
                case TextSeperator.comma:
                    return TextSeperator.comma.ToString();

                case TextSeperator.semicolon:
                    return TextSeperator.semicolon.ToString();

                case TextSeperator.space:
                    return TextSeperator.space.ToString();

                case TextSeperator.tab:
                    return TextSeperator.tab.ToString();

                default: return TextSeperator.tab.ToString();
            }
        }

        /// <summary>
        /// Get TextSeperator based on string as name
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param name="seperator">Name of TextSeperator</param>
        /// <returns>TextSeperator as enum TextSeperator</returns>
        public static TextSeperator GetSeperator(string seperator)
        {
            switch (seperator)
            {
                case "comma":
                    return TextSeperator.comma;

                case "semicolon":
                    return TextSeperator.semicolon;

                case "space":
                    return TextSeperator.space;

                case "tab":
                    return TextSeperator.tab;

                default: return TextSeperator.tab;
            }
        }

        /// <summary>
        /// Get a Textseparator as a Character
        /// </summary>
        /// <param name="sep"></param>
        /// <returns>TextSeperator as character</returns>
        public static char GetSeperator(TextSeperator sep)
        {
            switch (sep)
            {
                case TextSeperator.comma:
                    return ',';

                case TextSeperator.semicolon:
                    return ';';
                case TextSeperator.space:
                    return ' ';

                case TextSeperator.tab:
                default:
                    return '\t';
            }
        }

        /// <summary>
        /// Get all text seperators as char in a list
        /// </summary>
        /// <returns>List of char </returns>
        public static List<char> GetAllSeperator()
        {
            List<char> allSeperatorsAsChar = new List<char>();

            allSeperatorsAsChar.Add(',');
            allSeperatorsAsChar.Add(';');
            allSeperatorsAsChar.Add(' ');
            allSeperatorsAsChar.Add('\t');

            return allSeperatorsAsChar;
        }
    }
}