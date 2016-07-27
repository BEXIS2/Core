using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using BExIS.Web.Shell.Areas.DCM.Models.Create;

namespace BExIS.Web.Shell.Areas.DCM.Models.CreateDataset
{
    public class MetadataEditorModel
    {
        public long DatasetId { get; set; }
        public string DatasetTitle { get; set; }
        public bool EditRight { get; set; }
        public bool Created { get; set; }
        public bool FromEditMode { get; set; }
        public bool Changed { get; set; }
        public bool Import { get; set; }
        public List<StepModelHelper> StepModelHelpers { get; set; }

        public MetadataEditorModel()
        {
            DatasetId = -1;
            DatasetTitle = "";
            Created = false;
            FromEditMode = false;
            Changed = false;
            Import = false;
        }
    }
}