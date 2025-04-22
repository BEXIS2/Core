/// <summary>
///
/// </summary>
namespace BExIS.IO.Transform.Input
{
    /// <summary>
    /// This class is required to store information that is important to read of data from ascii files.
    /// </summary>
    /// <remarks></remarks>
    public class ApiFileReaderInfo : FileReaderInfo
    {
        /// <summary>
        ///
        /// </summary>
        public ApiFileReaderInfo()
        {
            Decimal = DecimalCharacter.point;
            TextMarker = TextMarker.none;
            Variables = 0;
            Data = 1;
        }

        /// <summary>
        /// separator stores the TextSeperator type from which serves as a delimiter
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref="TextSeperator"/>
        public TextSeperator Seperator { get; set; }

        /// <summary>
        /// Stores the marker type from the marking of text is used to
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref="TextMarker"/>
        public TextMarker TextMarker { get; set; }

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
        /// <remarks></remarks>
        /// <seealso cref=""/>
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
        /// Get Textmarker enum type as String
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param name="tmarker">TextMarker enum type</param>
        /// <returns>tmarker as string</returns>
        public static string GetTextMarkerAsString(TextMarker tmarker)
        {
            switch (tmarker)
            {
                case TextMarker.quotes:
                    return TextMarker.quotes.ToString();

                case TextMarker.doubleQuotes:
                    return TextMarker.doubleQuotes.ToString();

                default: return TextMarker.doubleQuotes.ToString();
            }
        }

        /// <summary>
        /// Get a Textmarker based on the textmarker string name input
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param name="textMarker"> name from textmarker</param>
        /// <returns>TextMarker based on name</returns>
        public static TextMarker GetTextMarker(string textMarker)
        {
            switch (textMarker)
            {
                case "quotes":
                    return TextMarker.quotes;

                case "doubleQuotes":
                    return TextMarker.doubleQuotes;

                default: return TextMarker.quotes;
            }
        }

        /// <summary>
        /// Get character based on the Textmarker enum type
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param name="tmarker">TextMarker enum</param>
        /// <returns>Character based on the tmarker</returns>
        public static char GetTextMarker(TextMarker tmarker)
        {
            switch (tmarker)
            {
                case TextMarker.quotes:
                    return '\'';

                case TextMarker.doubleQuotes:
                    return '"';

                default: return '"';
            }
        }
    }
}