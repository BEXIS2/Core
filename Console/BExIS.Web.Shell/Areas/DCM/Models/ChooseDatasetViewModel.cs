using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;


namespace BExIS.Web.Shell.Areas.DCM.Models
{
    public class ChooseDatasetViewModel
    {
        public StepInfo StepInfo { get; set; }
        public int SelectedDatasetId { get; set; }
        public int SelectedDatastructureId { get; set; }

      
        public List<long> Datasets { get; set; }
        public List<long> Datastructures { get; set; }

        public CreateDatasetViewModel DatasetViewModel { get; set; }

        public ChooseDatasetViewModel()
        {
            Datasets = new List<long>();
            Datastructures = new List<long>();
        }
    }
}