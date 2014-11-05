using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using BExIS.Security.Entities.Authorization;
using BExIS.Security.Entities.Subjects;

namespace BExIS.Web.Shell.Areas.Auth.Models
{
    public class RoleCreationModel
    {
        [Display(Name = "Role Name")]
        [RegularExpression("^[\\S]*$", ErrorMessage = "The role name is invalid.")]
        [Remote("ValidateRoleName", "Roles")]
        [Required]
        [StringLength(50, ErrorMessage = "The role name must be {2} - {1} characters long.", MinimumLength = 3)]
        public string RoleName { get; set; }

        [Display(Name = "Description")]
        [RegularExpression("^[^\\s]+(\\s+[^\\s]+)*", ErrorMessage = "The description must start and end with no space.")]
        [StringLength(250, ErrorMessage = "The description must be less than {1} characters long.")]
        public string Description { get; set; }
    }

    public class RoleModel
    {
        [Display(Name = "Role ID")]
        [Editable(false)]
        [Required]
        public long Id { get; set; }

        [Display(Name = "Role Name")]
        [RegularExpression("^[\\S]*$", ErrorMessage = "The role name is invalid.")]
        [Remote("ValidateRoleName", "Roles", AdditionalFields = "Id")]
        [Required]
        [StringLength(50, ErrorMessage = "The role name must be {2} - {1} characters long.", MinimumLength = 3)]
        public string RoleName { get; set; }

        [Display(Name = "Description")]
        [StringLength(250, ErrorMessage = "The description must be less than {1} characters long.")]
        public string Description { get; set; }

        public static RoleModel Convert(Role role)
        {
            return new RoleModel()
            {
                Id = role.Id,
                RoleName = role.Name,
                Description = role.Description
            };
        }
    }

    public class RoleUserModel
    {
        public long RoleId { get; set; }

        public long Id { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }

        public bool UserInRole { get; set; }

        public static RoleUserModel Convert(long roleId, User user, bool userInRole)
        {
            return new RoleUserModel()
            {
                RoleId = roleId,

                Id = user.Id,
                UserName = user.Name,
                Email = user.Email,

                UserInRole = userInRole
            };
        }
    }

    
}