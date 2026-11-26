using BEXIS2.DB.Entities.Common;

namespace BEXIS2.Security.Entities.FormerMember
{
    public class UsersGroupsRelationFormerMember : BaseEntity
    {
        public virtual long UserRef { get; set; }
        public virtual long GroupRef { get; set; }
    }
}