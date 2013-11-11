
namespace BExIS.DCM.Transform.Input
{
    public enum TextSeperator
    {
        tab,
        comma,
        semicolon,
        space
    }

    public class AsciiFileReaderInfo:FileReaderInfo
    {
        public AsciiFileReaderInfo()
        {
            this.Seperator = TextSeperator.tab;
            this.Offset = 0;
            this.Orientation = Orientation.columnwise;
            this.Variables = 1;
            this.Data = 2;
            this.Offset = 0;
        }

        public TextSeperator Seperator { get; set; }

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
    }
}
