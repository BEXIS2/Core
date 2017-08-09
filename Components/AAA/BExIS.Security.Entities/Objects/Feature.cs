using BExIS.Security.Entities.Authorization;
using System.Collections.Generic;
using Vaiona.Entities.Common;

namespace BExIS.Security.Entities.Objects
{
    public class Feature : BaseEntity
    {
        public Feature()
        {
            Children = new List<Feature>();
            Permissions = new List<FeaturePermission>();
            Workflows = new List<Workflow>();
        }

        public virtual ICollection<Feature> Ancestors
        {
            get
            {
                var ancestors = new List<Feature>();

                if (Parent == null) return ancestors;

                ancestors.Add(Parent);
                ancestors.AddRange(Parent.Ancestors);
                return ancestors;
            }
        }

        public virtual ICollection<Feature> Children { get; set; }
        public virtual string Description { get; set; }
        public virtual bool IsPublic { get; set; }
        public virtual string Name { get; set; }
        public virtual Feature Parent { get; set; }
        public virtual ICollection<FeaturePermission> Permissions { get; set; }
        public virtual ICollection<Workflow> Workflows { get; set; }
    }
}