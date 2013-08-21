using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace BExIS.Web.Shell.Areas.DCM.Models
{
    public class CreateDatasetViewModel
    {
        [Required(ErrorMessage = "Please enter a title.")]
        public string Title { get; set; }

        [Display(Name = "Dataset Author")]
        [Required(ErrorMessage = "Please enter a dataset author.")]
        public string DatasetAuthor { get; set; }

        [Display(Name = "Owner")]
        [Required(ErrorMessage = "Please enter a owner.")]
        public string Owner { get; set; }

        [Display(Name = "Project Name")]
        [Required(ErrorMessage = "Please enter a project name.")]
        public string ProjectName { get; set; }

        [Display(Name = "Project Institute")]
        [Required(ErrorMessage = "Please enter a project institute.")]
        public string ProjectInstitute { get; set; }

        [Display(Name = "Datastructure Id")]
        [Required(ErrorMessage = "Please select a datastructure id.")]
        public long DataStructureId { get; set; }

        public List<long> DataStructureIds { get; set; }

        public CreateDatasetViewModel()
        {
            Title = "";
            DatasetAuthor = "";
            Owner = "";
            ProjectName = "";
            ProjectInstitute = "";
            DataStructureIds = new List<long>();

        }
    }
}