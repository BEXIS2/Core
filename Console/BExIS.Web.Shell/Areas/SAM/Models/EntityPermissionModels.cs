using BExIS.Security.Entities.Subjects;

namespace BExIS.Modules.Sam.UI.Models
{
    public class EntityInstanceModel
    {
        public long EntityId { get; set; }
        public long InstanceId { get; set; }
    }

    public class EntityPermissionGridRowModel
    {
        public int EffectiveRights { get; set; }
        public int Rights { get; set; }
        public long Id { get; set; }
        public string DisplayName { get; set; }
        public string Type { get; set; }

        public static EntityPermissionGridRowModel Convert(Subject subject, int rights, int effectiveRights)
        {
            return new EntityPermissionGridRowModel()
            {
                Rights = rights,
                EffectiveRights = effectiveRights,
                Id = subject.Id,
                DisplayName = subject.DisplayName,
                Type = subject is User ? "User" : "Group",
            };
        }
    }

    public class ReferredEntityPermissionGridRowModel
    {
        public string Name { get; set; }
        public int Rights { get; set; }
        public long SubjectId { get; set; }
        public string Type { get; set; }

        public static ReferredEntityPermissionGridRowModel Convert(string name, string type, int rights)
        {
            return new ReferredEntityPermissionGridRowModel()
            {
                Name = name,
                Type = type,
                Rights = rights
            };
        }
    }

    public class SubjectInstanceModel
    {
        public long EntityId { get; set; }
        public long InstanceId { get; set; }
        public long SubjectId { get; set; }
    }
}