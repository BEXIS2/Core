using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using BExIS.DCM.Transform.Validation.Exceptions;
using BExIS.Dlm.Entities.DataStructure;

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