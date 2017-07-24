using BExIS.Security.Entities.Objects;
using BExIS.Security.Entities.Subjects;

namespace BExIS.Modules.Sam.UI.Models
{
    public class FeaturePermissionGridRowModel
    {
        public bool EffectiveRight { get; set; }
        public long FeatureId { get; set; }

        public int FeaturePermissionType { get; set; }
        public long SubjectId { get; set; }

        public string SubjectName { get; set; }

        public string SubjectType { get; set; }

        public static FeaturePermissionGridRowModel Convert(Subject subject, Feature feature, int featurePermissionType, bool effectiveRight)
        {
            return new FeaturePermissionGridRowModel()
            {
                FeatureId = feature.Id,

                SubjectId = subject.Id,
                SubjectName = subject.Name,
                SubjectType = subject is User ? "User" : "Group",

                EffectiveRight = effectiveRight,
                FeaturePermissionType = featurePermissionType
            };
        }
    }

    public class FeaturePermissionReadModel
    {
    }
}