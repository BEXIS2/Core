﻿using System.ComponentModel;
using BExIS.Utils.Filters;
using System.ComponentModel.DataAnnotations;
using BExIS.Security.Entities.Subjects;

namespace BExIS.Web.Shell.Models
{
    public class ExternalLoginConfirmationViewModel
    {
        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }
    }

    public class ExternalLoginListViewModel
    {
        public string Action { get; set; }
        public string ReturnUrl { get; set; }
    }

    public class ForgotPasswordViewModel
    {
        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }
    }

    public class LoginConfirmModel
    {
        [Required]
        public long Id { get; set; }

        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Display(Name = "Terms and Conditions")]
        [MustBeTrue(ErrorMessage = "You must agree to the Terms and Conditions before register.")]
        public bool TermsAndConditions { get; set; }

        [Display(Name = "Privacy Policy")]
        [MustBeTrue(ErrorMessage = "You must agree to the Privacy Policy before register.")]
        public bool PrivacyPolicy { get; set; }

        public static LoginConfirmModel Convert(User user)
        {
            return new LoginConfirmModel()
            {
                Id = user.Id,
                Email = user.Email,
                TermsAndConditions = user.HasTermsAndConditionsAccepted,
                PrivacyPolicy = user.HasPrivacyPolicyAccepted
            };
        }
    }

    public class LoginViewModel
    {
        [Display(Name = "Authenticator")]
        public string Authenticator { get; set; }

        [Required]
        [Display(Name = "Email or Username")]
        public string UserName { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        [System.Web.Mvc.AllowHtml]
        public string Password { get; set; }

        [Display(Name = "Remember me")]
        public bool RememberMe { get; set; }
    }

    public class ManageUserViewModel
    {
        [DataType(DataType.Password)]
        [Display(Name = "Confirm new password")]
        [Compare("NewPassword", ErrorMessage = "The new password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "New password")]
        public string NewPassword { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Current password")]
        public string OldPassword { get; set; }
    }

    public class RegisterViewModel
    {
        [Required]
        [Display(Name = "Username")]
        public string UserName { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }

        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        [System.Web.Mvc.AllowHtml]
        public string Password { get; set; }

        [Display(Name = "Terms and Conditions")]
        [MustBeTrue(ErrorMessage = "You must agree to the Terms and Conditions before register.")]
        public bool TermsAndConditions { get; set; }

        [Display(Name = "Privacy Policy")]
        [MustBeTrue(ErrorMessage = "You must agree to the Privacy Policy before register.")]
        public bool PrivacyPolicy { get; set; }
    }

    public class ResetPasswordViewModel
    {
        public string Code { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }

        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }
    }
}