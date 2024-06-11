using BExIS.Dcm.Wizard;
using BExIS.IO.Transform.Validation.Exceptions;
using System.Collections.Generic;

namespace BExIS.Modules.Dcm.UI.Models
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