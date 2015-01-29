using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using BExIS.Dcm.Wizard;

namespace BExIS.Web.Shell.Areas.DCM.Models.ImportMetadata
{
    public class SummaryModel:AbstractStepModel
    {
        public string Title { get; set; }

        public string SchemaName { get; set; }
        public string RootName { get; set; }

        public SummaryModel(StepInfo stepInfo)
        { 
            this.StepInfo = stepInfo;
        }
    }
}