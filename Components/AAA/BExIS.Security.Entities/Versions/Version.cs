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
        public long Id { get; set; }
        public string Module { get; set; }
        public string Value { get; set; }  
    }
}
