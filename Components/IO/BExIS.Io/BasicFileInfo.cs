using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BExIS.Io
{
    public class BasicFileInfo
    {
        public string Name { get; set; }
        public string Uri { get; set; }
        public string MimeType { get; set; }

        public BasicFileInfo(string path, string mimeType)
        {
            this.Name = path.Split('\\').Last();
            this.Uri = path;
            this.MimeType = mimeType;
        }
    }
}
