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
    public class UserCreateModel
    {
        [Display(Name = "User name")]
        [RegularExpression("^[\\S]*$", ErrorMessage = "The user name is invalid.")]
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
        [RegularExpression("^[^\\s]+(\\s+[^\\s]+)*$", ErrorMessage = "The security answer must start and end with no space.")]
        [Required]
        [StringLength(50, ErrorMessage = "The security answer must be less than {1} characters long.")]
        public string SecurityAnswer { get; set; }
    }

    public class UserUpdateModel
    {
        public long Id { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }

        public static UserUpdateModel Convert(User user)
        {
            return new UserUpdateModel()
            {
                Id = user.Id,
                UserName = user.Name,
                Email = user.Email
            };

        }
    }

    public class UserReadModel
    {
        public long Id { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }

        public static UserReadModel Convert(User user)
        {
            return new UserReadModel()
            {
                Id = user.Id,
                UserName = user.Name,
                Email = user.Email
            };

        }
    }

    public class UserDeleteModel
    {
        public long Id { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }

        public static UserDeleteModel Convert(User user)
        {
            return new UserDeleteModel()
            {
                Id = user.Id,
                UserName = user.Name,
                Email = user.Email
            };

        }
    }

    public class UserGridRowModel
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

        public static UserGridRowModel Convert(User user)
        {
            return new UserGridRowModel()
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

    public class UserMembershipGridRowModel
    {
        public long UserId { get; set; }

        public long Id { get; set; }
        public string GroupName { get; set; }
        public string Description { get; set; }

        public bool UserInGroup { get; set; }

        public static UserMembershipGridRowModel Convert(long userId, Group group, bool userInGroup)
        {
            return new UserMembershipGridRowModel()
            {
                UserId = userId,

                Id = group.Id,
                GroupName = group.Name,
                Description = group.Description,

                UserInGroup = userInGroup
            };
        }
    }

    public class UserSelectListItemModel
    {

    }

    public class UserSelectListModel
    {

    }
}