using BExIS.Security.Entities.Subjects;
using BExIS.Utils.Filters;
using System.Collections.Generic;

namespace BExIS.Modules.Sam.UI.Models
{
    public class FeaturePermissionGridRowModel
    {
        public int PermissionType { get; set; }
        [Query(typeof(Subject), "Id")]
        public long Id { get; set; }

        [Query(typeof(Subject), "Name")]
        public string Name { get; set; }
        public string Type { get; set; }

        public static FeaturePermissionGridRowModel Convert(Subject subject, Dictionary<long, int> permissionTypes)
        {
            return new FeaturePermissionGridRowModel()
            {
                Type = subject is User ? "User" : "Group",
                Name = subject.Name,
                Id = subject.Id,
                PermissionType = permissionTypes.ContainsKey(subject.Id) ? permissionTypes[subject.Id] : 2
            };
        }
    }
}