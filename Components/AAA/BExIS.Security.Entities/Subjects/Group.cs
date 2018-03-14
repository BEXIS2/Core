using Microsoft.AspNet.Identity;
using System.Collections.Generic;

namespace BExIS.Security.Entities.Subjects
{
    public class Group : Subject, IRole<long>
    {
        public Group()
        {
            Users = new List<User>();
        }

        public virtual string Description { get; set; }
        public virtual bool IsValid { get; set; }
        public virtual bool IsSystemGroup { get; set; }
        public virtual ICollection<User> Users { get; set; }
    }
}