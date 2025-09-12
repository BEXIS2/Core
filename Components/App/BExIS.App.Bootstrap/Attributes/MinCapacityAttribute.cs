using System;
using System.Collections;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace BExIS.App.Bootstrap.Attributes
{
    public class MinCapacityAttribute : ValidationAttribute
    {
        private readonly int _minSize;

        public MinCapacityAttribute(int minSize)
        {
            _minSize = minSize;
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value is ICollection collection)
            {
                if (collection.Cast<object>().Any(item => item is string s && string.IsNullOrEmpty(s)))
                {
                    return new ValidationResult("Collection cannot contain null or empty strings.");
                }

                if (collection.Count >= _minSize)
                {
                    return ValidationResult.Success;
                }
                else
                {
                    return new ValidationResult($"The collection must contain at least {_minSize} elements.");
                }
            }

            return new ValidationResult("This attribute can only be used on ICollection types.");
        }
    }
}

