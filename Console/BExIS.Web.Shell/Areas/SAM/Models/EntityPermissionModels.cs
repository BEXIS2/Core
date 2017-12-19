using BExIS.Security.Entities.Subjects;

namespace BExIS.Modules.Sam.UI.Models
{
    public class EntityPermissionGridRowModel
    {
        public int EffectiveRights { get; set; }
        public int Rights { get; set; }
        public long SubjectId { get; set; }
        public string SubjectName { get; set; }
        public string SubjectType { get; set; }

        public static EntityPermissionGridRowModel Convert(Subject subject, int rights, int effectiveRights)
        {
            return new EntityPermissionGridRowModel()
            {
                EffectiveRights = effectiveRights,
                SubjectId = subject.Id,
                SubjectName = subject.Name,
                SubjectType = subject is User ? "User" : "Group",

                Rights = rights
            };
        }
    }
}