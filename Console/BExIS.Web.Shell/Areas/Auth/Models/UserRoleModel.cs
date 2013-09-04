using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using BExIS.Security.Entities;

namespace BExIS.Web.Shell.Areas.Auth.Models
{
    public class UserRoleModel
    {
        // User
        public long UserId { get; set; }

        // Role
        public long Id { get; set; }
        public string RoleName { get; set; }
        public string Description { get; set; }

        // Membership
        public bool UserInRole { get; set; }

        public static UserRoleModel Convert(long userId, Role role, bool userInRole)
        {
            return new UserRoleModel()
            {
                UserId = userId,

                Id = role.Id,
                RoleName = role.Name,
                Description = role.Description,

                UserInRole = userInRole
            };
        }
    }
}