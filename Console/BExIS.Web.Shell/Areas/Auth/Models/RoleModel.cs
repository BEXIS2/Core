using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using BExIS.Security.Entities.Subjects;

namespace BExIS.Web.Shell.Areas.Auth.Models
{
    public class RoleModel
    {
        [Display(Name = "Role ID")]
        [Editable(false)]
        [Required]
        public long Id { get; set; }
        
        [Display(Name = "Role Name")]
        [RegularExpression("^([A-Za-z]+)$", ErrorMessage = "The role name must consist only of letters.")]
        [Remote("ValidateRoleName", "Roles", AdditionalFields = "Id")]
        [Required]
        [StringLength(50, ErrorMessage = "The role name must be {2} - {1} characters long.", MinimumLength = 3)]
        public string RoleName { get; set; }

        [Display(Name = "Description")]
        [Required]
        [StringLength(250, ErrorMessage = "The description must be {2} - {1} characters long.", MinimumLength = 16)]
        public string Description { get; set; }

        public static RoleModel Convert(Role role)
        {
            return new RoleModel()
            {
                Id = role.Id,
                RoleName = role.Name,
                Description = role.Description,
            };
        }
    }
}