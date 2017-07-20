using BExIS.Utils.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic;
using Telerik.Web.Mvc;

namespace BExIS.Utils.Extensions
{
    public static class QueryableExtensions
    {
        public static IQueryable<TEntity> FilterBy<TEntity, TModel>(this IQueryable<TEntity> queryable, List<FilterDescriptor> filterDescriptors)
        {
            // Extraction of Model Properties
            var expressions = new List<string>();

            foreach (var filterDescriptor in filterDescriptors)
            {
                var modelProperty = typeof(TModel).GetProperty(filterDescriptor.Member);

                if (modelProperty == null)
                    continue;

                var queryAttributes = modelProperty.GetCustomAttributes(typeof(QueryAttribute), true)
                    .Where(ca => ((QueryAttribute)ca).EntityType == typeof(TEntity));

                foreach (var queryAttribute in queryAttributes)
                {
                    try
                    {
                        expressions.Add($"({GetExpression(typeof(TEntity), ((QueryAttribute)queryAttribute).PropertyName, filterDescriptor.Value.ToString())})");
                    }
                    catch
                    {
                        // ignored
                    }
                }
            }

            return expressions.Count > 0 ? queryable.Where(string.Join(" OR ", expressions)) : queryable;
        }

        public static string GetExpression(Type currentEntityType, string propertyName, string filterValue)
        {
            //
            var propertyNames = propertyName.Split(new[] { '.' }, 2);

            if (propertyNames.Length > 1)
            {
                if (currentEntityType.GetProperty(propertyNames[0]).PropertyType.GetGenericTypeDefinition() == typeof(ICollection<>))
                {
                    var propertyType = currentEntityType.GetProperty(propertyNames[0]).PropertyType.GenericTypeArguments[0];
                    return $"{propertyNames[0]}.Any({GetExpression(propertyType, propertyNames[1], filterValue)})";
                }
                else
                {
                    var propertyType = currentEntityType.GetProperty(propertyNames[0]).PropertyType;
                    return $"{propertyNames[0]}.{GetExpression(propertyType, propertyNames[1], filterValue)}";
                }
            }

            var propertyInfo = currentEntityType.GetProperty(propertyNames[0]);

            // String
            if (propertyInfo.PropertyType == typeof(string))
            {
                return $"{propertyInfo.Name}.Contains(\"{filterValue}\")";
            }

            // Int16
            if (propertyInfo.PropertyType == typeof(short))
            {
                short value;

                if (short.TryParse(filterValue, out value))
                {
                    return $"{propertyInfo.Name} = {filterValue}";
                }
            }

            // Int32
            if (propertyInfo.PropertyType == typeof(int))
            {
                int value;

                if (int.TryParse(filterValue, out value))
                {
                    return $"{propertyInfo.Name} = {filterValue}";
                }
            }

            // Int64
            if (propertyInfo.PropertyType == typeof(long))
            {
                long value;

                if (long.TryParse(filterValue, out value))
                {
                    return $"{propertyInfo.Name} = {filterValue}";
                }
            }

            throw new Exception();
        }
    }
}