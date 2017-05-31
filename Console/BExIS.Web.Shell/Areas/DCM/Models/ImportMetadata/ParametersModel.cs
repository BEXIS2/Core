using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BExIS.Modules.Dcm.UI.Models.ImportMetadata
{
    public class ParametersModel : AbstractStepModel
    {

        [Display(Name = "Title node")]
        public string TitleNode { get; set; }

        [Display(Name = "Description node")]
        public string DescriptionNode { get; set; }

        [Display(Name = "Entity Type")]
        public string EntityType { get; set; }

        public List<SearchMetadataNode> MetadataNodes { get; set; }
        public List<string> Entities { get; set; }

        public ParametersModel()
        {
            TitleNode = "";
            DescriptionNode = "";
            MetadataNodes = new List<SearchMetadataNode>();
            Entities = new List<string>();
        }
    }
}