using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using BExIS.Io;

namespace BExIS.Web.Shell.Areas.DCM.Models.Push
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