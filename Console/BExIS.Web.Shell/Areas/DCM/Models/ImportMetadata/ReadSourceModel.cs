using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using BExIS.Dcm.Wizard;

namespace BExIS.Web.Shell.Areas.DCM.Models.ImportMetadata
{
    public class ReadSourceModel:AbstractStepModel
    {
        
        [Display(Name = "Schema name")]
        [Required(ErrorMessage = "Please define a name for the metadata structure.")]
        public string SchemaName { get; set; }

        [Display(Name = "Root node" ) ]
        public string RootNode { get; set; }

        public bool IsGenerated { get; set; }

        public ReadSourceModel(StepInfo stepInfo)
        { 
            this.StepInfo = stepInfo;
            RootNode = "";
            SchemaName = "";
            IsGenerated = false;
        }
    }
}