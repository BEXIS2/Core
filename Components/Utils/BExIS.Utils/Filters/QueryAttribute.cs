using System;

namespace BExIS.Utils.Filters
{
    [AttributeUsage(AttributeTargets.Property)]
    public class QueryAttribute : Attribute
    {
        public QueryAttribute(Type entityType, string propertyName)
        {
            EntityType = entityType;
            PropertyName = propertyName;
        }

        public Type EntityType { get; set; }
        public string PropertyName { get; set; }
    }
}