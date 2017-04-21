using Vaiona.Entities.Common;

namespace BExIS.Security.Entities.Authorization
{
    public class Rule : BaseEntity
    {
        public virtual Role Role { get; set; }
    }
}
