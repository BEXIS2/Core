using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using BExIS.Dcm.Wizard;

namespace BExIS.Web.Shell.Areas.DCM.Models.ImportMetadata
{
    public class ReadSourceModel:AbstractStepModel
    {
        public string RootNode { get; set; }
        public string SchemaName { get; set; }

        public ReadSourceModel(StepInfo stepInfo)
        { 
            this.StepInfo = stepInfo;
            RootNode = "";
            SchemaName = "";
        }
    }
}