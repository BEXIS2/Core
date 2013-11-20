using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vaiona.Entities.Common;

namespace BExIS.Security.Entities
{
    public class UserSecurityInfo : BaseEntity
    {
        public virtual string Name { get; set; }

        // PASSWORD
        public virtual string Password { get; set; }
        public virtual string Salt { get; set; }
        public virtual string PasswordQuestion { get; set; }
        public virtual string PasswordAnswer { get; set; }
        public virtual Int32 PasswordFailureCount { get; set; }
        public virtual Int32 PasswordAnswerFailureCount { get; set; }
        public virtual DateTime LastPasswordFailureDate { get; set; }
        public virtual DateTime LastPasswordAnswerFailureDate { get; set; }

        // BOOLEAN
        public virtual Boolean IsApproved { get; set; }
        public virtual Boolean IsLockedOut { get; set; }
    }
}
