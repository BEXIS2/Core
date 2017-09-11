using BExIS.Security.Entities.Subjects;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace BExIS.Modules.Sam.UI.Models
{
    public class CreateUserModel
    {
        public string Email { get; set; }
        public string UserName { get; set; }
    }

    public class DeleteUserModel
    {
        public string Email { get; set; }
        public string UserName { get; set; }
    }

    public class UpdateUserModel
    {
        [Required]
        public string Email { get; set; }

        public long Id { get; set; }
        public string UserName { get; set; }

        public static UpdateUserModel Convert(User user)
        {
            return new UpdateUserModel()
            {
                Email = user.Email,
                Id = user.Id,
                UserName = user.Name
            };
        }
    }

    public class UserGridRowModel
    {
        public string Email { get; set; }
        public long Id { get; set; }
        public string UserName { get; set; }

        public static UserGridRowModel Convert(User user)
        {
            return new UserGridRowModel()
            {
                Email = user.Email,
                Id = user.Id,
                UserName = user.Name
            };
        }
    }

    public class UserMembershipGridRowModel
    {
        public string Email { get; set; }
        public long Id { get; set; }
        public bool IsUserInGroup { get; set; }
        public string Name { get; set; }

        public static UserMembershipGridRowModel Convert(User user, string groupName)
        {
            return new UserMembershipGridRowModel()
            {
                Email = user.Email,
                Id = user.Id,
                IsUserInGroup = user.Groups.Any(g => g.Name.ToUpperInvariant() == groupName.ToUpperInvariant()),
                Name = user.Name
            };
        }
    }
}