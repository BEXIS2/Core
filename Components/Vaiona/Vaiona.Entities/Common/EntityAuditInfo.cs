using System;

namespace Vaiona.Entities.Common
{
    public enum AuditActionType
    {
        Create, Edit, Delete, Read, Execute, Unkown, UserDefined
    }

    public class EntityAuditInfo
    {
        public virtual AuditActionType ActionType { get; set; }
        public virtual string Performer { get; set; }
        public virtual string Comment { get; set; }
        public virtual DateTime? Timestamp { get; set; }
    }
}