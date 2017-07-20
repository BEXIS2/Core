using System.Collections.Generic;
using BExIS.IO;

namespace BExIS.Modules.Dcm.UI.Models.Push
{
    public class SendBigFilesToServerModel
    {
        public List<BasicFileInfo> ServerFileList { get; set; }
        public bool IsUploading { get; set; }

        public List<string> SupportedFileExtentions = new List<string>();

        public SendBigFilesToServerModel() {

            IsUploading = false;
        }
    }

}