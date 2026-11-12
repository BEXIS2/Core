using BEXIS2.Security.Entities.Authorization;
using BEXIS2.Security.Entities.Objects;
using BEXIS2.Security.Entities.Subjects;
using BEXIS2.DB.Entities.Common;

namespace BEXIS2.Security.Entities.FormerMember
{
    public class FeaturePermissionFormerMember : BaseEntity
    {
        public virtual Feature Feature { get; set; }
        public virtual PermissionType PermissionType { get; set; }
        public virtual Subject Subject { get; set; }
    }
}