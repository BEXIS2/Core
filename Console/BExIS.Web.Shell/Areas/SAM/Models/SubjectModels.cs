using BExIS.Security.Entities.Subjects;

namespace BExIS.Modules.Sam.UI.Models
{
    public class GroupCreationModel
    {
        public string GroupName { get; set; }
    }

    public class GroupGridRowModel
    { }

    public class GroupUpdateModel
    {
    }

    public class SubjectGridRowModel
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }

        public static SubjectGridRowModel Convert(Subject subject)
        {
            return new SubjectGridRowModel()
            {
                Id = subject.Id,

                Name = subject.Name,
                Type = subject is User ? "User" : "Group"
            };
        }
    }

    public class UserCreationModel
    {
        public string Email { get; set; }
        public string UserName { get; set; }
    }

    public class UserGridRowModel
    {
    }

    public class UserUpdateModel
    {
    }
}