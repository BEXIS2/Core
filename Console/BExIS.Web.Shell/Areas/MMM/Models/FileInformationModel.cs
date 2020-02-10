using System.IO;
using System.Collections.Generic;
using System.Collections;

namespace IDIV.Modules.Mmm.UI.Models
{
    public class FileInformation
    {
        public string Name { get; set; }
        public string MimeType { get; set; }
        public uint Size { get; set; }
        public string Path { get; set; }
        public Dictionary<string, Dictionary<string, string>> EXIF;

        public FileInformation()
        {
            this.Name = null;
            this.MimeType = null;
            this.Size = 0;
            this.Path = null;
            this.EXIF = null;
        }

        public FileInformation(string name, string mimeType, uint size, string path, Dictionary<string, Dictionary<string, string>> exif = null)
        {
            this.Name = name;
            this.MimeType = mimeType;
            this.Size = size;
            this.Path = path;
            this.EXIF = exif;
        }

    }
}

    