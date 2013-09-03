using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DataAnnotationsExtensions;

namespace BExIS.Web.Shell.Areas.Auth.Models
{
    public class UserCreationModel
    {
        [Display(Name = "User name")]
        [Remote("ValidateUserName", "Users")]
        [Required]
        public string UserName { get; set; }

        [Display(Name = "Email address")]
        [Email]
        [Remote("ValidateEmail", "Users")]
        [Required]
        public string Email { get; set; }

        [Display(Name = "Password")]
        [Required]
        public string Password { get; set; }

        [Compare("Password", ErrorMessage = "The password and confirmation do not match.")]
        [Display(Name = "Confirm Password")]
        [Required]
        public string ConfirmPassword { get; set; }

        [Display(Name = "Security Question")]
        [Required]
        public string SecurityQuestion { get; set; }

        [Display(Name = "Security Answer")]
        [Required]
        public string SecurityAnswer { get; set; }

        [Display(Name = "Confirm Security Answer")]
        [Compare("SecurityAnswer", ErrorMessage = "The security answer and confirmation do not match.")]
        [Required]
        public string ConfirmSecuritydAnswer { get; set; }

        public string Comment { get; set; }
    }
}