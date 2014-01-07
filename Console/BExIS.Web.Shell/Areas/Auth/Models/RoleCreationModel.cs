using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace BExIS.Web.Shell.Areas.Auth.Models
{
    public class RoleCreationModel
    {
        [Display(Name = "Role Name")]
        [RegularExpression("^([A-Za-z]+)$", ErrorMessage = "The role name must consist only of letters.")]
        [Remote("ValidateRoleName", "Roles")]
        [Required]
        [StringLength(50, ErrorMessage = "The role name must be {2} - {1} characters long.", MinimumLength = 3)]
        public string RoleName { get; set; }

        [Display(Name = "Description")]
        [Required]
        [StringLength(250, ErrorMessage = "The description must be {2} - {1} characters long.", MinimumLength = 16)]
        public string Description { get; set; }
    }
}