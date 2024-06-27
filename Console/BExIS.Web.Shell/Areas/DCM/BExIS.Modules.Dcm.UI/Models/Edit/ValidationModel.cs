using System.Collections.Generic;

namespace BExIS.Modules.Dcm.UI.Models.Edit
{
    public class ValidationModel
    {
        public bool IsValid { get; set; }

        public List<FileValidationResult> FileResults { get; set; }

        public ValidationModel()
        {
            IsValid = false;
            FileResults = new List<FileValidationResult>();
        }
    }

    public class FileValidationResult
    {
        public string File { get; set; }
        public List<string> Errors { get; set; }
        public List<SortedError> SortedErrors { get; set; }

        public FileValidationResult()
        {
            Errors = new List<string>();
            SortedErrors = new List<SortedError>();
            File = "";
        }
    }
}