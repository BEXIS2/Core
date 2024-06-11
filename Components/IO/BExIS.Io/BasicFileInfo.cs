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
        /// Path of a file
        /// </summary>
        public string Extention { get; set; }

        /// <summary>
        /// Mine Type of e File
        /// </summary>
        public string MimeType { get; set; }

        /// <summary>
        /// Mine Type of e File
        /// </summary>
        public double FileSize { get; set; }

        /// <summary>
        /// Constructor of BasicFileInfo
        /// </summary>
        /// <param name="path">Location where the file exist</param>
        /// <param name="mimeType">Type of the file</param>
        public BasicFileInfo(string name, string path, string mimeType, string extention, double fileSize)
        {
            Name = name;
            Uri = path;
            MimeType = mimeType;
            FileSize = fileSize;
            Extention = extention;
        }
    }
}