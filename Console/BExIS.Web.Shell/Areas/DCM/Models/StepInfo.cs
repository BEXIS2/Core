using System;
using System.Collections.Generic;

namespace BExIS.Web.Shell.Areas.DCM.Models
{
    public class StepInfo
    {
        public string title;
        public bool valid = false;
        public StepStatus stepStatus;
        public Dictionary<string, object> parameters;

        public ActionInfo GetActionInfo { get; set; }
        public ActionInfo PostActionInfo { get; set; }


        public StepInfo(string title)
        {
            this.title = title;
            this.stepStatus = StepStatus.none;
            this.GetActionInfo = new ActionInfo();
            this.PostActionInfo = new ActionInfo();
        }

        public Tuple<string, StepStatus> GetStatusWithName()
        {
            return new Tuple<string, StepStatus>(title, stepStatus);
        }

        public void SetStatus(StepStatus status)
        {
            this.stepStatus = status;
        }

        public bool IsEmpty()
        {
            if (String.IsNullOrEmpty(title)) return true;
            else return false;
        }

        public bool IsValid()
        {
            return valid;
        }

        public void SetValid(bool valid)
        {
            this.valid = valid;
        }
    }

    public enum StepStatus
    { 
        none,
        success,
        error,
        inProgress,
    }
}
