using BExIS.Security.Entities.Subjects;
using System.Collections.Generic;

namespace BExIS.Modules.Sam.UI.Models
{
    public class CreateUserModel
    {
        public string Email { get; set; }
        public bool IsAdministrator { get; set; }
        public string UserName { get; set; }
    }

    public class UpdateUserModel
    {
        public string Email { get; set; }
        public bool IsAdministrator { get; set; }
        public string UserName { get; set; }
    }

    public class DeleteUserModel
    {
        public string Email { get; set; }
        public bool IsAdministrator { get; set; }
        public string UserName { get; set; }
    }

    public class UserMembershipGridRowModel
    {
        public string Email { get; set; }
        public long Id { get; set; }
        public bool IsAdministrator { get; set; }
        public string UserName { get; set; }
        public bool IsMember { get; set; }

        public static UserMembershipGridRowModel Convert(User user, List<long> memberships)
        {
            return new UserMembershipGridRowModel()
            {
                Email = user.Email,
                UserName = user.UserName,
                IsAdministrator = user.IsAdministrator,
                Id = user.Id,
                IsMember = memberships.Contains(user.Id)
            };
        }
    }

    public class UserGridRowModel
    {
        public string Email { get; set; }
        public long Id { get; set; }
        public bool IsAdministrator { get; set; }
        public string UserName { get; set; }

        public static UserGridRowModel Convert(User user)
        {
            return new UserGridRowModel()
            {
                Email = user.Email,
                Id = user.Id,
                IsAdministrator = user.IsAdministrator,
                UserName = user.Name
            };
        }
    }
}