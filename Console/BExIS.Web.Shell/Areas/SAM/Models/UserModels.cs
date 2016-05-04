using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using BExIS.Security.Entities.Subjects;
using DataAnnotationsExtensions;

namespace BExIS.Web.Shell.Areas.SAM.Models
{
    public class UserChangePasswordModel
    {
        public long UserId { get; set; }

        [Required]
        public string Password { get; set; }
    }

    public class UserCreateModel
    {
        [Display(Name = "Username")]
        [RegularExpression("^[\\S]*$", ErrorMessage = "The user name must not contain spaces.")]
        [Remote("ValidateUsername", "Users")]
        [Required]
        [StringLength(64, ErrorMessage = "The user name must be {2} - {1} characters long.", MinimumLength = 3)]
        public string Username { get; set; }

        [Display(Name = "Password")]
        [RegularExpression("^[\\S]*$", ErrorMessage = "The password is invalid.")]
        [Required]
        [StringLength(24, ErrorMessage = "The password must be {2} - {1} characters long.", MinimumLength = 6)]
        public string Password { get; set; }

        [global::System.Web.Mvc.Compare("Password", ErrorMessage = "The password and confirmation do not match.")]
        [Display(Name = "Confirm Password")]
        [Required]
        public string ConfirmPassword { get; set; }

        [Display(Name = "Full Name")]
        [Required]
        public string FullName { get; set; }

        [Display(Name = "Email Address")]
        [Email]
        [Remote("ValidateEmail", "Users")]
        [Required]
        [StringLength(250, ErrorMessage = "The email must be {2} - {1} characters long.", MinimumLength = 5)]
        public string Email { get; set; }

        [Display(Name = "Security Answer")]
        [RegularExpression("^[^\\s]+(\\s+[^\\s]+)*", ErrorMessage = "The security answer must start and end with no space.")]
        [Required]
        [StringLength(50, ErrorMessage = "The security answer must be less than {1} characters long.")]
        public string SecurityAnswer { get; set; }

        public SecurityQuestionSelectListModel SecurityQuestionList { get; set; }

        public AuthenticatorSelectListModel AuthenticatorList { get; set; }

        public UserCreateModel()
        {
            SecurityQuestionList = new SecurityQuestionSelectListModel();
            AuthenticatorList = new AuthenticatorSelectListModel(true);
        }
    }

    public class UserEditModel
    {
        public long UserId { get; set; }

        [Required]
        public string Username { get; set; }

        [Required]
        public string FullName { get; set; }

        [Required]
        [Email]
        public string Email { get; set; }

        public string Password { get; set; }

        [global::System.Web.Mvc.Compare("Password", ErrorMessage = "The password and confirmation do not match.")]
        [Display(Name = "Confirm Password")]
        public string ConfirmPassword { get; set; }

        [Display(Name = "Approved")]
        public bool IsApproved { get; set; }

        [Display(Name = "Blocked")]
        public bool IsBlocked { get; set; }

        [Display(Name = "Locked Out")]
        public bool IsLockedOut { get; set; }

        public static UserEditModel Convert(User user)
        {
            return new UserEditModel()
            {
                UserId = user.Id,
                Username = user.Name,
                FullName = user.FullName,
                Email = user.Email,

                IsApproved = user.IsApproved,
                IsBlocked = user.IsBlocked,
                IsLockedOut = user.IsLockedOut,
            };
        }
    }

    public class UserGridRowModel
    {
        public long Id { get; set; }

        public string Username { get; set; }

        public string FullName { get; set; }

        public string Email { get; set; }

        public bool IsApproved { get; set; }

        public bool IsBlocked { get; set; }

        public bool IsLockedOut { get; set; }

        public static UserGridRowModel Convert(User user)
        {
            return new UserGridRowModel()
            {
                Id = user.Id,
                Username = user.Name,
                FullName = user.FullName,
                Email = user.Email,

                IsApproved = user.IsApproved,
                IsBlocked = user.IsBlocked,
                IsLockedOut = user.IsLockedOut
            };
        }
    }

    public class UserMembershipGridRowModel
    {
        public long Id { get; set; }

        public string GroupName { get; set; }

        public string Description { get; set; }

        public bool IsUserInGroup { get; set; }

        public static UserMembershipGridRowModel Convert(Group group, bool isUserInGroup)
        {
            return new UserMembershipGridRowModel()
            {
                Id = group.Id,
                GroupName = group.Name,
                Description = group.Description,

                IsUserInGroup = isUserInGroup
            };
        }
    }
}