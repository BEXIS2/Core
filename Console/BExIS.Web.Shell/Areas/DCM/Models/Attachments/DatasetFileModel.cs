using BExIS.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BExIS.Modules.Dcm.UI.Models.Attachments
{
    public class DatasetFilesModel
    {
        public Dictionary<BasicFileInfo,String> ServerFileList { get; set; }
        public int FileSize = 0;
    }
}