using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Xml.Schema;

namespace BExIS.Xml.Helpers
{
    public class XmlSchemaUtility
    {
        /// <summary>
        /// Get All Complex Types from the Schema.
        /// </summary>
        /// <param name="schema"></param>
        /// <returns></returns>
        public static List<XmlSchemaComplexType> GetAllComplexTypes(XmlSchema schema)
        {
            List<XmlSchemaComplexType> complexTypes = new List<XmlSchemaComplexType>();

            foreach (XmlSchemaObject item in schema.Items)
            {
                if (item is XmlSchemaComplexType) complexTypes.Add((XmlSchemaComplexType)item);
            }

            return complexTypes;
        }

        /// <summary>
        /// Get all simple types from the schema.
        /// </summary>
        /// <param name="schema"></param>
        /// <returns></returns>
        public static List<XmlSchemaSimpleType> GetAllSimpleTypes(XmlSchema schema)
        {
            List<XmlSchemaSimpleType> simpleTypes = new List<XmlSchemaSimpleType>();

            foreach (XmlSchemaObject item in schema.Items)
            {
                if (item is XmlSchemaSimpleType) simpleTypes.Add((XmlSchemaSimpleType)item);
            }

            return simpleTypes;
        }

        /// <summary>
        /// Get all simple types from the schema.
        /// </summary>
        /// <param name="schema"></param>
        /// <returns></returns>
        public static List<XmlSchemaAttribute> GetAllAttributes(XmlSchema schema)
        {
            List<XmlSchemaAttribute> attributes = new List<XmlSchemaAttribute>();

            foreach (XmlSchemaObject item in schema.Items)
            {
                if (item is XmlSchemaAttribute) attributes.Add((XmlSchemaAttribute)item);
            }

            return attributes;
        }

        /// <summary>
        /// Get all elements from the schema.
        /// </summary>
        /// <param name="schema"></param>
        /// <returns></returns>
        public static List<XmlSchemaElement> GetAllElements(XmlSchema schema)
        {
            List<XmlSchemaElement> elements = new List<XmlSchemaElement>();

            foreach (XmlSchemaElement item in schema.Elements.Values)
            {
                if (item is XmlSchemaElement)
                {
                    Debug.WriteLine("                          ");
                    Debug.WriteLine("__________________________");
                    Debug.WriteLine(item.Name);
                    Debug.WriteLine("---------------------------------");
                    //elements.Add((XmlSchemaElement)item);
                    elements = GetElements((XmlSchemaElement)item, elements, true, new List<XmlSchemaElement>());
                }
            }

            return elements;
        }

        public static List<XmlSchemaGroup> GetAllGroups(XmlSchema schema)
        {
            List<XmlSchemaGroup> groups = new List<XmlSchemaGroup>();

            foreach (XmlSchemaGroup item in schema.Groups.Values)
            {
                groups.Add(item);
            }

            return groups;
        }

