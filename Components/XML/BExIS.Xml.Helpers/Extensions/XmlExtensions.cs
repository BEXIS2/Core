using System;
using System.Collections.Generic;
using System.Reflection;
using System.Xml;

namespace BExIS.Xml.Helpers.Extensions
{
    public static class XmlExtensions
    {
        public static void TransformXmlToMatchClassTypes(XmlNode node, Type classType)
        {
            foreach (XmlNode childNode in node.ChildNodes)
            {
                string propertyName = childNode.Name;
                PropertyInfo propertyInfo = classType.GetProperty(propertyName, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);

                if (propertyInfo != null)
                {
                    // If it's a list or array type, ensure the XML node appropriately reflects that
                    if (propertyInfo.PropertyType.IsGenericType && propertyInfo.PropertyType.GetGenericTypeDefinition() == typeof(List<>))
                    {
                        Type itemType = propertyInfo.PropertyType.GetGenericArguments()[0];

                        foreach (XmlNode listItem in childNode.ChildNodes)
                        {
                            // Recursively transform the XML node to match the item type of the collection
                            TransformXmlToMatchClassTypes(listItem, itemType);
                        }
                    }
                    else if (childNode.HasChildNodes)
                    {
                        TransformXmlToMatchClassTypes(childNode, propertyInfo.PropertyType);
                    }
                }
                else
                {
                    // Handle cases where the property is not found in the class type
                    Console.WriteLine($"Property '{propertyName}' not found in '{classType.Name}'.");
                }
            }
        }
    }
}
