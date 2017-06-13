using BExIS.Dcm.Wizard;

namespace BExIS.Modules.Dcm.UI.Models.ImportMetadata
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