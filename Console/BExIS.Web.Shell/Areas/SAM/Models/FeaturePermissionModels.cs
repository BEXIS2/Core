using BExIS.Security.Entities.Subjects;

namespace BExIS.Modules.Sam.UI.Models
{
    public class FeaturePermissionGridRowModel
    {
        public bool EffectiveRight { get; set; }
        public long FeatureId { get; set; }

        public int FeaturePermissionType { get; set; }
        public long Id { get; set; }


        public string DisplayName { get; set; }

        public string Type { get; set; }

        public static FeaturePermissionGridRowModel Convert(Subject subject, long featureId, int featurePermissionType, bool effectiveRight)
        {
            return new FeaturePermissionGridRowModel()
            {
                FeatureId = featureId,

                Id = subject.Id,
                DisplayName = subject.DisplayName,
                Type = subject is User ? "User" : "Group",

                EffectiveRight = effectiveRight,
                FeaturePermissionType = featurePermissionType
            };
        }
    }

    public class FeaturePermissionReadModel
    {
    }
}