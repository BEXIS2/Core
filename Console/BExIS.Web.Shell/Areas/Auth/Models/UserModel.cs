using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using BExIS.Security.Entities;
using DataAnnotationsExtensions;

namespace BExIS.Web.Shell.Areas.Auth.Models
{
    public class UserModel
    {
        [Display(Name = "User ID")]
        [Required]
        public long Id { get; set; }

        [Display(Name = "User Name")]
        [Remote("ValidateUserName", "Users", AdditionalFields = "Id")]
        [Required]
        public string UserName { get; set; }

        [Display(Name = "Email")]
        [Email]
        [Remote("ValidateEmail", "Users", AdditionalFields = "Id")]
        [Required]
        public string Email { get; set; }

        [Display(Name = "Registration Date")]
        public DateTime RegistrationDate { get; set; }

        [Display(Name = "Last Login Date")]
        public DateTime LastLoginDate { get; set; }

        [Display(Name = "Last Activity Date")]
        public DateTime LastActivityDate { get; set; }

        [Display(Name = "Is Approved")]
        public bool IsApproved { get; set; }

        [Display(Name = "Is Locked Out")]
        public bool IsLockedOut { get; set; }

        [Display(Name = "Comment")]
        public string Comment { get; set; }

        public static UserModel Convert(User user)
        {
            return new UserModel()
            {
                Id = user.Id,
                UserName = user.Name,
                Email = user.Email,
                RegistrationDate = user.RegistrationDate,
                LastLoginDate = user.LastLoginDate,
                LastActivityDate = user.LastActivityDate,
                IsApproved = user.IsApproved,
                IsLockedOut = user.IsLockedOut,
                Comment = user.Comment
            };
        }
    }
}