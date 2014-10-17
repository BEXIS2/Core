using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Schema;

namespace BExIS.Xml.Services
{
    public class XmlSchemaUtility
    {

        public static List<XmlSchemaComplexType> GetAllComplexTypes(XmlSchema schema)
        {
            List<XmlSchemaComplexType> complexTypes = new List<XmlSchemaComplexType>();

            foreach (XmlSchemaObject item in schema.Items)
            {
                if (item is XmlSchemaComplexType) complexTypes.Add((XmlSchemaComplexType)item);
            }

            return complexTypes;
        }

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
                    elements = GetElements((XmlSchemaElement)item, elements, true);
                }
            }

            return elements;
        }

        public static List<XmlSchemaElement> GetAllElements(XmlSchemaObject obj, bool recursive)
        {
            List<XmlSchemaElement> elements  = new List<XmlSchemaElement>(); 

            // Element
            if (obj.GetType().Equals(typeof(XmlSchemaElement)))
            {
                XmlSchemaElement element  = (XmlSchemaElement)obj;

                XmlSchemaComplexType complexType = element.ElementSchemaType as XmlSchemaComplexType;

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
                            elements = GetElements(childElement, elements, recursive);
                        }
                    }

                    #endregion

                    #region choice
                    // check if it is e choice
                    XmlSchemaChoice choice = complexType.ContentTypeParticle as XmlSchemaChoice;
                    if (choice != null)
                    {
                        // Iterate over each XmlSchemaElement in the Items collection.
                        foreach (XmlSchemaObject childElement in choice.Items)
                        {
                            elements = GetElements(childElement, elements, recursive);
                        }
                    }
                    #endregion
                }
            }


            return elements;

        }

        private static List<XmlSchemaElement> GetElements(XmlSchemaObject element, List<XmlSchemaElement> list, bool recursive )
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
                                        list = GetElements(childElement, list, recursive);
                                    }
                                }

                                #endregion

                                #region choice
                                // check if it is e choice
                                XmlSchemaChoice choice = complexType.ContentTypeParticle as XmlSchemaChoice;
                                if (choice != null)
                                {
                                    // Iterate over each XmlSchemaElement in the Items collection.
                                    foreach (XmlSchemaObject childElement in choice.Items)
                                    {
                                        list = GetElements(childElement, list, recursive);
                                    }
                                }
                                #endregion
                            }
                        }
                        //}
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
                    list = GetElements(childElement, list, recursive);
                }
            }
            else 
                if (element.GetType().Equals(typeof(XmlSchemaSequence)))
            {
                XmlSchemaSequence sequence = (XmlSchemaSequence)element;
                    
                // Iterate over each XmlSchemaElement in the Items collection.
                foreach (XmlSchemaObject childElement in sequence.Items)
                {
                    list = GetElements(childElement, list, recursive);
                }
            }

            return list;
        }

    }
}
