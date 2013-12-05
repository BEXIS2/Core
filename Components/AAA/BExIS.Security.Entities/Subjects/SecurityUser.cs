using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vaiona.Entities.Common;

namespace BExIS.Security.Entities.Subjects
{
    public class SecurityUser : BaseEntity
    {
        public virtual string Name { get; set; }

        public virtual string Password { get; set; }
        public virtual string PasswordSalt { get; set; }
        public virtual string SecurityQuestion { get; set; }
        public virtual string SecurityAnswer { get; set; }
        public virtual string SecurityAnswerSalt { get; set; }
        public virtual Int32 PasswordFailureCount { get; set; }
        public virtual Int32 SecurityAnswerFailureCount { get; set; }
        public virtual DateTime LastPasswordFailureDate { get; set; }
        public virtual DateTime LastSecurityAnswerFailureDate { get; set; }
    }
}
