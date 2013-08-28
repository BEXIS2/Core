using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using BExIS.Dlm.Services.DataStructure;
using BExIS.Dlm.Entities.DataStructure;

namespace BExIS.Web.Shell.Areas.DCM.Models
{
    public class CreateDatasetViewModel
    {
        [Required(ErrorMessage = "Please enter a title.")]
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
        [Required(ErrorMessage = "Please select a datastructure id.")]
        public long DataStructureId { get; set; }

        [Display(Name = "Research Plan")]
        [Required(ErrorMessage = "Please select a research plan.")]
        public long ResearchPlanId { get; set; }

        public List<long> DataStructureIds { get; set; }
        public List<ListViewItem> DatastructuresViewList { get; set; }

        public List<ListViewItem> ResearchPlanViewList { get; set; }

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

        }

       
    }
}