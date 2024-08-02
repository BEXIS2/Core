using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace BEXIS.JSON.Helpers.Extensions
{
    public static class JsonExtensions
    {
        public static void TransformToMatchClassTypes(JObject jObject, Type classType)
        {
            foreach (var property in jObject.Properties())
            {
                string propertyName = property.Name;
                JTokenType propertyType = property.Value.Type;
                PropertyInfo propertyInformation = classType.GetProperty(propertyName, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);

                if (propertyInformation != null)
                {
                    // Arrays / Lists / ...
                    if (propertyType != JTokenType.Array && propertyInformation.PropertyType.IsGenericType && propertyInformation.PropertyType.GetGenericTypeDefinition() == typeof(List<>))
                    {
                        jObject.Property(propertyName).Value = new JArray(jObject.Property(propertyName).Value);
                    }

                }

                if (property.Value.Type == JTokenType.Object)
                {
                    Console.WriteLine((JObject)property.Value + ", " + propertyInformation.PropertyType);
                    TransformToMatchClassTypes((JObject)property.Value, propertyInformation.PropertyType);
                }
                else if (property.Value.Type == JTokenType.Array)
                {
                    Type itemType = propertyInformation.PropertyType.GetGenericArguments()[0];
                    foreach (var item in (JArray)property.Value)
                    {
                        if (item.Type == JTokenType.Object)
                        {
                            TransformToMatchClassTypes((JObject)item, itemType);
                        }
                    }
                }
            }
        }
    }
}
