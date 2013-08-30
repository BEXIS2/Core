using System;
using BExIS.Security.Entities;

namespace BExIS.Web.Shell.Areas.Auth.Models
{
    public class UserModel
    {
        public long Id { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public DateTime RegistrationDate { get; set; }
        public DateTime LastLoginDate { get; set; }
        public DateTime LastActivityDate { get; set; }
        public bool IsApproved { get; set; }
        public bool IsLockedOut { get; set; }
        public string Comment { get; set; }

        public static UserModel Convert(User user)
        {
            return new UserModel()
            {
                Id = user.Id,
                UserName = user.Name,
                Email = user.Email,
                RegistrationDate = user.RegistrationDate,
                LastLoginDate = user.LastLoginDate,
                LastActivityDate = user.LastActivityDate,
                IsApproved = user.IsApproved,
                IsLockedOut = user.IsLockedOut,
                Comment = user.Comment,
            };
        }
    }
}