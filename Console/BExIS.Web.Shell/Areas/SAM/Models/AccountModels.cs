using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using BExIS.Security.Entities.Subjects;
using BExIS.Web.Shell.Areas.SAM.Helpers;
using DataAnnotationsExtensions;

namespace BExIS.Web.Shell.Areas.SAM.Models
{
    public class AccountRegisterModel
    {
        [Display(Name = "Username")]
        [RegularExpression("^[\\S]*$", ErrorMessage = "Username must be without spaces.")]
        [Remote("ValidateUsername", "Account")]
        [Required(AllowEmptyStrings = false, ErrorMessage = "Username is required.")]
        [StringLength(64, ErrorMessage = "Username must be {2} - {1} characters long.", MinimumLength = 3)]
        public string Username { get; set; }

        [Display(Name = "Password")]
        [RegularExpression("^[\\S]*$", ErrorMessage = "The Password must not contain spaces.")]
        [Required(ErrorMessage = "Password is required.")]
        [StringLength(24, ErrorMessage = "Password must be {2} - {1} characters long.", MinimumLength = 6)]
        public string Password { get; set; }

        [global::System.Web.Mvc.Compare("Password", ErrorMessage = "Password and Confirm Password do not match.")]
        [Display(Name = "Confirm Password")]
        [Required(ErrorMessage = "Confirm Password is required.")]
        public string ConfirmPassword { get; set; }

        [Display(Name = "Full Name")]
        [Required(ErrorMessage = "Full Name is required.")]
        public string FullName { get; set; }

        [Display(Name = "Email Address")]
        [Email]
        [Remote("ValidateEmail", "Account")]
        [Required(ErrorMessage = "Email Address is required.")]
        [StringLength(250, ErrorMessage = "Email Address must be {2} - {1} characters long.", MinimumLength = 5)]
        public string Email { get; set; }

        [Display(Name = "Security Question")]
        [Required(ErrorMessage = "Security Question is required.")]
        public long SecurityQuestion { get; set; }

        [Display(Name = "Security Answer")]
        [RegularExpression("^[^\\s]+(\\s+[^\\s]+)*", ErrorMessage = "Security Answer must start and end with no space.")]
        [Required(ErrorMessage = "Security Answer is required.")]
        [StringLength(50, ErrorMessage = "Security Answer must be less than {1} characters long.")]
        public string SecurityAnswer { get; set; }

        public SecurityQuestionSelectListModel SecurityQuestionList { get; set; }

        public AuthenticatorSelectListModel AuthenticatorList { get; set; }

        [Display(Name = "Terms and Conditions")]
        [MustBeTrue(ErrorMessage = "You must agree to the Terms and Conditions before register.")]
        public bool TermsAndConditions { get; set; }

        public AccountRegisterModel()
        {
            SecurityQuestionList = new SecurityQuestionSelectListModel();
            AuthenticatorList = new AuthenticatorSelectListModel(true);
        }
    }

    public class MyAccountModel
    {
        [Display(Name = "User Id")]
        public long UserId { get; set; }

        public long AuthenticatorId { get; set; }

        [Display(Name = "Username")]
        public string Username { get; set; }

        [Display(Name = "Password")]
        [RegularExpression("^[\\S]*$", ErrorMessage = "The Password must not contain spaces.")]
        [StringLength(24, ErrorMessage = "The Password must be {2} - {1} characters long.", MinimumLength = 6)]
        public string Password { get; set; }

        [global::System.Web.Mvc.Compare("Password", ErrorMessage = "The Password and Confirm Password do not match.")]
        [Display(Name = "Confirm Password")]
        public string ConfirmPassword { get; set; }

        [Display(Name = "Full Name")]
        [Required]
        public string FullName { get; set; }

        [Display(Name = "Email Address")]
        [Email]
        [Remote("ValidateEmail", "Account", AdditionalFields = "UserId")]
        [Required]
        [StringLength(250, ErrorMessage = "The email must be {2} - {1} characters long.", MinimumLength = 5)]
        public string Email { get; set; }

        [Display(Name = "Security Answer")]
        [RegularExpression("^[^\\s]+(\\s+[^\\s]+)*", ErrorMessage = "The Security Answer must start and end with no space.")]
        [StringLength(50, ErrorMessage = "The Security Answer must be less than {1} characters long.")]
        public string SecurityAnswer { get; set; }

        public long SecurityQuestionId { get; set; }

        public SecurityQuestionSelectListModel SecurityQuestionList { get; set; }

        public AuthenticatorSelectListModel AuthenticatorList { get; set; }

        public MyAccountModel()
        {
            SecurityQuestionList = new SecurityQuestionSelectListModel();
            AuthenticatorList = new AuthenticatorSelectListModel(true);
        }

        public static MyAccountModel Convert(User user)
        {
            return new MyAccountModel()
            {
                UserId = user.Id,
                Username = user.Name,
                FullName = user.FullName,
                Email = user.Email,
                AuthenticatorId = user.Authenticator.Id,
                SecurityQuestionId = user.Authenticator.Id == 1 ? user.SecurityQuestion.Id : 0
            };
        }
    }

    public class AccountLogOnModel
    {
        [Required]
        [Display(Name = "Username")]
        public string Username { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        public AuthenticatorSelectListModel AuthenticatorList { get; set; }

        public AccountLogOnModel()
        {
            AuthenticatorList = new AuthenticatorSelectListModel();
        }
    }

    public class ChangePasswordModel
    {
        public string Username { get; set; }

        public string SecurityQuestion { get; set; }

        public string SecurityAnswer { get; set; }

        public string Password { get; set; }

        public string ConfirmPassword { get; set; }
    }

    public class ResetPasswordModel
    {
        public string Username { get; set; }
    }
}