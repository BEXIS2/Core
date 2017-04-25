using BExIS.Security.Entities.Objects;

namespace BExIS.Security.Entities.Requests
{
    public class EntityRequest : Request
    {
        public virtual Entity Entity { get; set; }
        public virtual long Key { get; set; }
    }
}