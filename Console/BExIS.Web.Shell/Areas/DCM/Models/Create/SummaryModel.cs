using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using BExIS.Dcm.Wizard;
using BExIS.Web.Shell.Areas.DCM.Models.Metadata;

namespace BExIS.Web.Shell.Areas.DCM.Models.Create
{
    public class SummaryModel : AbstractStepModel
    {
        public Dictionary<string, MetadataPackageModel> Packages { get; set; }

        public SummaryModel()
        {
            Packages = new Dictionary<string, MetadataPackageModel>();
        }

        public static SummaryModel Convert(Dictionary<string,MetadataPackageModel> packages, StepInfo stepInfo)
        {
            return new SummaryModel
            {
                Packages = packages,
                StepInfo = stepInfo
            };
        }
    }
}