using System;
using System.Collections;
using System.ComponentModel.DataAnnotations;

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