using BExIS.Dcm.Wizard;
using BExIS.IO.Transform.Validation.Exceptions;
using System.Collections.Generic;

namespace BExIS.Modules.Dcm.UI.Models
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