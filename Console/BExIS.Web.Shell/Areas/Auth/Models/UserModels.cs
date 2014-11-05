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
        [Display(Name = "User Name")]
        [RegularExpression("^[\\S]*$", ErrorMessage = "The user name is invalid.")]
        [Remote("ValidateUserName", "Users")]
        [Required]
        [StringLength(64, ErrorMessage = "The user name must be {2} - {1} characters long.", MinimumLength = 3)]
        public string UserName { get; set; }

        [Display(Name = "Password")]
        [RegularExpression("^[\\S]*$", ErrorMessage = "The password is invalid.")]
        [Required]
        [StringLength(24, ErrorMessage = "The password must be {2} - {1} characters long.", MinimumLength = 6)]
        public string Password { get; set; }

        [Compare("Password", ErrorMessage = "The password and confirmation do not match.")]
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

    public class UserUpdateModel
    {
        public long Id { get; set; }
        public string UserName { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }

        public static UserUpdateModel Convert(User user)
        {
            return new UserUpdateModel()
            {
                Id = user.Id,
                UserName = user.Name,
                FullName = user.FullName,
                Email = user.Email
            };

        }
    }

    public class UserReadModel
    {
        public long Id { get; set; }
        public string UserName { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }

        public static UserReadModel Convert(User user)
        {
            return new UserReadModel()
            {
                Id = user.Id,
                UserName = user.Name,
                FullName = user.FullName,
                Email = user.Email
            };

        }
    }

    public class UserDeleteModel
    {
        public long Id { get; set; }
        public string UserName { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }

        public static UserDeleteModel Convert(User user)
        {
            return new UserDeleteModel()
            {
                Id = user.Id,
                UserName = user.Name,
                FullName = user.FullName,
                Email = user.Email
            };

        }
    }

    public class UserGridRowModel
    {
        public long Id { get; set; }
        public string UserName { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }

        public static UserGridRowModel Convert(User user)
        {
            return new UserGridRowModel()
            {
                Id = user.Id,
                UserName = user.Name,
                FullName = user.FullName,
                Email = user.Email
            };

        }
    }

    public class UserMembershipGridRowModel
    {
        public long UserId { get; set; }

        public long Id { get; set; }
        public string GroupName { get; set; }
        public string Description { get; set; }

        public bool IsUserInGroup { get; set; }

        public static UserMembershipGridRowModel Convert(long userId, Group group, bool isUserInGroup)
        {
            return new UserMembershipGridRowModel()
            {
                UserId = userId,

                Id = group.Id,
                GroupName = group.Name,
                Description = group.Description,

                IsUserInGroup = isUserInGroup
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