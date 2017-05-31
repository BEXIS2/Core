using BExIS.Security.Entities.Objects;

namespace BExIS.Security.Entities.Authorization
{
    public enum RightType
    {
        Read = 0,
        Write = 1,
        Delete = 2,
        Grant = 3
    }

    public class EntityPermission : Permission
    {
        public virtual Entity Entity { get; set; }
        public virtual long Key { get; set; }
        public virtual short Rights { get; set; }
    }
}