using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using BExIS.DCM.Transform.Input;
using BExIS.DCM.Transform.Validation.Exceptions;
using BExIS.DCM.UploadWizard;

namespace BExIS.Web.Shell.Areas.DCM.Models
{
    public class GetFileInformationModel
    {
        public StepInfo StepInfo { get; set; }
        public FileInfoModel FileInfoModel { get; set; }
        public string Extention { get; set; }
        public List<Error> ErrorList { get; set; }

        public GetFileInformationModel()
        {
            FileInfoModel = new FileInfoModel();
            Extention = "";
            ErrorList = new List<Error>();
        }

    }
}