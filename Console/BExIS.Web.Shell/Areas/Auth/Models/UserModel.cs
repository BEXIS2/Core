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
        [StringLength(250, ErrorMessage = "The email must be {2} - {1} characters long.", MinimumLength = 5)]
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
                LastActivityDate = user.LastActivityDate,
            };
        }
    }
}