using System.Collections.Generic;

namespace BExIS.Modules.Rpm.UI.Models
{
    public class ValidationResult
    {
        public bool IsValid { get; set; }
        public List<ValidationItem> ValidationItems { get; set; }

        public ValidationResult()
        {
            IsValid = true;
            ValidationItems = new List<ValidationItem>();
        }
    }

    public class ValidationItem
    {
        public string Name { get; set; }
        public string Message { get; set; }

        public ValidationItem()
        {
            Name = string.Empty;
            Message = string.Empty;
        }
    }
}