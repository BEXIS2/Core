using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BExIS.Security.Entities
{
    public class User : Subject
    {
        #region Attributes

        // EMAIL
        public virtual string Email { get; set; }

        // PASSWORD
        public virtual string Password { get; set; }
        public virtual string Salt { get; set; }
        public virtual string PasswordQuestion { get; set; }
        public virtual string PasswordAnswer { get; set; }
        public virtual Int32 PasswordFailureCount { get; set; }
        public virtual Int32 PasswordAnswerFailureCount { get; set; }
        public virtual DateTime LastPasswordFailureDate { get; set; }
        public virtual DateTime LastPasswordAnswerFailureDate { get; set; }

        // DATES
        public virtual DateTime RegistrationDate { get; set; }
        public virtual DateTime LastActivityDate { get; set; }
        public virtual DateTime LastLockOutDate { get; set; }
        public virtual DateTime LastLoginDate { get; set; }
        public virtual DateTime LastPasswordChangeDate { get; set; }

        // BOOLEANS
        public virtual Boolean IsApproved { get; set; }
        public virtual Boolean IsLockedOut { get; set; }

        #endregion


        #region Associations

        public virtual ICollection<Role> Roles { get; set; }

        #endregion


        #region Methods

        public User()
        {
            Roles = new List<Role>();
        }

        #endregion
    }
}
