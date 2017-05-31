using BExIS.Security.Entities.Subjects;
using Microsoft.AspNet.Identity;
using System.Collections.Generic;
using Vaiona.Entities.Common;

namespace BExIS.Security.Entities.Authorization
{
    public class Role : BaseEntity, IRole<long>
    {
        public Role()
        {
            Rules = new List<Rule>();
            Subjects = new List<Subject>();
        }

        public virtual string Description { get; set; }
        public virtual string Name { get; set; }
        public virtual ICollection<Rule> Rules { get; set; }
        public virtual ICollection<Subject> Subjects { get; set; }
    }
}