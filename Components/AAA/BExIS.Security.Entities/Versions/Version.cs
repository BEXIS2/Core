using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vaiona.Entities.Common;

namespace BExIS.Security.Entities.Versions
{
    public class Version : BaseEntity
    {
        public virtual long Id { get; set; }
        public virtual string Module { get; set; }
        public virtual string Value { get; set; }
        public virtual DateTime Date { get; set; }
    }
}
