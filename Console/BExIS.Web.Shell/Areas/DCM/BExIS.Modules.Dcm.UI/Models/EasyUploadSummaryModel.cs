using BExIS.Dcm.Wizard;
using BExIS.IO.Transform.Validation.Exceptions;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BExIS.Modules.Dcm.UI.Models
{
    public class EasyUploadSummaryModel
    {
        [Display(Name = "Dataset Id")]
        public int DatasetId { get; set; }

        [Display(Name = "Dataset Title")]
        public String DatasetTitle { get; set; }

        [Display(Name = "Datastructure Id")]
        public int DataStructureId { get; set; }

        [Display(Name = "Datastructure Title")]
        public String DataStructureTitle { get; set; }

        [Display(Name = "Research Plan Id")]
        public int ResearchPlanId { get; set; }

        [Display(Name = "Research Plan Title")]
        public String ResearchPlanTitle { get; set; }

        [Display(Name = "Metadata Schema Title")]
        public String MetadataSchemaTitle { get; set; }

        [Display(Name = "File Format")]
        public String FileFormat { get; set; }

        [Display(Name = "Number Of Headers")]
        public int NumberOfHeaders { get; set; }

        [Display(Name = "Number Of Data rows/columns")]
        public int NumberOfData { get; set; }

        /*
        [Display(Name = "Dataset Owner")]
        public String  Owner { get; set; }

        [Display(Name = "Dataset Author")]
        public String  Author { get; set; }

        [Display(Name = "Project Name")]
        public String  ProjectName { get; set; }

        [Display(Name = "Organisation")]
        public String  ProjectInstitute { get; set; }

        [Display(Name = "Number of Variables")]
        public int NumOfVars { get; set; }

        [Display(Name = "Number of Rows")]
        public int NumOfRows { get; set; }*/

        public StepInfo StepInfo { get; set; }
        public List<Error> ErrorList = new List<Error>();

        public EasyUploadSummaryModel()
        {
            DatasetId = 0;
            DataStructureId = 0;
            DatasetTitle = "";
            DataStructureTitle = "";
            MetadataSchemaTitle = "";
            FileFormat = "";
            NumberOfHeaders = 0;
            NumberOfData = 0;
        }
    }
}