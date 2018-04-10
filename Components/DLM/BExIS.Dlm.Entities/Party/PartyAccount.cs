using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vaiona.Entities.Common;

namespace BExIS.Dlm.Entities.Party
{
    public class PartyUser : BaseEntity
    {
        public virtual long PartyId { get; set; }

        public virtual long UserId { get; set; }

        public virtual Party Party { get; set; }
    }
}
