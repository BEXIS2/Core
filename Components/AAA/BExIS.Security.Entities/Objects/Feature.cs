using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BExIS.Security.Entities.Security;
using BExIS.Security.Entities.Subjects;
using Vaiona.Entities.Common;

/// <summary>
/// The security system contains three types of entities. This namespace is used for
/// all object entities of the security system, e.g. features or tasks.
/// </summary>           
namespace BExIS.Security.Entities.Objects
{
    /// <summary>
    /// A feature is one concrete type of an object.
    /// </summary>
    /// <remarks>
    /// The security system needs to distinguish between different types of objects.
    /// On the one hand there are features - functional items of the system. And on the
    /// other side, there are non-functional items - data that is inside the system. It 
    /// is obvious that these two types are used in different ways.
    /// </remarks>        
    public class Feature : BaseEntity
    {
        #region Attributes

        /// <summary>
        /// Get or set the name of the feature.
        /// </summary>
        /// <remarks>
        /// In addition to the BaseEntity properties, every feature of the security system 
        /// must have a name.
        /// </remarks>
        /// <seealso cref="NA"/>      
        public virtual string Name { get; set; }

        /// <summary>
        /// Get or set the description of the feature.
        /// </summary>
        /// <remarks>
        /// The security system does not require this information. Nevertheless, this 
        /// information might be important for the administration / feature management.
        /// </remarks>
        /// <seealso cref="NA"/>      
        public virtual string Description { get; set; }

        #endregion


        #region Associations

        /// <summary>
        /// Get or set the parent of the feature.
        /// </summary>
        /// <remarks>
        /// Within the security system, there is a tree-shaped hierarchy of features. This 
        /// means, each feature has exactly one single parent (except the root) and a set of
        /// children. On every feature access request, the security system is using this 
        /// hierarchy (bottom-up) for the calculation of the effective rights.
        /// </remarks>
        /// <seealso cref="NA"/>      
        public virtual Feature Parent { get; set; }

        /// <summary>
        /// Get or set the children of the feature.
        /// </summary>
        /// <remarks>
        /// Within the security system, there is a tree-shaped hierarchy of features. This 
        /// means, each feature has exactly one single parent (except the root) and a set of
        /// children. On every feature access request, the security system is using this 
        /// hierarchy (bottom-up) for the calculation of the effective rights.
        /// </remarks>
        /// <seealso cref="NA"/> 
        public virtual ICollection<Feature> Children { get; set; }

        /// <summary>
        /// Get or set the tasks of the feature.
        /// </summary>
        /// <remarks>
        /// In general, a feature is an entity that is more domain specific. But the 
        /// security system has to work on system specific entities. That is why a feature
        /// contains a set of tasks. These are really related to that kind of system actions,
        /// which are suitable for the feature access. 
        /// </remarks>
        /// <seealso cref="NA"/> 
        public virtual ICollection<Task> Tasks { get; set; }

        /// <summary>
        /// Get or set the permissions of the feature.
        /// </summary>
        /// <remarks>
        /// Within the security system, each feature has a set of feature permissions,
        /// because a feature is part of it. So, every feature permission contains the
        /// information about a relationship of a subject with an object (functional item - feature).
        /// </remarks>
        /// <seealso cref="NA"/> 
        public virtual ICollection<FeaturePermission> FeaturePermissions { get; set; }

        #endregion


        #region Methods

        /// <summary>
        /// Create an empty feature construct.
        /// </summary>
        /// <remarks>
        /// This is the constructor of the feature entity. Since a feature contains a 
        /// collection of children, tasks and feature permissions, this method is needed to create an empty feature.
        /// Otherwise, the collections are not going to be initialized.
        /// </remarks>
        /// <seealso cref="NA"/>
        /// <param>
        /// NA
        /// </param>       
        public Feature()
        {
            Children = new List<Feature>();
            FeaturePermissions = new List<FeaturePermission>();
            Tasks = new List<Task>();
        }

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param></param>       
        public virtual ICollection<Feature> Ancestors
        {
            get
            {
                List<Feature> ancestors = new List<Feature>();

                if (Parent != null)
                {
                    ancestors.Add(Parent);
                    ancestors.AddRange(Parent.Ancestors);
                }

                return ancestors;
            }
        }

        #endregion
    }
}
