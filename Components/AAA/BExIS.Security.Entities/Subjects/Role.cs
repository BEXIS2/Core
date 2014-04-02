using System.Collections.Generic;

/// <summary>
/// The security system contains three types of entities. This namespace is used for
/// all subject entities of the security system - users and roles.
/// </summary>        
namespace BExIS.Security.Entities.Subjects
{
    /// <summary>
    /// A role is one concrete type of a subject.
    /// </summary>
    /// <remarks>
    /// The security system needs to distinguish between different types of subjects.
    /// On the one hand there are real users who register with the system. And on the
    /// other side, there are unreal constructs (e.g. roles). It is obvious that 
    /// these two types are used for different purposes. 
    /// </remarks> 
    public class Role : Subject
    {
        #region Attributes

        /// <summary>
        /// Get or set the description of the role.
        /// </summary>
        /// <remarks>
        /// The security system does not require this information. Nevertheless, this 
        /// information might be important for the administration / role management.
        /// </remarks>
        /// <seealso cref="NA"/>        
        public virtual string Description { get; set; }

        #endregion


        #region Associations

        /// <summary>
        /// Get or set the users of the role.
        /// </summary>
        /// <remarks>
        /// The security system is based on the user-role security approach. That is why each 
        /// role is assigned to a set of users. This information is important to calculate 
        /// the effective rights of a user, because the security system needs to have all 
        /// relevant data for security calculations.
        /// 
        /// In addition, this information might be important for the administration / role management.
        /// </remarks>
        /// <seealso cref="NA"/>
        public virtual ICollection<User> Users { get; set; }

        #endregion


        #region Methods

        /// <summary>
        /// Create an empty role construct.
        /// </summary>
        /// <remarks>
        /// This is the constructor of the role entity. Since a role contains a 
        /// collection of users, this method is needed to create an empty role.
        /// Otherwise, the collection is not going to be initialized.
        /// </remarks>
        /// <seealso cref="NA"/>
        /// <param>
        /// NA
        /// </param>       
        public Role()
        {
            Users = new List<User>();
        }

        #endregion

    }
}