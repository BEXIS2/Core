using Vaiona.Entities.Common;

namespace BExIS.Security.Entities.Notifications
{
    public class Notification : BaseEntity
    {
        public virtual string Body { get; set; }
        public virtual string Destination { get; set; }
        public virtual string Subject { get; set; }
    }
}