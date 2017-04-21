using BExIS.Security.Entities.Subjects;
using Vaiona.Entities.Common;

namespace BExIS.Security.Entities.Authentication
{
    public class Login : BaseEntity
    {
        public virtual string LoginProvider { get; set; }
        public virtual string ProviderKey { get; set; }
        public virtual User User { get; set; }
    }
}
