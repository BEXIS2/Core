using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BExIS.Security.Entities.Authorization;
using BExIS.Security.Entities.Subjects;
using Vaiona.Entities.Common;
       
namespace BExIS.Security.Entities.Objects
{       
    public class Feature : BaseEntity
    {
        #region Attributes
    
        public virtual string Name { get; set; }   
        public virtual string Description { get; set; }

        public virtual bool IsPublic { get; set; }

        #endregion


        #region Associations
      
        public virtual Feature Parent { get; set; }
        public virtual ICollection<Feature> Children { get; set; }
        public virtual ICollection<Task> Tasks { get; set; }
        public virtual ICollection<FeaturePermission> FeaturePermissions { get; set; }

        #endregion


        #region Methods
     
        public Feature()
        {
            Children = new List<Feature>();
            FeaturePermissions = new List<FeaturePermission>();
            Tasks = new List<Task>();
        }
     
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
