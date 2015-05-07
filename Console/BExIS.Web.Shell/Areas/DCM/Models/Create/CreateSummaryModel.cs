using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using BExIS.Dcm.Wizard;
using BExIS.Web.Shell.Areas.DCM.Models.Metadata;
using BExIS.IO.Transform.Validation.Exceptions;

namespace BExIS.Web.Shell.Areas.DCM.Models.Create
{
    public class CreateSummaryModel : AbstractStepModel
    {
        public List<StepModelHelper> StepModelHelpers { get; set; }
        public String DatasetTitle { get; set; }
        public long DatasetId { get; set; }

        public List<Tuple<StepInfo, List<Error>>> AllErrors { get; set; }

        public CreateSummaryModel()
        {
            StepModelHelpers = new List<StepModelHelper>();
            AllErrors = new List<Tuple<StepInfo, List<Error>>>();
        }

        public static CreateSummaryModel Convert(List<StepModelHelper> stepModelHelpers, StepInfo stepInfo)
        {
            return new CreateSummaryModel
            {
                StepModelHelpers = stepModelHelpers,
                StepInfo = stepInfo,
                DatasetTitle = "",
                DatasetId=0
            };
        }
    }
}