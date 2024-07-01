using BExIS.Dcm.Wizard;
using BExIS.IO.Transform.Validation.Exceptions;
using BExIS.Utils.Data.Upload;
using System;
using System.Collections.Generic;
using System.IO;
using System.Web;

namespace BExIS.Modules.Dcm.UI.Models
{
    public class SelectFileViewModel
    {
        public HttpPostedFileBase file;
        public List<string> serverFileList = new List<string>();
        public List<Error> ErrorList = new List<Error>();
        public String SelectedFileName = "";
        public String SelectedServerFileName = "";
        public Stream fileStream;
        public DataStructureType DataStructureType;
        public int MaxFileLength = 256;

        public List<string> SupportedFileExtentions = new List<string>();

        public StepInfo StepInfo { get; set; }
    }
}