using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vaiona.Entities.Common;

/// <summary>
/// The security system contains three types of entities. This namespace is used for
/// all subject entities of the security system - users and roles.
/// </summary> 
namespace BExIS.Security.Entities.Subjects
{
    /// <summary>
    /// A security user includes all security-relevant information about the corresponding
    /// user.
    /// </summary>
    /// <remarks>
    /// The security system distinguish between domain information and security information 
    /// of a user. That is why the security system stores two separate entities (user and
    /// security user).
    /// </remarks>        
    public class SecurityUser : BaseEntity
    {
        /// <summary>
        /// Get or set the name of the security user.
        /// </summary>
        /// <remarks>
        /// The security system needs that information for several reasons (e.g. user identification). 
        /// </remarks>
        /// <seealso cref="NA"/>  
        public virtual string Name { get; set; }

        /// <summary>
        /// Get or set the password of the security user.
        /// </summary>
        /// <remarks>
        /// The security system needs that information for user authentification. 
        /// </remarks>
        /// <seealso cref="NA"/>  
        public virtual string Password { get; set; }

        /// <summary>
        /// Get or set the password salt of the security user.
        /// </summary>
        /// <remarks>
        /// The security system needs that information for password encryption. 
        /// </remarks>
        /// <seealso cref="NA"/>  
        public virtual string PasswordSalt { get; set; }

        /// <summary>
        /// Get or set the security question of the security user.
        /// </summary>
        /// <remarks>
        /// The security system needs that information for password reset. 
        /// </remarks>
        /// <seealso cref="NA"/>  
        public virtual string SecurityQuestion { get; set; }

        /// <summary>
        /// Get or set the security answer of the security user.
        /// </summary>
        /// <remarks>
        /// The security system needs that information for password reset. 
        /// </remarks>
        /// <seealso cref="NA"/>  
        public virtual string SecurityAnswer { get; set; }

        /// <summary>
        /// Get or set the security answer salt of the security user.
        /// </summary>
        /// <remarks>
        /// The security system needs that information for security answer encryption. 
        /// </remarks>
        /// <seealso cref="NA"/>  
        public virtual string SecurityAnswerSalt { get; set; }

        /// <summary>
        /// Get or set the password failure count of the security user.
        /// </summary>
        /// <remarks>
        /// The security system needs that information for user authentification. 
        /// </remarks>
        /// <seealso cref="NA"/>  
        public virtual Int32 PasswordFailureCount { get; set; }

        /// <summary>
        /// Get or set the security answer failure count of the security user.
        /// </summary>
        /// <remarks>
        /// The security system needs that information for password reset. 
        /// </remarks>
        /// <seealso cref="NA"/>  
        public virtual Int32 SecurityAnswerFailureCount { get; set; }

        /// <summary>
        /// Get or set the last password failure date of the security user.
        /// </summary>
        /// <remarks>
        /// The security system does not require this information. Nevertheless, this 
        /// information might be important for the administration / user management.
        /// </remarks>
        /// <seealso cref="NA"/>  
        public virtual DateTime LastPasswordFailureDate { get; set; }

        /// <summary>
        /// Get or set the last security answer failure date of the security user.
        /// </summary>
        /// <remarks>
        /// The security system does not require this information. Nevertheless, this 
        /// information might be important for the administration / user management.
        /// </remarks>
        /// <seealso cref="NA"/>  
        public virtual DateTime LastSecurityAnswerFailureDate { get; set; }
    }
}
