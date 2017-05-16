using BExIS.Utils.Filters;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic;
using Telerik.Web.Mvc;

namespace BExIS.Utils.Extensions
{
    public static class QueryableExtensions
    {
        public static IQueryable<TEntity> FilterBy<TEntity, TModel>(this IQueryable<TEntity> queryable,
            IList<IFilterDescriptor> filterDescriptors)
        {
            // Extraction of Model Properties
            var expressions = new List<string>();

            foreach (var filterDescriptor in filterDescriptors)
            {
                var filter = (FilterDescriptor)filterDescriptor;

                // Query Attributes
                var propertyInfo = typeof(TModel).GetProperty(filter.Member);
                if (propertyInfo != null)
                {
                    var queryAttributes = propertyInfo
                        .GetCustomAttributes(typeof(QueryAttribute), true)
                        .Where(ca => ((QueryAttribute)ca).EntityType == typeof(TEntity));

                    // Query Attribute
                    foreach (var queryAttribute in queryAttributes)
                    {
                    }
                }
            }

            return expressions.Count > 0 ? queryable.Where(string.Join(" OR ", expressions)) : queryable;
        }
    }
}