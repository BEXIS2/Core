using BExIS.Security.Entities.Subjects;
using System.Linq;

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
        public bool IsAdministrator { get; set; }
        public long Id { get; set; }
        public bool IsUserInGroup { get; set; }
        public string Name { get; set; }

        public static UserMembershipGridRowModel Convert(User user, long featureId)
        {
            return new UserMembershipGridRowModel()
            {
                Email = user.Email,
                IsAdministrator = user.IsAdministrator,
                Id = user.Id,
                IsUserInGroup = user.Groups.Any(g => g.Id == featureId),
                Name = user.Name
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