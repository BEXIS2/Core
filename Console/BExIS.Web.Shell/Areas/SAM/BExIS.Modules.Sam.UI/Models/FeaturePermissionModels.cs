using BExIS.Security.Entities.Subjects;
using Newtonsoft.Json;

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

    public class CreateFeaturePermissionModel
    {
        [JsonProperty("featureId")]
        public long FeatureId { get; set; }

        [JsonProperty("subjectId")]
        public long SubjectId { get; set; }

        [JsonProperty("featurePermissionType")]
        public int FeaturePermissionType { get; set; }
    }

    public class ReadFeaturePermissionModel
    {
        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("featureId")]
        public long FeatureId { get; set; }

        [JsonProperty("subjectId")]
        public long SubjectId { get; set; }

        [JsonProperty("featurePermissionType")]
        public int FeaturePermissionType { get; set; }
    }

    public class UpdateFeaturePermissionModel
    {
        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("featurePermissionType")]
        public int FeaturePermissionType { get; set; }
    }
}