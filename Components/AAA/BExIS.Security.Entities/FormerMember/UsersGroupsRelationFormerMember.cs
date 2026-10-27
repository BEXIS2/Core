using BEXIS2.DB.Entities.Common;

namespace BExIS.Security.Entities.FormerMember
{
    public class UsersGroupsRelationFormerMember : BaseEntity
    {
        public virtual long UserRef { get; set; }
        public virtual long GroupRef { get; set; }
    }
}