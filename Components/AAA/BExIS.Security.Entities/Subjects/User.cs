using System;
using System.Collections.Generic;
using BExIS.Security.Entities.Security;

/// <summary>
/// The security system contains three types of entities. This namespace is used for
/// all subject entities of the security system - users and roles.
/// </summary>           
namespace BExIS.Security.Entities.Subjects
{
    /// <summary>
    /// A user is one concrete type of a subject.
    /// </summary>
    /// <remarks>
    /// The security system needs to distinguish between different types of subjects.
    /// On the one hand there are real users who register with the system. And on the
    /// other side, there are unreal constructs (e.g. roles). It is obvious that 
    /// these two types are used for different purposes. 
    /// </remarks>        
    public class User : Subject
    {
        #region Attributes
      
        public virtual string Email { get; set; }

        public virtual string Password { get; set; }
        public virtual string PasswordSalt { get; set; }

        public virtual string SecurityQuestion { get; set; }
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

        #endregion


        #region Methods
      
        public User()
        {
            Groups = new List<Group>();
        }

        #endregion
    }
}