using System.Collections.Generic;
using BExIS.Dcm.Wizard;
using BExIS.IO.Transform.Validation.Exceptions;

namespace BExIS.Modules.Dcm.UI.Models
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