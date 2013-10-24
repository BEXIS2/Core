using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using BExIS.DCM.Transform.Validation.Exceptions;
using BExIS.DCM.UploadWizard;

namespace BExIS.Web.Shell.Areas.DCM.Models
{
    public class PrimaryKeyViewModel
    {
        public List<Error> ErrorList { get; set; }
        public List<ListViewItem> VariableLableList { get; set; }
        public List<ListViewItem> PrimaryKeysList { get; set; }
        public StepInfo StepInfo { get; set; }
        public bool IsUnique { get; set; }

        public PrimaryKeyViewModel()
        {
            ErrorList = new List<Error>();
            VariableLableList = new List<ListViewItem>();
            PrimaryKeysList = new List<ListViewItem>();
            IsUnique = false;
        }
    }
}