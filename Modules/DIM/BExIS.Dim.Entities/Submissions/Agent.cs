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
        public string Name { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public Repository Repository { get; set; }
        public List<Submission> Submissions { get; set; }
    }
}
