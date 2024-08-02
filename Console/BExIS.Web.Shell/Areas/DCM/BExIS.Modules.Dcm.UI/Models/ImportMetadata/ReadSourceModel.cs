using BExIS.Dcm.Wizard;
using System.ComponentModel.DataAnnotations;

namespace BExIS.Modules.Dcm.UI.Models.ImportMetadata
{
    public class ReadSourceModel : AbstractStepModel
    {
        [Display(Name = "Schema name")]
        [Required(ErrorMessage = "Please define a name for the metadata structure.")]
        public string SchemaName { get; set; }

        [Display(Name = "Root node")]
        public string RootNode { get; set; }

        public bool IsGenerated { get; set; }

        public ReadSourceModel(StepInfo stepInfo)
        {
            this.StepInfo = stepInfo;
            RootNode = "";
            SchemaName = "";
            IsGenerated = false;
        }
    }
}