        public static List<XmlSchemaElement> GetAllElements(XmlSchemaObject obj, bool recursive, List<XmlSchemaElement> allElements)
        {
            List<XmlSchemaElement> elements = new List<XmlSchemaElement>();

            // Element
            if (obj.GetType().Equals(typeof(XmlSchemaElement)))
            {
                XmlSchemaElement element = (XmlSchemaElement)obj;

                XmlSchemaComplexType complexType = element.ElementSchemaType as XmlSchemaComplexType;

                if (complexType == null) complexType = element.SchemaType as XmlSchemaComplexType;

                if (complexType != null)
                {
                    #region sequence  as XmlSchemaSequence

                    /// Get the sequence particle of the complex type.
                    XmlSchemaSequence sequence = complexType.ContentTypeParticle as XmlSchemaSequence;
                    if (sequence != null)
                    {
                        // Iterate over each XmlSchemaElement in the Items collection.
                        foreach (XmlSchemaObject childElement in sequence.Items)
                        {
                            elements = GetElements(childElement, elements, recursive, allElements);
                        }
                    }

                    #endregion sequence  as XmlSchemaSequence

                    #region sequence as XmlSchemaAll

                    /// Get the sequence particle of the complex type.
                    XmlSchemaAll all = complexType.ContentTypeParticle as XmlSchemaAll;
                    if (all != null)
                    {
                        // Iterate over each XmlSchemaElement in the Items collection.
                        foreach (XmlSchemaObject childElement in all.Items)
                        {
                            elements = GetElements(childElement, elements, recursive, allElements);
                        }
                    }

                    #endregion sequence as XmlSchemaAll

                    //#region sequence as XmlSchemaAll
                    ///// Get the sequence particle of the complex type.
                    //XmlSchemaAll all = complexType.ContentTypeParticle as Content;
                    //if (all != null)
                    //{
                    //    // Iterate over each XmlSchemaElement in the Items collection.
                    //    foreach (XmlSchemaObject childElement in all.Items)
                    //    {
                    //        elements = GetElements(childElement, elements, recursive, allElements);
                    //    }
                    //}

                    //#endregion

                    #region choice

                    // check if it is e choice
                    XmlSchemaChoice choice = complexType.ContentTypeParticle as XmlSchemaChoice;
                    if (choice != null)
                    {
                        // Iterate over each XmlSchemaElement in the Items collection.
                        foreach (XmlSchemaObject childElement in choice.Items)
                        {
                            elements = GetElements(childElement, elements, recursive, allElements);
                        }
                    }

                    #endregion choice
                }
            }

            if (obj.GetType().Equals(typeof(XmlSchemaComplexType)))
            {
                XmlSchemaComplexType complexType = obj as XmlSchemaComplexType;

                if (complexType != null)
                {
                    #region sequence

                    /// Get the sequence particle of the complex type.
                    XmlSchemaSequence sequence = complexType.ContentTypeParticle as XmlSchemaSequence;
                    if (sequence != null)
                    {
                        // Iterate over each XmlSchemaElement in the Items collection.
                        foreach (XmlSchemaObject childElement in sequence.Items)
                        {
                            elements = GetElements(childElement, elements, recursive, allElements);
                        }
                    }

                    #endregion sequence

                    #region choice

                    // check if it is e choice
                    XmlSchemaChoice choice = complexType.ContentTypeParticle as XmlSchemaChoice;
                    if (choice != null)
                    {
                        // Iterate over each XmlSchemaElement in the Items collection.
                        foreach (XmlSchemaObject childElement in choice.Items)
                        {
                            elements = GetElements(childElement, elements, recursive, allElements);
                        }
                    }

                    #endregion choice
                }
            }

            if (obj is XmlSchemaGroup)
            {
                XmlSchemaGroup group = obj as XmlSchemaGroup;

                #region sequence

                /// Get the sequence particle of the complex type.
                XmlSchemaSequence sequence = group.Particle as XmlSchemaSequence;
                if (sequence != null)
                {
                    // Iterate over each XmlSchemaElement in the Items collection.
                    foreach (XmlSchemaObject childElement in sequence.Items)
                    {
                        elements = GetElements(childElement, elements, recursive, allElements);
                    }
                }

                #endregion sequence

                #region choice

                // check if it is e choice
                XmlSchemaChoice choice = group.Particle as XmlSchemaChoice;
                if (choice != null)
                {
                    // Iterate over each XmlSchemaElement in the Items collection.
                    foreach (XmlSchemaObject childElement in choice.Items)
                    {
                        elements = GetElements(childElement, elements, recursive, allElements);
                    }
                }

                #endregion choice
            }

            //if (obj is XmlSchemaSequence)
            //{
            //    // Iterate over each XmlSchemaElement in the Items collection.
            //    foreach (XmlSchemaObject childElement in ((XmlSchemaSequence)obj).Items)
            //    {
            //        elements = GetAllElements(childElement, recursive, allElements);
            //    }
            //}

            //if (obj is XmlSchemaChoice)
            //{
            //    // Iterate over each XmlSchemaElement in the Items collection.
            //    foreach (XmlSchemaObject childElement in ((XmlSchemaChoice)obj).Items)
            //    {
            //        elements = GetAllElements(childElement, recursive, allElements);
            //    }
            //}

            return elements;
        }

