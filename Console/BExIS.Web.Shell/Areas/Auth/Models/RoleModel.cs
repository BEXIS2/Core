using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using BExIS.Security.Entities;

namespace BExIS.Web.Shell.Areas.Auth.Models
{
    public class RoleModel
    {
        [Display(Name = "Role ID")]
        [Required]
        public long Id { get; set; }
        
        [Display(Name = "Role Name")]
        [Remote("ValidateRoleName", "Roles", AdditionalFields = "Id")]
        [Required]
        public string RoleName { get; set; }

        [Display(Name = "Description")]
        [Required]
        public string Description { get; set; }

        [Display(Name = "Comment")]
        public string Comment { get; set; }

        public static RoleModel Convert(Role role)
        {
            return new RoleModel()
            {
                Id = role.Id,
                RoleName = role.Name,
                Description = role.Description,
                Comment = role.Comment
            };
        }
    }
}