using System;
using System.Collections.Generic;
using BExIS.Security.Entities.Authentication;
using BExIS.Security.Entities.Objects;
      
namespace BExIS.Security.Entities.Subjects
{      
    public class User : Subject
    {
        #region Attributes
      
        public virtual string Email { get; set; }

        public virtual string FullName { get; set; }

        public virtual string Password { get; set; }
        public virtual string PasswordSalt { get; set; }

        public virtual string SecurityAnswer { get; set; }
        public virtual string SecurityAnswerSalt { get; set; }

        public virtual DateTime RegistrationDate { get; set; }
        public virtual DateTime LastActivityDate { get; set; }
        public virtual DateTime LastLockOutDate { get; set; }
        public virtual DateTime LastLoginDate { get; set; }
        public virtual DateTime LastPasswordChangeDate { get; set; }

        public virtual Boolean IsActive { get; set; }
        public virtual Boolean IsApproved { get; set; }
        public virtual Boolean IsLockedOut { get; set; }

        public virtual Int32 PasswordFailureCount { get; set; }
        public virtual Int32 SecurityAnswerFailureCount { get; set; }

        public virtual DateTime LastPasswordFailureDate { get; set; }
        public virtual DateTime LastSecurityAnswerFailureDate { get; set; }

        public virtual Authenticator ActiveAuthenticator { get; set; }

        #endregion


        #region Associations

        public virtual Authenticator Authenticator { get; set; }

        public virtual ICollection<Group> Groups { get; set; }

        public virtual SecurityQuestion SecurityQuestion { get; set; }

        #endregion


        #region Methods
      
        public User()
        {
            Groups = new List<Group>();
        }

        #endregion
    }
}