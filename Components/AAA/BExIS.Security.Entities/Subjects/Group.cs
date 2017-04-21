using System.Collections.Generic;

namespace BExIS.Security.Entities.Subjects
{
    public class Group : Subject
    {
        public virtual string Description { get; set; }
        public virtual ICollection<User> Users { get; set; }

        public Group()
        {
            Users = new List<User>();
        }
    }
}
