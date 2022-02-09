using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Vaiona.Entities.Common
{
    public interface IAuditableEntity
    {
        EntityAuditInfo CreationInfo { get; set; } 
        EntityAuditInfo ModificationInfo { get; set; } 
    }
}
