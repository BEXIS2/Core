using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using BExIS.Io.Transform.Validation.Exceptions;
using BExIS.Dcm.Wizard;

namespace BExIS.Web.Shell.Areas.DCM.Models
{
    public class PrimaryKeyViewModel
    {
        public List<Error> ErrorList { get; set; }
        public List<ListViewItem> VariableLableList { get; set; }
        public List<ListViewItem> PrimaryKeysList { get; set; }
        public List<long> PK_Id_List { get; set; }
        public StepInfo StepInfo { get; set; }
        public bool IsUnique { get; set; }

        public PrimaryKeyViewModel()
        {
            ErrorList = new List<Error>();
            VariableLableList = new List<ListViewItem>();
            PrimaryKeysList = new List<ListViewItem>();
            PK_Id_List = new List<long>();
            IsUnique = false;
        }
    }
}