using BExIS.Dim.Entities.Submissions;
using System.Collections.Generic;
using Vaiona.Entities.Common;

namespace BExIS.Dim.Entities.Publication
{
    public class Repository : BaseEntity, IBusinessVersionedEntity
    {
        public virtual Broker Broker { get; set; }
        public virtual string Name { get; set; }
        public virtual string Description { get; set; }
        public virtual string Endpoint { get; set; }
        public virtual string Url { get; set; }
        public virtual List<Agent> Agents { get; set; }

        public Repository()
        {
            Agents = new List<Agent>();
        }
    }
}
