using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace BExIS.Web.Shell.Areas.DCM.Models.CreateDataset
{
    public class SetupModel
    {
        [Display(Name = "Metadata Structure")]
        [Required(ErrorMessage = "Please select a metadata structure.")]
        public long SelectedMetadatStructureId { get; set; }

        [Display(Name = "Data Structure")]
        [Required(ErrorMessage = "Please select a data structure.")]
        public long SelectedDataStructureId { get; set; }

        public List<ListViewItem> MetadataStructureViewList { get; set; }

        public List<ListViewItem> DataStructureViewList { get; set; }

        public SetupModel()
        {
            SelectedMetadatStructureId = 0;
            MetadataStructureViewList = new List<ListViewItem>();

            SelectedDataStructureId = 0;
            DataStructureViewList = new List<ListViewItem>(); 
        }
    }
}