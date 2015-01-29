using System.Collections.Generic;
using BExIS.IO.Transform.Validation.Exceptions;
using BExIS.Dcm.Wizard;

namespace BExIS.Web.Shell.Areas.DCM.Models
{
    public class ValidationModel
    {
        public StepInfo StepInfo { get; set; }

        public List<Error> ErrorList { get; set; }

        public CreateDatasetViewModel DatasetViewModel { get; set; }

        public bool Validated { get; set; }

        public ValidationModel()
        {
            Validated = false;
            ErrorList = new List<Error>();
        }
    }
}