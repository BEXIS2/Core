using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using BExIS.Dcm.Wizard;
using BExIS.Web.Shell.Areas.DCM.Models.Metadata;

namespace BExIS.Web.Shell.Areas.DCM.Models.Create
{
    public class CreateSummaryModel : AbstractStepModel
    {
        public Dictionary<string, MetadataPackageModel> Packages { get; set; }
        public String DatasetTitle { get; set; }
        public long DatasetId { get; set; }



        public CreateSummaryModel()
        {
            Packages = new Dictionary<string, MetadataPackageModel>();
        }

        public static CreateSummaryModel Convert(Dictionary<string,MetadataPackageModel> packages, StepInfo stepInfo)
        {
            return new CreateSummaryModel
            {
                Packages = packages,
                StepInfo = stepInfo,
                DatasetTitle = "",
                DatasetId=0
            };
        }
    }
}