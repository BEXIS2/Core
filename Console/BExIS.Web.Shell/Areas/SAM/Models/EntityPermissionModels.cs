using BExIS.Security.Entities.Authorization;

namespace BExIS.Modules.Sam.UI.Models
{
    public class CreateEntityPermissionModel
    {
    }

    public class EntityPermissionGridRowModel
    {
        public static EntityPermissionGridRowModel Convert(EntityPermission entityPermission)
        {
            return new EntityPermissionGridRowModel()
            {
            };
        }
    }

    public class GroupEntityPermissionGridRowModel
    {
        public long EntityId { get; set; }
        public string EntityName { get; set; }
        public long GroupId { get; set; }
        public string GroupName { get; set; }
        public short Rights { get; set; }
    }

    public class UserEntityPermissionGridRowModel
    {
        public long EntityId { get; set; }
        public string EntityName { get; set; }
        public short Rights { get; set; }
        public long UserId { get; set; }
        public string UserName { get; set; }
    }
}