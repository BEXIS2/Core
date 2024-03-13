using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vaiona.Entities.Common;

namespace BExIS.Dim.Entities.Submissions
{
    public class Repository : BaseEntity
    {
        public virtual string Name { get; set; }
        public virtual string Description { get; set; }
        public virtual string Endpoint { get; set; }

        public virtual List<Agent> Agents { get; set; }

        public Repository() 
        {
            Agents = new List<Agent>();
        }
    }
}
