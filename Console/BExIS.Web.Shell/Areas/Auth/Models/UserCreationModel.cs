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
        [RegularExpression("^([A-Za-z]+)$", ErrorMessage = "The user name must consist only of letters.")]
        [Remote("ValidateUserName", "Users")]
        [Required]
        [StringLength(50, ErrorMessage = "The user name must be {2} - {1} characters long.", MinimumLength = 3)]
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