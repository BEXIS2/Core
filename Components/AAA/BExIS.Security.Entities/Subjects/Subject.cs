using System.Collections.Generic;
using BExIS.Security.Entities.Objects;
using Vaiona.Entities.Common;


/// <summary>
/// The security system contains three types of entities. This namespace is used for
/// all subject entities of the security system - users and roles.
/// </summary>        
namespace BExIS.Security.Entities.Subjects
{
    /// <summary>
    /// In general, the security system of BExIS is acting on subjects, objects and 
    /// security issues. A subject is either a person (users) or a thing, that is acting 
    /// like a person (role).
    /// </summary>
    /// <remarks>
    /// The security system needs to be flexible and modular. Therefore it is important
    /// to use generic entities which will ease the management later on 
    /// (e.g. permissions - creation, deletion,...). That is why the security system of 
    /// BExIS is dealing with subjects and not only concrete entities like users and
    /// roles.
    /// </remarks>        
    public abstract class Subject : BaseEntity
    {
        #region Attributes

        /// <summary>
        /// Get or set the name of the subject. 
        /// </summary>
        /// <remarks>
        /// In addition to the BaseEntity properties, every subject of the security system 
        /// must have a name, e.g. user name or role name.
        /// </remarks>
        /// <seealso cref="NA"/>        
        public virtual string Name { get; set; }

        #endregion


        #region Associations

        /// <summary>
        /// Get or set the permissions of the subject.
        /// </summary>
        /// <remarks>
        /// Within the security system, each subject (user or role) have a set of permissions. 
        /// With the help of this information, the security system will grant or deny a
        /// requested access.
        /// </remarks>
        /// <seealso cref="NA"/>        
        public virtual ICollection<FeaturePermission> FeaturePermissions { get; set; }

        #endregion


        #region Methods

        /// <summary>
        /// Create an empty subject construct.
        /// </summary>
        /// <remarks>
        /// This is the constructor of the subject entity. Since a subject contains a 
        /// collection of permissions, this method is needed to create an empty subject.
        /// Otherwise, the collection is not going to be initialized.
        /// </remarks>
        /// <seealso cref="NA"/>
        /// <param>
        /// NA
        /// </param>       
        public Subject()
        {
            FeaturePermissions = new List<FeaturePermission>();
        }

        #endregion
    }
}