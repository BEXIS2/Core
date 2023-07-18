using BExIS.IO.Transform.Validation.Exceptions;
using BExIS.UI.Models;
using System;
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

    public class SortedError
    {
        public string Name { get; set; }
        public int Count { get; set; }
        public string Issue { get; set; }
        public ErrorType Type { get; set; }

        public List<string> Errors { get; set; }

        public SortedError()
        {
            Name =  string.Empty;
            Count = 0;
            Issue = string.Empty;
            Type = ErrorType.Other;
            Errors = new List<string>();
        }
        public SortedError(string name, int count, string issue, ErrorType type, List<string> errors)
        {
            Name = name;
            Count = count;
            Issue = issue;
            Type = type;
            Errors = errors;
        }
    
    }
}