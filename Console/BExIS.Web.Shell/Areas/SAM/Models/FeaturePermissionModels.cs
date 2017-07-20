using BExIS.Security.Entities.Authorization;

namespace BExIS.Modules.Sam.UI.Models
{
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