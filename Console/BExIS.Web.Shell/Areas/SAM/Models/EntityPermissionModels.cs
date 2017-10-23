using BExIS.Security.Entities.Subjects;

namespace BExIS.Modules.Sam.UI.Models
{
    public class EntityPermissionGridRowModel
    {
        public long Id { get; set; }
        public int Rights { get; set; }
        public long SubjectId { get; set; }
        public string SubjectName { get; set; }
        public string SubjectType { get; set; }

        public static EntityPermissionGridRowModel Convert(Subject subject, int rights)
        {
            return new EntityPermissionGridRowModel()
            {
                SubjectId = subject.Id,
                SubjectName = subject.Name,
                SubjectType = subject is User ? "User" : "Group",
                Rights = rights
            };
        }
    }
}