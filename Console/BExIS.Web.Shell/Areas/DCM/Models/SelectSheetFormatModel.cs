using BExIS.Dcm.Wizard;
using BExIS.IO.Transform.Validation.Exceptions;
using System.Collections.Generic;

namespace BExIS.Modules.Dcm.UI.Models
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