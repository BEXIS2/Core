using BExIS.Utils.Models;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BExIS.Modules.Dcm.UI.Models.ImportMetadata
{
    public class ParametersModel : AbstractStepModel
    {
        public ParametersModel()
        {
            TitleNode = "";
            DescriptionNode = "";
            MetadataNodes = new List<SearchMetadataNode>();
            Entities = new List<string>();
        }

        [Display(Name = "Description node")]
        public string DescriptionNode { get; set; }

        public List<string> Entities { get; set; }

        [Display(Name = "Entity Type")]
        public string EntityType { get; set; }

        public List<SearchMetadataNode> MetadataNodes { get; set; }

        [Display(Name = "Title node")]
        public string TitleNode { get; set; }
    }
}