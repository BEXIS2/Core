using BEXIS2.Security.Entities.Subjects;
using BEXIS2.DB.Entities.Common;

namespace BEXIS2.Security.Entities.Authentication
{
    public class Login : BaseEntity
    {
        public virtual string LoginProvider { get; set; }
        public virtual string ProviderKey { get; set; }
        public virtual User User { get; set; }
    }
}