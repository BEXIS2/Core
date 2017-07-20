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
        public string Description { get; set; }
        public GroupType GroupType { get; set; }
        public long Id { get; set; }
        public bool IsMember { get; set; }
        public string Name { get; set; }

        public static UserMembershipGridRowModel Convert(Group group, List<long> memberships)
        {
            return new UserMembershipGridRowModel()
            {
                Description = group.Description,
                Name = group.Name,
                GroupType = group.GroupType,
                Id = group.Id,
                IsMember = memberships.Contains(group.Id)
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