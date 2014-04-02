using System;
using System.Collections.Generic;

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

        /// <summary>
        /// Get or set the email of the user.
        /// </summary>
        /// <remarks>
        /// The security system needs that information for several reasons (e.g. password reset). 
        /// </remarks>
        /// <seealso cref="NA"/>        
        public virtual string Email { get; set; }

        /// <summary>
        /// Get or set the registration date of the user.
        /// </summary>
        /// <remarks>
        /// The security system does not require this information. Nevertheless, this 
        /// information might be important for the administration / user management.
        /// </remarks>
        /// <seealso cref="NA"/> 
        public virtual DateTime RegistrationDate { get; set; }

        /// <summary>
        /// Get or set the last activity date of the user.
        /// </summary>
        /// <remarks>
        /// The security system does not require this information. Nevertheless, this 
        /// information might be important for the administration / user management.
        /// </remarks>
        /// <seealso cref="NA"/> 
        public virtual DateTime LastActivityDate { get; set; }

        /// <summary>
        /// Get or set the last lock out date of the user.
        /// </summary>
        /// <remarks>
        /// The security system does not require this information. Nevertheless, this 
        /// information might be important for the administration / user management.
        /// </remarks>
        /// <seealso cref="NA"/> 
        public virtual DateTime LastLockOutDate { get; set; }

        /// <summary>
        /// Get or set the last login date of the user.
        /// </summary>
        /// <remarks>
        /// The security system does not require this information. Nevertheless, this 
        /// information might be important for the administration / user management.
        /// </remarks>
        /// <seealso cref="NA"/> 
        public virtual DateTime LastLoginDate { get; set; }

        /// <summary>
        /// Get or set the last password change date of the user.
        /// </summary>
        /// <remarks>
        /// The security system does not require this information. Nevertheless, this 
        /// information might be important for the administration / user management.
        /// </remarks>
        /// <seealso cref="NA"/> 
        public virtual DateTime LastPasswordChangeDate { get; set; }

        /// <summary>
        /// Get or set the approval property of the user.
        /// </summary>
        /// <remarks>
        /// The security system needs that information for authentication issues.
        /// </remarks>
        /// <seealso cref="NA"/> 
        public virtual Boolean IsApproved { get; set; }

        /// <summary>
        /// Get or set the lock out property of the user.
        /// </summary>
        /// <remarks>
        /// The security system needs that information for authentication issues.
        /// </remarks>
        /// <seealso cref="NA"/> 
        public virtual Boolean IsLockedOut { get; set; }

        #endregion


        #region Associations

        /// <summary>
        /// Get or set the roles of the user.
        /// </summary>
        /// <remarks>
        /// The security system is based on the user-role security approach. That is why each 
        /// user is assigned to a set of roles. This information is important to calculate 
        /// the effective rights of the user, because the security system needs to have all 
        /// relevant data for security calculations.
        /// 
        /// In addition, this information might be important for the administration / user management.
        /// </remarks>
        /// <seealso cref="NA"/>        
        public virtual ICollection<Role> Roles { get; set; }

        #endregion


        #region Methods

        /// <summary>
        /// Create an empty user construct.
        /// </summary>
        /// <remarks>
        /// This is the constructor of the user entity. Since a user contains a 
        /// collection of roles, this method is needed to create an empty user.
        /// Otherwise, the collection is not going to be initialized.
        /// </remarks>
        /// <seealso cref="NA"/>
        /// <param>
        /// NA
        /// </param>       
        public User()
        {
            Roles = new List<Role>();
        }

        #endregion
    }
}