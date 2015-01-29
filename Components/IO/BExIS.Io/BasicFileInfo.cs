using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BExIS.IO
{
    /// <summary>
    /// Basic informations of a file
    /// </summary>
    public class BasicFileInfo
    {
        /// <summary>
        /// Name of a File
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Path of a file
        /// </summary>
        public string Uri { get; set; }

        /// <summary>
        /// Mine Type of e File
        /// </summary>
        public string MimeType { get; set; }

        /// <summary>
        /// Constructor of BasicFileInfo
        /// </summary>
        /// <param name="path">Location where the file exist</param>
        /// <param name="mimeType">Type of the file</param>
        public BasicFileInfo(string path, string mimeType)
        {
            this.Name = path.Split('\\').Last();
            this.Uri = path;
            this.MimeType = mimeType;
        }
    }
}
