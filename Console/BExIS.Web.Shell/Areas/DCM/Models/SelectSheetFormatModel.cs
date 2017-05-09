using System.Collections.Generic;
using BExIS.IO.Transform.Validation.Exceptions;
using BExIS.Dcm.Wizard;
using BExIS.Web.Shell.Areas.DCM.Models.Metadata;

namespace BExIS.Web.Shell.Areas.DCM.Models
{
    public class SelectSheetFormatModel
    {
        public StepInfo StepInfo { get; set; }
        public string SelectedSheetFormat { get; set; }

        public List<Error> ErrorList { get; set; }

        public SelectSheetFormatModel()
        {
            ErrorList = new List<Error>();
        }
    }
}