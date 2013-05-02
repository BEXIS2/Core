using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BExIS.Core.Data
{
    [AttributeUsageAttribute(AttributeTargets.Class, Inherited = true, AllowMultiple = true)]
    public class AutomaticMaterializationInfoAttribute: Attribute
    {
        public AutomaticMaterializationInfoAttribute(string sourcePropertyName, Type sourcePropertyType, string targetPropertyName, Type targetPropertyType)
        {
            SourcePropertyName = sourcePropertyName;
            SourcePropertyType = sourcePropertyType;

            TargetPropertyName = targetPropertyName;
            TargetPropertyType = targetPropertyType;
        }

        public string SourcePropertyName { get; set; }
        public Type SourcePropertyType { get; set; }

        public string TargetPropertyName { get; set; }
        public Type TargetPropertyType { get; set; }
    }
}