        private static List<XmlSchemaElement> GetElements(XmlSchemaObject element, List<XmlSchemaElement> list, bool recursive, List<XmlSchemaElement> allElements)
        {
            // Element
            if (element.GetType().Equals(typeof(XmlSchemaElement)))
            {
                XmlSchemaElement child = (XmlSchemaElement)element;

                if (child.Name != null)
                {
                    if (list.Where(e => e.Name.Equals(child.Name)).Count() == 0)
                    {
                        //if (!child.SchemaTypeName.Name.Equals(parentTypeName))
                        //{
                        list.Add(child);

                        if (recursive)
                        {
                            Debug.WriteLine("--<" + child.Name);

                            XmlSchemaComplexType complexType = child.ElementSchemaType as XmlSchemaComplexType;
                            if (complexType != null)
                            {
                                #region sequence

                                /// Get the sequence particle of the complex type.
                                XmlSchemaSequence sequence = complexType.ContentTypeParticle as XmlSchemaSequence;
                                if (sequence != null)
                                {
                                    // Iterate over each XmlSchemaElement in the Items collection.
                                    foreach (XmlSchemaObject childElement in sequence.Items)
                                    {
                                        list = GetElements(childElement, list, recursive, allElements);
                                    }
                                }

                                #endregion sequence

                                #region choice

                                // check if it is e choice
                                XmlSchemaChoice choice = complexType.ContentTypeParticle as XmlSchemaChoice;
                                if (choice != null)
                                {
                                    // Iterate over each XmlSchemaElement in the Items collection.
                                    foreach (XmlSchemaObject childElement in choice.Items)
                                    {
                                        list = GetElements(childElement, list, recursive, allElements);
                                    }
                                }

                                #endregion choice
                            }
                        }

                        //}
                    }
                }
                else
                {
                    if (child.RefName != null)
                    {
                        XmlSchemaElement refElement = allElements.Where(e => e.QualifiedName.Equals(child.RefName)).FirstOrDefault();
                        if (refElement != null)
                        {
                            //set parameters from the child to the refernce
                            refElement.MinOccurs = child.MinOccurs;
                            refElement.MaxOccurs = child.MaxOccurs;

                            list.Add(refElement);

                            if (recursive)
                            {
                                Debug.WriteLine("--<" + refElement.Name);

                                XmlSchemaComplexType complexType = refElement.ElementSchemaType as XmlSchemaComplexType;
                                if (complexType != null)
                                {
                                    #region sequence

                                    /// Get the sequence particle of the complex type.
                                    XmlSchemaSequence sequence = complexType.ContentTypeParticle as XmlSchemaSequence;
                                    if (sequence != null)
                                    {
                                        // Iterate over each XmlSchemaElement in the Items collection.
                                        foreach (XmlSchemaObject childElement in sequence.Items)
                                        {
                                            list = GetElements(childElement, list, recursive, allElements);
                                        }
                                    }

                                    #endregion sequence

                                    #region choice

                                    // check if it is e choice
                                    XmlSchemaChoice choice = complexType.ContentTypeParticle as XmlSchemaChoice;
                                    if (choice != null)
                                    {
                                        // Iterate over each XmlSchemaElement in the Items collection.
                                        foreach (XmlSchemaObject childElement in choice.Items)
                                        {
                                            list = GetElements(childElement, list, recursive, allElements);
                                        }
                                    }

                                    #endregion choice
                                }
                            }
                        }
                    }
                }
            }
            else
                if (element.GetType().Equals(typeof(XmlSchemaChoice)))
            {
                XmlSchemaChoice choice = (XmlSchemaChoice)element;

                // Iterate over each XmlSchemaElement in the Items collection.
                foreach (XmlSchemaObject childElement in choice.Items)
                {
                    list = GetElements(childElement, list, recursive, allElements);
                }
            }
            else
                if (element.GetType().Equals(typeof(XmlSchemaSequence)))
            {
                XmlSchemaSequence sequence = (XmlSchemaSequence)element;

                // Iterate over each XmlSchemaElement in the Items collection.
                foreach (XmlSchemaObject childElement in sequence.Items)
                {
                    list = GetElements(childElement, list, recursive, allElements);
                }
            }

            return list;
        }

