using System;
using System.Collections.Generic;
using System.Linq;

namespace Vaiona.Entities.Security
{
    public enum SecurityObjectType
    {
        Feature = 0, Entity = 1
    }

    public enum AccessRuleMergeOption
    {
        MaximumRight = 0, MinimumRight = 1, Normal = 2
    }

    public class AccessRuleEntity
    {
        public virtual Int64 Id { get; set; }
        public virtual Int32 VersionNo { get; set; }

        public virtual string SecurityKey { get; set; }
        public virtual SecurityObjectType SecurityObjectType { get; set; }
        public virtual string RuleBody { get; set; }

        public virtual string DisplayName { get; set; }

        public virtual AccessRuleEntity Parent { get; set; }
        public virtual ICollection<AccessRuleEntity> Children { get; set; }

        public virtual List<string> SimpleRoles
        {
            get
            {
                var a = string.IsNullOrWhiteSpace(RuleBody) ? new List<string>() : RuleBody.Replace(" | ", '|'.ToString()).Split('|').ToList();
                a.RemoveAll(p => string.IsNullOrWhiteSpace(p));
                return (a);
            }
            set
            {
                RuleBody = String.Join(" | ", value);
            }
        }
    }
}