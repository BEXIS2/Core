using System.Collections.Generic;
using BExIS.IO.Transform.Validation.Exceptions;
using BExIS.Dcm.Wizard;
using BExIS.Web.Shell.Areas.DCM.Models.Metadata;
using System;

namespace BExIS.Web.Shell.Areas.DCM.Models
{
    public class SelectMetaDataModel
    {
        public StepInfo StepInfo { get; set; }
        public long SelectedMetaDataId { get; set; }
        public List<Tuple<long, string>> AvailableMetadata { get; set; }
        public string DescriptionTitle { get; set; }

        public List<Error> ErrorList { get; set; }

        public SelectMetaDataModel()
        {
            ErrorList = new List<Error>();
            AvailableMetadata = new List<Tuple<long, string>>();
            SelectedMetaDataId = -1;
        }
    }
}