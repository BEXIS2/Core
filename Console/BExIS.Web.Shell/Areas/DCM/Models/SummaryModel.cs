using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace BExIS.Web.Shell.Areas.DCM.Models
{
    public class SummaryModel
    {
        [Display(Name = "Dataset Id")]
        public int DatasetId { get; set; }

        [Display(Name = "Dataset Title")]
        public String  DatasetTitle { get; set; }

        [Display(Name = "Datastructure Id")]
        public int DataStructureId { get; set; }

        [Display(Name = "Datastructure Title")]
        public String DataStructureTitle { get; set; }

        [Display(Name = "Research Plan Id")]
        public int ResearchPlanId { get; set; }

        [Display(Name = "Research Plan Title")]
        public String ResearchPlanTitle { get; set; }

        [Display(Name = "Number of Variables")]
        public int NumOfVars { get; set; }

        [Display(Name = "Number of Rows")]
        public int NumOfRows { get; set; }

        public StepInfo StepInfo { get; set; }


        public SummaryModel()
        {
            DatasetId = 0;
            DataStructureId = 0;
            DatasetTitle = "";
            DataStructureTitle = "";
            NumOfRows = 0;
            NumOfVars = 0;
        }
    }
}