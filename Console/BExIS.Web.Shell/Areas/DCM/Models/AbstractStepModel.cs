using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using BExIS.Dcm.Wizard;
using BExIS.Io.Transform.Validation.Exceptions;

namespace BExIS.Web.Shell.Areas.DCM.Models
{
    public enum PageStatus { 
    
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