        /// <summary>
        /// returns true if a simpletype is a restriction
        /// </summary>
        /// <param name="restrictionType"></param>
        /// <returns></returns>
        public static bool IsEnumerationType(XmlSchemaObject restrictionType)
        {
            XmlSchemaObjectCollection facets = new XmlSchemaObjectCollection();

            if (restrictionType is XmlSchemaSimpleTypeRestriction)
                facets = ((XmlSchemaSimpleTypeRestriction)restrictionType).Facets;

            if (restrictionType is XmlSchemaSimpleContentRestriction)
                facets = ((XmlSchemaSimpleContentRestriction)restrictionType).Facets;

            foreach (XmlSchemaObject facet in facets)
            {
                if (facet is XmlSchemaEnumerationFacet) return true;
            }

            return false;
        }

        /// <summary>
        /// return true if a Element is a SimpleType
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        public static bool IsSimpleType(XmlSchemaElement element)
        {
            if (element.ElementSchemaType is XmlSchemaSimpleType)
                return true;

            if (element.ElementSchemaType is XmlSchemaComplexType)
            {
                XmlSchemaComplexType complexType = (XmlSchemaComplexType)element.ElementSchemaType;
                if (complexType.ContentType is XmlSchemaContentType.TextOnly ||
                   complexType.ContentType is XmlSchemaContentType.Mixed ||
                   complexType.ContentType is XmlSchemaContentType.Empty)
                    return true;
            }

            return false;
        }

        /// <summary>
        /// return true if a Element is a Choice
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        public static bool IsChoiceType(XmlSchemaElement element)
        {
            if (element.ElementSchemaType is XmlSchemaComplexType)
            {
                XmlSchemaComplexType ct = element.ElementSchemaType as XmlSchemaComplexType;

                if (ct != null)
                {
                    #region choice

                    // check if it is e choice
                    XmlSchemaChoice choice = ct.ContentTypeParticle as XmlSchemaChoice;
                    if (choice != null)
                    {
                        return true;
                    }

                    #endregion choice
                }
            }

            return false;
        }

        /// <summary>
        /// return true if a Element is a Choice
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        public static bool IsChoiceType(XmlSchemaComplexType complexType)
        {
            if (complexType != null)
            {
                #region choice

                // check if it is e choice
                XmlSchemaChoice choice = complexType.ContentTypeParticle as XmlSchemaChoice;
                if (choice != null)
                {
                    return true;
                }

                #endregion choice
            }

            return false;
        }

        public static bool IsAllSimpleType(List<XmlSchemaElement> elements)
        {
            bool allSimple = true;

            foreach (XmlSchemaElement element in elements)
            {
                if (!IsSimpleType(element))
                {
                    allSimple = false;
                }
            }

            return allSimple;
        }

        public static XmlSchemaComplexType GetComplextType(XmlSchemaElement element)
        {
            if (element.ElementSchemaType is XmlSchemaComplexType)
                return element.ElementSchemaType as XmlSchemaComplexType;

            return null;
        }

        public static List<XmlSchemaElement> GetAllSimpleElements(List<XmlSchemaElement> elements)
        {
            List<XmlSchemaElement> simpleElementList = new List<XmlSchemaElement>();

            foreach (XmlSchemaElement element in elements)
            {
                if (IsSimpleType(element)) simpleElementList.Add(element);
            }

            return simpleElementList;
        }

        public static List<XmlSchemaElement> GetAllComplexElements(List<XmlSchemaElement> elements)
        {
            List<XmlSchemaElement> simpleElementList = new List<XmlSchemaElement>();

            foreach (XmlSchemaElement element in elements)
            {
                if (!IsSimpleType(element)) simpleElementList.Add(element);
            }

            return simpleElementList;
        }
    }
}