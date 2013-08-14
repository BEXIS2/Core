using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using BExIS.Security.Entities;

namespace BExIS.Web.Shell.Areas.Auth.Models
{
    public class RoleModel
    {
        public long Id { get; set; }

        [Required]
        public string RoleName { get; set; }

        [Required]
        [DataType(DataType.MultilineText)]
        public string Description { get; set; }

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