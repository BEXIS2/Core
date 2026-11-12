using BEXIS2.Security.Entities.Objects;
using BEXIS2.Security.Entities.Subjects;
using BEXIS2.DB.Entities.Common;

namespace BEXIS2.Security.Entities.FormerMember
{
    /// <summary>
    ///
    /// </summary>
    public class EntityPermissionFormerMember : BaseEntity
    {
        public virtual Entity Entity { get; set; }
        public virtual long Key { get; set; }
        public virtual int Rights { get; set; }
        public virtual Subject Subject { get; set; }
    }
}