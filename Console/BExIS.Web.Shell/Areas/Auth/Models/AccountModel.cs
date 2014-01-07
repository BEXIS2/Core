using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using DataAnnotationsExtensions;

namespace BExIS.Web.Shell.Areas.Auth.Models
{
    public class ChangePasswordModel
    {
        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Current password")]
        public string OldPassword { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "New password")]
        public string NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm new password")]
        [Compare("NewPassword", ErrorMessage = "The new password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }
    }

    public class LogOnModel
    {
        [Required]
        [Display(Name = "User name")]
        public string UserName { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [Display(Name = "Remember me?")]
        public bool RememberMe { get; set; }
    }

    public class RegistrationModel
    {
        [Display(Name = "User name")]
        [RegularExpression("^([A-Za-z]+)$", ErrorMessage = "The user name must consist only of letters.")]
        [Remote("ValidateUserName", "Users")]
        [Required]
        [StringLength(50, ErrorMessage = "The user name must be {2} - {1} characters long.", MinimumLength = 6)]
        public string UserName { get; set; }

        [Display(Name = "Email address")]
        [Email]
        [Remote("ValidateEmail", "Users")]
        [Required]
        [StringLength(250, ErrorMessage = "The email must be {2} - {1} characters long.", MinimumLength = 5)]
        public string Email { get; set; }

        [Display(Name = "Password")]
        [Required]
        [StringLength(50, ErrorMessage = "The password must be {2} - {1} characters long.", MinimumLength = 6)]
        public string Password { get; set; }

        [Compare("Password", ErrorMessage = "The password and confirmation do not match.")]
        [Display(Name = "Confirm Password")]
        [Required]
        public string ConfirmPassword { get; set; }

        [Display(Name = "Security Question")]
        [Required]
        [StringLength(250)]
        public string SecurityQuestion { get; set; }

        [Display(Name = "Security Answer")]
        [Required]
        [StringLength(50, ErrorMessage = "The security answer must be {2} - {1} characters long.", MinimumLength = 6)]
        public string SecurityAnswer { get; set; }

        [Display(Name = "Confirm Security Answer")]
        [Compare("SecurityAnswer", ErrorMessage = "The security answer and confirmation do not match.")]
        [Required]
        public string ConfirmSecuritydAnswer { get; set; }
    }
}