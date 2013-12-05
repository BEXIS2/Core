using System;
using System.Collections.Generic;

namespace BExIS.Security.Entities.Subjects
{
    public class User : Subject
    {
        #region Attributes

        public virtual string Email { get; set; }

        public virtual DateTime RegistrationDate { get; set; }
        public virtual DateTime LastActivityDate { get; set; }
        public virtual DateTime LastLockOutDate { get; set; }
        public virtual DateTime LastLoginDate { get; set; }
        public virtual DateTime LastPasswordChangeDate { get; set; }

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