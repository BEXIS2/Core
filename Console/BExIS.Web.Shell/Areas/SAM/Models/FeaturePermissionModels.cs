using BExIS.Security.Entities.Authorization;
using System;
using System.Linq;
using System.Linq.Expressions;

namespace BExIS.Modules.Sam.UI.Models
{
    public static IQueryable<FeaturePermissionGridRowModel> ToFeaturePermissionGridRowModel(this IQueryable<FeaturePermission> source)
    {
        Expression<Func<FeaturePermission, FeaturePermissionGridRowModel>> conversion = u =>
            new UserGridRowModel()
            {
                Email = u.Email,
                Id = u.Id,
                IsAdministrator = u.IsAdministrator,
                UserName = u.Name
            };

        return source.Select(conversion);
    }

    public static class FeaturePermissionExtensions
    {
    }

    public class CreateFeaturePermissionModel
    {
    }

    public class FeaturePermissionGridRowModel
    {
        public long Id { get; set; }
        public long EntityId { get; set; }
        public long Key { get; set; }
        public long SubjectId { get; set; }
        public string SubjectName { get; set; }
        public string SubjectType { get; set; }
        public int PermissionType { get; set; }

        public static FeaturePermissionGridRowModel Convert(FeaturePermission featurePermission)
        {
            return new FeaturePermissionGridRowModel()
            {
                Id = featurePermission.Id,
                SubjectType = featurePermission.Subject.GetType().ToString(),
                SubjectName = featurePermission.Subject.Name,
                SubjectId = featurePermission.Subject.Id,
                PermissionType = (int)featurePermission.PermissionType
            };
        }
    }
}