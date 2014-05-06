using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using BExIS.Dcm.Wizard;
using BExIS.Io.Transform.Validation.Exceptions;

namespace BExIS.Web.Shell.Areas.DCM.Models.Create
{
    public class SelectDatasetSetupModel
    {
        [Display(Name = "Data Structure")]
        [Required(ErrorMessage = "Please select a data structure.")]
        public long SelectedDatastructureId { get; set; }

        [Display(Name = "Research Plan")]
        [Required(ErrorMessage = "Please select a research plan.")]
        public long SelectedResearchPlanId { get; set; }

        [Display(Name = "Metadata Structure")]
        [Required(ErrorMessage = "Please select a metadata structure.")]
        public long SelectedMetadatStructureId { get; set; }

        public StepInfo StepInfo { get; set; }
        public List<Error> ErrorList { get; set; }

        public List<ListViewItem> DatastructuresViewList { get; set; }
        public List<ListViewItem> ResearchPlanViewList { get; set; }
        public List<ListViewItem> MetadataStructureViewList { get; set; }

        public SelectDatasetSetupModel()
        {
            SelectedDatastructureId = 0;
            SelectedResearchPlanId = 0;
            SelectedMetadatStructureId = 0;

            ErrorList = new List<Error>();

            DatastructuresViewList = new List<ListViewItem>();
            ResearchPlanViewList = new List<ListViewItem>();
            MetadataStructureViewList = new List<ListViewItem>();
        }
    }
}