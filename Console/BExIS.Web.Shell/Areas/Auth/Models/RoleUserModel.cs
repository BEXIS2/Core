using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using BExIS.Security.Entities;

namespace BExIS.Web.Shell.Areas.Auth.Models
{
    public class RoleUserModel
    {
        public long Id { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }

        public bool UserInRole { get; set; }

        public static RoleUserModel Convert(User user, bool userInRole)
        {
            return new RoleUserModel()
            {
                Id = user.Id,
                UserName = user.Name,
                Email = user.Email,

                UserInRole = userInRole
            };
        }
    }
}