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
        [Remote("ValidateRoleName", "Roles")]
        [Required]
        public string RoleName { get; set; }

        [Display(Name = "Description")]
        [Required]
        public string Description { get; set; }
    }
}