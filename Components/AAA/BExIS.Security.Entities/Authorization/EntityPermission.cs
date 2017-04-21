using BExIS.Security.Entities.Objects;

namespace BExIS.Security.Entities.Authorization
{
    public class EntityPermission : Permission
    {
        public virtual Entity Entity { get; set; }
        public virtual long Key { get; set; }
    }
}
