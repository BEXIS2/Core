using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using BExIS.Dlm.Entities.Common;
using BExIS.Web.Shell.Areas.DCM.Models.Metadata;
using BExIS.IO.Transform.Validation.Exceptions;

namespace BExIS.Web.Shell.Areas.DCM.Models.Create
{
    public class StepModelHelper
    {
        public int StepId { get; set; }
        public BaseUsage Usage { get; set; }
        public int Number { get; set; }
        public AbstractMetadataStepModel Model { get; set; }
        public string XPath { get; set; }
        

        public StepModelHelper()
        {
            StepId = 0;
            Usage = new BaseUsage();
            Number = 0;
            XPath = "";
        }

        public StepModelHelper(int stepId, int number, BaseUsage usage, string xpath)
        {
            StepId = stepId;
            Usage = usage;
            Number = number;
            XPath = xpath;
        }
    }
}