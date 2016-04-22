using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BExIS.Security.Entities.Subjects;
using Vaiona.Entities.Common;

namespace BExIS.Security.Entities.Objects
{
    public class SecurityQuestion : BaseEntity
    {
        public virtual string Title { get; set; }
        public virtual bool IsValid { get; set; }

        public virtual ICollection<User> Users { get; set; }

        public SecurityQuestion()
        {
            Users = new List<User>();
        }
    }
}
