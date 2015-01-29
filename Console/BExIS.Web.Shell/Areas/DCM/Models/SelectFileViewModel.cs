using System;
using System.Collections.Generic;
using System.IO;
using System.Web;
using BExIS.IO.Transform.Validation.Exceptions;
using BExIS.Dcm.Wizard;
using BExIS.Dcm.UploadWizard;

namespace BExIS.Web.Shell.Areas.DCM.Models
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

        public List<string> SupportedFileExtentions = new List<string>();

        
        public StepInfo StepInfo { get; set; }
    }
}