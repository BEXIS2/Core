using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace BExIS.Utils.Filters
{
    [AttributeUsage(AttributeTargets.Property)]
    public class MustBeTrueAttribute : ValidationAttribute, IClientValidatable
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if ((bool)value)
                return ValidationResult.Success;
            return new ValidationResult(string.Format(ErrorMessageString, validationContext.DisplayName));
        }

        public IEnumerable<ModelClientValidationRule> GetClientValidationRules(ModelMetadata metadata, ControllerContext context)
        {
            var rule = new ModelClientValidationRule
            {
                ErrorMessage = FormatErrorMessage(metadata.GetDisplayName()),
                ValidationType = "checkrequired"
            };

            yield return rule;
        }
    }
}