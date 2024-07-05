using BExIS.Dcm.Wizard;
using BExIS.IO.Transform.Validation.Exceptions;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BExIS.Modules.Dcm.UI.Models
{
    public class SummaryModel
    {
        //Dataset
        [Display(Name = "Dataset Id")]
        public int DatasetId { get; set; }

        [Display(Name = "Dataset Title")]
        public String DatasetTitle { get; set; }

        [Display(Name = "Dataset Status")]
        public String DatasetStatus { get; set; }

        //Datastructure
        [Display(Name = "Datastructure Id")]
        public int DataStructureId { get; set; }

        [Display(Name = "Datastructure Title")]
        public String DataStructureTitle { get; set; }

        [Display(Name = "Datastructure Type")]
        public string DataStructureType { get; set; }

        //file
        [Display(Name = "Name")]
        public string Filename { get; set; }

        [Display(Name = "Path")]
        public string Filepath { get; set; }

        [Display(Name = "Extention")]
        public string Extention { get; set; }

        //if structured
        [Display(Name = "Upload Method")]
        public string UploadMethod { get; set; }

        [Display(Name = "Number of Rows")]
        public int NumberOfRows { get; set; }

        [Display(Name = "Number of Variables")]
        public int NumberOfVariables { get; set; }

        //if structred & update
        [Display(Name = "Primary Keys")]
        public string PrimaryKeys { get; set; }

        //Sync or ASync updload
        [Display(Name = "Async")]
        public bool AsyncUpload { get; set; }

        public string AsyncUploadMessage { get; set; }

        public StepInfo StepInfo { get; set; }
        public List<Error> ErrorList = new List<Error>();

        public SummaryModel()
        {
            DatasetId = 0;
            DataStructureId = 0;
            DatasetTitle = "";
            DataStructureTitle = "";
            AsyncUpload = false;
            AsyncUploadMessage = "";
        }
    }
}