using BExIS.Dcm.Wizard;
using BExIS.IO.Transform.Validation.Exceptions;
using System.Collections.Generic;

namespace BExIS.Modules.Dcm.UI.Models
{
    public enum PageStatus
    {
        FirstLoad,
        Error,
        NoError,
        Saved,
        Submited,
        LastAndSubmitted
    }

    public class AbstractStepModel
    {
        public StepInfo StepInfo { get; set; }
        public List<Error> ErrorList { get; set; }
        public bool Saved { get; set; }
        public PageStatus PageStatus { get; set; }

        public AbstractStepModel()
        {
            ErrorList = new List<Error>();
            Saved = false;
        }
    }
}