using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BExIS.Modules.Dcm.UI.Models.CreateDataset
{
    public enum DataStructureOptions
    {
        Existing,

        [Display(Name = "New File")]
        CreateNewFile,

        [Display(Name = "New Data Structure")]
        CreateNewStructure
    }

    public class SetupModel
    {
        [Display(Name = "Dataset")]
        public long SelectedDatasetId { get; set; }

        [Display(Name = "Metadata Structure")]
        [Required(ErrorMessage = "Please select a metadata structure.")]
        public long SelectedMetadataStructureId { get; set; }

        [Display(Name = "Data Structure")]
        public long SelectedDataStructureId { get; set; }

        public List<ListViewItem> MetadataStructureViewList { get; set; }

        public List<ListViewItemWithType> DataStructureViewList { get; set; }

        public List<ListViewItem> DatasetViewList { get; set; }

        public bool BlockDatasetId { get; set; }
        public bool BlockDatastructureId { get; set; }
        public bool BlockMetadataStructureId { get; set; }

        public DataStructureOptions DataStructureOptions { get; set; }

        public SetupModel()
        {
            SelectedMetadataStructureId = -1;
            MetadataStructureViewList = new List<ListViewItem>();
            BlockMetadataStructureId = false;

            SelectedDataStructureId = -1;
            DataStructureViewList = new List<ListViewItemWithType>();
            BlockDatastructureId = false;

            SelectedDatasetId = -1;
            DatasetViewList = new List<ListViewItem>();
            BlockDatasetId = false;

            DataStructureOptions = DataStructureOptions.Existing;
        }
    }
}