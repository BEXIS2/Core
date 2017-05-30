using BExIS.Security.Entities.Authorization;
using System;
using System.Linq;
using System.Linq.Expressions;

namespace BExIS.Modules.Sam.UI.Models
{
    public static class EntityPermissionExtensions
    {
        public static IQueryable<GroupEntityPermissionGridRowModel> ToGroupEntityPermissionGridRowModel(this IQueryable<EntityPermission> source)
        {
            Expression<Func<EntityPermission, GroupEntityPermissionGridRowModel>> conversion = p =>
                new GroupEntityPermissionGridRowModel()
                {
                    EntityName = p.Entity.Name,
                    EntityId = p.Entity.Id,
                    Rights = p.Rights,
                    GroupName = p.Subject.Name,
                    GroupId = p.Subject.Id
                };

            return source.Select(conversion);
        }

        public static IQueryable<UserEntityPermissionGridRowModel> ToUserEntityPermissionGridRowModel(this IQueryable<EntityPermission> source)
        {
            Expression<Func<EntityPermission, UserEntityPermissionGridRowModel>> conversion = p =>
                new UserEntityPermissionGridRowModel()
                {
                    EntityName = p.Entity.Name,
                    EntityId = p.Entity.Id,
                    Rights = p.Rights,
                    UserName = p.Subject.Name,
                    UserId = p.Subject.Id
                };

            return source.Select(conversion);
        }
    }

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