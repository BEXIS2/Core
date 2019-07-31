using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vaiona.Entities.Common;

namespace BExIS.Security.Entities.Objects
{
    public class EntityReference : BaseEntity
    {
        public virtual long SourceId { get; set; }
        public virtual long SourceEntityId { get; set; }
        public virtual long TargetId { get; set; }
        public virtual long TargetEntityId { get; set; }
        public virtual string Context { get; set; }

        public EntityReference()
        {
            SourceId = 0;
            SourceEntityId = 0;
            TargetId = 0;
            TargetEntityId = 0;
            Context = "";
        }

        public EntityReference(long sourceId, long sourceEntityId, long targetId, long targetEntityId, string context)
        {
            SourceId = sourceId;
            SourceEntityId = sourceEntityId;
            TargetId = targetId;
            TargetEntityId = targetEntityId;
            Context = context;
        }
    }
}