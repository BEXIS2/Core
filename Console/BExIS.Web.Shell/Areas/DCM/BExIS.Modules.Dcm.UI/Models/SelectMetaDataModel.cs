using BExIS.Dcm.Wizard;
using BExIS.IO.Transform.Validation.Exceptions;
using System;
using System.Collections.Generic;

namespace BExIS.Modules.Dcm.UI.Models
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