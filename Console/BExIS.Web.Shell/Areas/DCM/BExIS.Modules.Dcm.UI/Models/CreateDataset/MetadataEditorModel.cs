using BExIS.Dcm.Wizard;
using System.Collections.Generic;

namespace BExIS.Modules.Dcm.UI.Models.CreateDataset
{
    public class MetadataEditorModel
    {
        public long DatasetId { get; set; }
        public string DatasetTitle { get; set; }
        public bool EditRight { get; set; }
        public bool EditAccessRight { get; set; }
        public bool Created { get; set; }
        public bool FromEditMode { get; set; }
        public bool Changed { get; set; }
        public bool Import { get; set; }
        public bool SaveWithErrors { get; set; }
        public bool LatestVersion { get; set; }
        public string HeaderHelp { get; set; }

        public List<StepModelHelper> StepModelHelpers { get; set; }
        public Dictionary<string, ActionInfo> Actions { get; set; }

        public MetadataEditorModel()
        {
            DatasetId = -1;
            DatasetTitle = "";
            Created = false;
            FromEditMode = false;
            Changed = false;
            Import = false;
            SaveWithErrors = true;
            HeaderHelp = "";

            EditRight = false;
            EditAccessRight = false;
            LatestVersion = false;

            Actions = new Dictionary<string, ActionInfo>();
        }
    }
}