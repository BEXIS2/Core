using BExIS.Utils.Filters;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic;

namespace BExIS.Utils.Extensions
{
    public static class QueryableExtensions
    {
        public static IQueryable<TEntity> FilterBy<TEntity, TModel>(this IQueryable<TEntity> queryable, string filterProperties)
        {
            // Extraction of Model Properties
            var filters = filterProperties.Split(',').Where(s => !string.IsNullOrWhiteSpace(s)).Select(s => s.Trim()).ToArray();
            var expressions = new List<string>();

            foreach (var modelProperty in typeof(TModel).GetProperties())
            {
                var queryAttributes = modelProperty.GetCustomAttributes(typeof(QueryAttribute), true)
                    .Where(ca => ((QueryAttribute)ca).EntityType == typeof(TEntity));

                foreach (var queryAttribute in queryAttributes)
                {
                    foreach (var filter in filters)
                    {
                        expressions.Add($"{((QueryAttribute)queryAttribute).PropertyName}.Contains(\"{filter}\")");
                    }
                }
            }

            return expressions.Count > 0 ? queryable.Where(string.Join(" OR ", expressions)) : queryable;
        }
    }
}