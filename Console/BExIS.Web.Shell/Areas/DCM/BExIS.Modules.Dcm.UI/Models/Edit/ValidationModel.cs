using BExIS.UI.Models;
using System;
using System.Collections.Generic;

namespace BExIS.Modules.Dcm.UI.Models.Edit
{
    public class ValidationModel
    {
        public bool IsValid { get; set; }
        public List<string> Errors { get; set; }
        public List<Tuple<string, int, string>> SortedErrors { get; set; }

    

        public ValidationModel()
        {
            IsValid = false;
            Errors = new List<string>();
            SortedErrors = new List<Tuple<string, int, string>>();

        }
    }
}