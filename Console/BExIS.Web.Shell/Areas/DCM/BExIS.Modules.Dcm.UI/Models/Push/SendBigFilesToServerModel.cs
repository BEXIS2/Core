using BExIS.IO;
using System.Collections.Generic;

namespace BExIS.Modules.Dcm.UI.Models.Push
{
    public class SendBigFilesToServerModel
    {
        public List<BasicFileInfo> ServerFileList { get; set; }
        public bool IsUploading { get; set; }

        public List<string> SupportedFileExtentions = new List<string>();
        public int FileSize = 0;

        public SendBigFilesToServerModel()
        {
            IsUploading = false;
        }
    }
}