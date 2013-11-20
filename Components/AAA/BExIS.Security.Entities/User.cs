using System;
using System.Collections.Generic;

namespace BExIS.Security.Entities
{
    public class User : Subject
    {
        #region Attributes

        // PERSONAL INFORMATION
        public virtual string Email { get; set; }

        // DATES
        public virtual DateTime RegistrationDate { get; set; }
        public virtual DateTime LastActivityDate { get; set; }
        public virtual DateTime LastLockOutDate { get; set; }
        public virtual DateTime LastLoginDate { get; set; }
        public virtual DateTime LastPasswordChangeDate { get; set; }

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