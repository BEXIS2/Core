using BExIS.Modules.Dcm.UI.Models.Metadata;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace BExIS.Modules.Dcm.UI.Models
{
    public class CreateDatasetViewModel
    {
        [Remote("Test", "SubmitSpecifyDataset")]
        [Required(ErrorMessage = "Please enter a title.")]
        [RegularExpression("^([A-Za-z0-9- ]+)$", ErrorMessage = "The title must consist only of letters, space, numbers or -.")]
        public string Title { get; set; }

        [Display(Name = "Dataset Author")]
        [Required(ErrorMessage = "Please enter a dataset author.")]
        public string DatasetAuthor { get; set; }

        [Display(Name = "Dataset Owner")]
        [Required(ErrorMessage = "Please enter a owner.")]
        public string Owner { get; set; }

        [Display(Name = "Project Name")]
        [Required(ErrorMessage = "Please enter a project name.")]
        public string ProjectName { get; set; }

        [Display(Name = "Organisation")]
        [Required(ErrorMessage = "Please enter a organisation.")]
        public string ProjectInstitute { get; set; }

        [Display(Name = "Data Structure")]
        [Remote("ValidateMetadataAttributeUsage", "SubmitSpecifyDataset")]
        [Required(ErrorMessage = "Please select a datastructure id.")]
        public long DataStructureId { get; set; }

        [Display(Name = "Research Plan")]
        [Required(ErrorMessage = "Please select a research plan.")]
        public long ResearchPlanId { get; set; }

        public List<long> DataStructureIds { get; set; }
        public List<ListViewItem> DatastructuresViewList { get; set; }

        public List<ListViewItem> ResearchPlanViewList { get; set; }

        public List<MetadataPackageModel> MetadataPackageModel { get; set; }

        public CreateDatasetViewModel()
        {
            Title = "";
            DatasetAuthor = "";
            Owner = "";
            ProjectName = "";
            ProjectInstitute = "";
            DataStructureIds = new List<long>();
            DatastructuresViewList = new List<ListViewItem>();
            ResearchPlanViewList = new List<ListViewItem>();
            MetadataPackageModel = new List<MetadataPackageModel>();
        }
    }
}