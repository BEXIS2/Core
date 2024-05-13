using BExIS.Dim.Entities.Publication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vaiona.Entities.Common;

namespace BExIS.Dim.Entities.Submissions
{
    public class Agent : BaseEntity
    {
        public virtual string Name { get; set; }
        public virtual string Username { get; set; }
        public virtual string Password { get; set; }
        public virtual Repository Repository { get; set; }
        public virtual List<Submission> Submissions { get; set; }

        public Agent()
        {
            Submissions = new List<Submission>();
        }
    }
}
