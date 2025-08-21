using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BExIS.App.Bootstrap.Attributes
{
    public class NoNullOrEmptyItemsAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value is ICollection collection)
            {
                if (collection.Cast<object>().Any(item => item is string s && string.IsNullOrEmpty(s)))
                    return new ValidationResult("Collection cannot contain null or empty strings.");

                return ValidationResult.Success;
            }

            return new ValidationResult("This attribute can only be used on ICollection types.");
        }
    }
}
