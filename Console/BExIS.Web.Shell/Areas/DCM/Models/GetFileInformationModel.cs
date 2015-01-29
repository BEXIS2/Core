using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using BExIS.IO.Transform.Input;
using BExIS.IO.Transform.Validation.Exceptions;
using BExIS.Dcm.Wizard;

namespace BExIS.Web.Shell.Areas.DCM.Models
{
    public class GetFileInformationModel
    {
        public StepInfo StepInfo { get; set; }
        public FileInfoModel FileInfoModel { get; set; }
        public string Extention { get; set; }
        public List<Error> ErrorList { get; set; }
        public bool IsSaved { get; set; }

        public GetFileInformationModel()
        {
            FileInfoModel = new FileInfoModel();
            Extention = "";
            ErrorList = new List<Error>();
            IsSaved = false;
        }

    }
}