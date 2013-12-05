using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BExIS.Security.Entities.Security;
using Vaiona.Entities.Common;

namespace BExIS.Security.Entities.Objects
{
    public class Feature : BaseEntity
    {
        #region Attributes

        public virtual string Name { get; set; }
        public virtual string Description { get; set; }

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

        #endregion
    }
}
