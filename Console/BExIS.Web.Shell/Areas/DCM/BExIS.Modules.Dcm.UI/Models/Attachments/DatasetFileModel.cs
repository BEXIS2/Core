using BExIS.IO;
using System;
using System.Collections.Generic;

namespace BExIS.Modules.Dcm.UI.Models.Attachments
{
    public class DatasetFilesModel
    {
        public Dictionary<BasicFileInfo, String> ServerFileList { get; set; }
        public int FileSize = 0;

        //only read the file names, RughtType.Read
        public bool ViewAccess { get; set; }

        //Grand access (delete, upload, download), RughtType.Grant,RughtType.Write
        public bool UploadAccess { get; set; }

        public bool DeleteAccess { get; set; }

        //only read and download the files, RughtType.Download
        public bool DownloadAccess { get; set; }
    }
}