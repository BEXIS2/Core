using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using BExIS.Security.Entities.Security;
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

        public AuthenticatorSelectListModel AuthenticatorList { get; set; }

        public LogOnModel()
        {
            AuthenticatorList = new AuthenticatorSelectListModel();
        }
    }

    public class AccountRegistrationModel
    {
        [Display(Name = "User name")]
        [RegularExpression("^[\\S]*$", ErrorMessage = "The user name is invalid.")]
        [Remote("ValidateUserName", "Account")]
        [Required(AllowEmptyStrings = false)]
        [StringLength(50, ErrorMessage = "The user name must be {2} - {1} characters long.", MinimumLength = 3)]
        public string UserName { get; set; }

        [Display(Name = "Email address")]
        [Email]
        [Remote("ValidateEmail", "Account")]
        [Required]
        [StringLength(250, ErrorMessage = "The email must be {2} - {1} characters long.", MinimumLength = 5)]
        public string Email { get; set; }

        [Display(Name = "Password")]
        [RegularExpression("^[\\S]*$", ErrorMessage = "The password is invalid.")]
        [Required]
        [StringLength(24, ErrorMessage = "The password must be {2} - {1} characters long.", MinimumLength = 6)]
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
        [RegularExpression("^[^\\s]+(\\s+[^\\s]+)*", ErrorMessage = "The security answer must start and end with no space.")]
        [Required]
        [StringLength(50, ErrorMessage = "The security answer must be less than {1} characters long.")]
        public string SecurityAnswer { get; set; }
    }
}