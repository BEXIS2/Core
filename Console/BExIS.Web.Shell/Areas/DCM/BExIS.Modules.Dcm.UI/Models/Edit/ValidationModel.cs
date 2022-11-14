using BExIS.UI.Models;
using System;
using System.Collections.Generic;

namespace BExIS.Modules.Dcm.UI.Models.Edit
{
    public class ValidationModel
    {
        public bool IsValid { get; set; }

        public List<FileErrors> FileErrors { get; set; }


        public ValidationModel()
        {
            IsValid = false;
            FileErrors = new List<FileErrors>();
        }

    }


    public class FileErrors
    {
        public string File { get; set; }
        public List<string> Errors { get; set; }
        public List<Tuple<string, int, string>> SortedErrors { get; set; }

        public FileErrors()
        {
            Errors = new List<string>();
            SortedErrors = new List<Tuple<string, int, string>>();
            File = "";
        }

    }
}