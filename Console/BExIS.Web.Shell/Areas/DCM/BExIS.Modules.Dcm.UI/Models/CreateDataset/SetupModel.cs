using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BExIS.Modules.Dcm.UI.Models.CreateDataset
{
    public enum DataStructureOptions
    {
        [Display(Name = "Tabular data (new)")]
        CreateNewStructure,

        [Display(Name = "File (new)")]
        CreateNewFile,

        [Display(Name = "Existing tabular data structure")]
        Existing_structured,

        [Display(Name = "Existing file data structure")]
        Existing_unstructured
    }

    public class SetupModel
    {
        [Display(Name = "Dataset")]
        [Required]
        public long SelectedDatasetId { get; set; }

        [Display(Name = "Metadata Structure")]
        [Required(ErrorMessage = "Please select a metadata structure.")]
        public long SelectedMetadataStructureId { get; set; }

        [Display(Name = "Data Structure")]
        public long SelectedDataStructureId { get; set; }

        public long SelectedDataStructureId_ { get; set; }

        public List<ListViewItem> MetadataStructureViewList { get; set; }

        public List<ListViewItemWithType> DataStructureViewList_unstructured { get; set; }

        public List<ListViewItem> DatasetViewList { get; set; }

        public bool BlockDatasetId { get; set; }
        public bool BlockDatastructureId { get; set; }
        public bool BlockMetadataStructureId { get; set; }

        public DataStructureOptions DataStructureOptions { get; set; }
        public List<ListViewItemWithType> DataStructureViewList_structured { get; internal set; }

        public SetupModel()
        {
            SelectedMetadataStructureId = -1;
            MetadataStructureViewList = new List<ListViewItem>();
            BlockMetadataStructureId = false;

            SelectedDataStructureId = -1;
            SelectedDataStructureId_ = -1;
            DataStructureViewList_unstructured = new List<ListViewItemWithType>();
            BlockDatastructureId = false;

            SelectedDatasetId = -1;
            DatasetViewList = new List<ListViewItem>();
            BlockDatasetId = false;

            DataStructureOptions = DataStructureOptions.CreateNewStructure;
        }
    }
}