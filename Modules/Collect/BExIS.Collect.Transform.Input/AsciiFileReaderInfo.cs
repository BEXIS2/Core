using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
        public TextSeperator Seperator { get; set; }
    }
}
