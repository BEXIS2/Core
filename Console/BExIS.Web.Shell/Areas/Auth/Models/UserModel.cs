using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using BExIS.Security.Entities.Subjects;
using DataAnnotationsExtensions;

namespace BExIS.Web.Shell.Areas.Auth.Models
{
    public class UserCreationModel
    {
        [Display(Name = "User name")]
        [RegularExpression("^[^\\s]+(\\s+[^\\s]+)*$", ErrorMessage = "The user name must start and end with no space.")]
        [Remote("ValidateUserName", "Users")]
        [Required]
        [StringLength(50, ErrorMessage = "The user name must be {2} - {1} characters long.", MinimumLength = 3)]
        public string UserName { get; set; }

        [Display(Name = "Email address")]
        [Email]
        [Remote("ValidateEmail", "Users")]
        [Required]
        public string Email { get; set; }

        [Display(Name = "Password")]
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
        [RegularExpression("^[^\\s]+(\\s+[^\\s]+)*$", ErrorMessage = "The security answer must start and end with no space.")]
        [Required]
        [StringLength(50, ErrorMessage = "The security answer must be less than {1} characters long.")]
        public string SecurityAnswer { get; set; }
    }

    public class UserModel
    {
        [Display(Name = "User ID")]
        [Editable(false)]
        [Required]
        public long Id { get; set; }

        [Display(Name = "User Name")]
        [Editable(false)]
        [Required]
        public string UserName { get; set; }

        [Display(Name = "Email")]
        [Email]
        [Remote("ValidateEmail", "Users", AdditionalFields = "Id")]
        [Required]
        public string Email { get; set; }

        [Display(Name = "Registration Date")]
        [Editable(false)]
        public DateTime RegistrationDate { get; set; }

        [Display(Name = "Last Login Date")]
        [Editable(false)]
        public DateTime LastLoginDate { get; set; }

        [Display(Name = "Last Activity Date")]
        [Editable(false)]
        public DateTime LastActivityDate { get; set; }

        [Display(Name = "Is Approved")]
        public bool IsApproved { get; set; }

        [Display(Name = "Is Locked Out")]
        public bool IsLockedOut { get; set; }

        public static UserModel Convert(User user)
        {
            return new UserModel()
            {
                Id = user.Id,
                UserName = user.Name,
                Email = user.Email,
                RegistrationDate = user.RegistrationDate,
                LastLoginDate = user.LastLoginDate,
                LastActivityDate = user.LastActivityDate
            };
        }
    }

    public class UserRoleModel
    {
        // User
        public long UserId { get; set; }

        // Role
        public long Id { get; set; }
        public string RoleName { get; set; }
        public string Description { get; set; }

        // Membership
        public bool UserInRole { get; set; }

        public static UserRoleModel Convert(long userId, Role role, bool userInRole)
        {
            return new UserRoleModel()
            {
                UserId = userId,

                Id = role.Id,
                RoleName = role.Name,
                Description = role.Description,

                UserInRole = userInRole
            };
        }
    }

    
}