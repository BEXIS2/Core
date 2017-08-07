using BExIS.Security.Entities.Authorization;

namespace BExIS.Modules.Sam.UI.Models
{
    public static class EntityPermissionExtensions
    {
    }

    public class CreateEntityPermissionModel
    {
    }

    public class EntityPermissionGridRowModel
    {
        public long EntityId { get; set; }
        public long Id { get; set; }
        public long Key { get; set; }
        public int Rights { get; set; }
        public long SubjectId { get; set; }
        public string SubjectName { get; set; }
        public string SubjectType { get; set; }

        public static EntityPermissionGridRowModel Convert(EntityPermission entityPermission)
        {
            return new EntityPermissionGridRowModel()
            {
                Id = entityPermission.Id,
                EntityId = entityPermission.Entity.Id,
                Key = entityPermission.Key,
                Rights = entityPermission.Rights,
                SubjectType = entityPermission.Subject.GetType().FullName,
                SubjectName = entityPermission.Subject.Name,
                SubjectId = entityPermission.Subject.Id,
            };
        }
    }
}