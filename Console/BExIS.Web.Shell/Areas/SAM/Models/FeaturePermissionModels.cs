using BExIS.Security.Entities.Authorization;
using BExIS.Security.Entities.Subjects;
using System.Collections.Generic;

namespace BExIS.Modules.Sam.UI.Models
{
    public class CreateFeaturePermissionGridRowModel
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public int PermissionType { get; set; }
        public string Type { get; set; }

        public static CreateFeaturePermissionGridRowModel Convert(Subject subject, Dictionary<long, int> permissionTypes)
        {
            return new CreateFeaturePermissionGridRowModel()
            {
                Id = subject.Id,
                Name = subject.Name,
                Type = subject is User ? "User" : "Group",
                PermissionType = permissionTypes.ContainsKey(subject.Id) ? permissionTypes[subject.Id] : 2
            };
        }
    }

    public class FeaturePermissionGridRowModel
    {
        public long FeatureId { get; set; }
        public long Id { get; set; }
        public int PermissionType { get; set; }
        public long SubjectId { get; set; }
        public string SubjectName { get; set; }
        public string SubjectType { get; set; }

        public static FeaturePermissionGridRowModel Convert(FeaturePermission featurePermission)
        {
            return new FeaturePermissionGridRowModel()
            {
                FeatureId = featurePermission.Feature.Id,
                Id = featurePermission.Id,
                SubjectType = featurePermission.Subject is User ? "User" : "Group",
                SubjectName = featurePermission.Subject.Name,
                SubjectId = featurePermission.Subject.Id,
                PermissionType = (int)featurePermission.PermissionType
            };
        }
    }
}