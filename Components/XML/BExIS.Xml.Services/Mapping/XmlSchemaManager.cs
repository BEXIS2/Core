using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Schema;

namespace BExIS.Xml.Services.Mapping
{
    public class XmlSchemaManager
    {
        public List<XmlSchemaComplexType> ComplexTypes { get; set; }
        public List<XmlSchemaElement> Elements { get; set; }
        public XmlSchema Schema;

        public XmlSchemaManager()
        {
            Elements = new List<XmlSchemaElement>();
            ComplexTypes = new List<XmlSchemaComplexType>();
        }
        
        /// <summary>
        /// Load Schema from path
        /// </summary>
        /// <param name="path"></param>
        public void Load(string path)
        {
            XmlTextReader xsd_file = new XmlTextReader(path);
            Schema = XmlSchema.Read(xsd_file, verifyErrors);
            XmlSchema selectedSchema; 

            XmlSchemaSet schemaSet = new XmlSchemaSet();
            schemaSet.ValidationEventHandler += new ValidationEventHandler(verifyErrors);
            schemaSet.Add(Schema);
            schemaSet.Compile();

            foreach (XmlSchema currentSchema in schemaSet.Schemas())
            {
                selectedSchema = currentSchema;
                Elements.AddRange(GetAllElements(selectedSchema));
                ComplexTypes.AddRange(GetAllComplexTypes(selectedSchema));
            }
        }

        /// <summary>
        /// Return a list of all complext types in the schema
        /// </summary>
        /// <param name="schema"></param>
        /// <returns>List<XmlSchemaComplexType></returns>
        private List<XmlSchemaComplexType> GetAllComplexTypes(XmlSchema schema)
        {
            return XmlSchemaUtility.GetAllComplexTypes(schema);
        }

        /// <summary>
        /// Return a list of all elements in the schema
        /// </summary>
        /// <param name="schema"></param>
        /// <returns></returns>
        private List<XmlSchemaElement> GetAllElements(XmlSchema schema)
        {
            return XmlSchemaUtility.GetAllElements(schema);
        }

        /// <summary>
        /// Check if the node is defined as a sequence in the schema
        /// </summary>
        /// <param name="node">xmlnode to check</param>
        /// <returns></returns>
        public bool IsSequence(XmlNode node)
        {
            XmlSchemaElement element = Elements.Where(e => e.Name.Equals(node.LocalName)).FirstOrDefault();

            if (element != null)
            {
                XmlSchemaComplexType type = element.ElementSchemaType as XmlSchemaComplexType;
                if (type != null)
                {
                    XmlSchemaSequence sequence = type.ContentTypeParticle as XmlSchemaSequence;
                    if (sequence != null)
                    {
                        return true;
                    }

                    XmlSchemaChoice choice = type.ContentTypeParticle as XmlSchemaChoice;
                    if (choice != null)
                    {
                        if (!existXmlSchemaElement(choice.Items))
                        {
                            foreach (XmlSchemaObject obj in choice.Items)
                            {
                                return isSequence(obj);
                            }
                        }

                        return false;
                    }
                }
            }

            return false;
        }

        private bool isSequence(XmlSchemaObject element)
        {
            XmlSchemaSequence sequence = element as XmlSchemaSequence;
            if (sequence != null)
            {
                return true;
            }

            XmlSchemaChoice choice = element as XmlSchemaChoice;
            if (choice != null)
            {
                if (!existXmlSchemaElement(choice.Items))
                {
                    foreach (XmlSchemaObject obj in choice.Items)
                    {
                        return isSequence((XmlSchemaElement)obj);
                    }
                }

                return false;
            }

            XmlSchemaElement e = element as XmlSchemaElement;

            if (e != null)
            {
                XmlSchemaComplexType type = e.ElementSchemaType as XmlSchemaComplexType;
                if (type != null)
                {
                    Debug.WriteLine("Element");
                }
            }

            return false;
        }

        private bool existXmlSchemaElement(XmlSchemaObjectCollection collection)
        {
            foreach (XmlSchemaObject obj in collection)
            {
                XmlSchemaElement element = obj as XmlSchemaElement;
                if (element != null)
                {
                    XmlSchemaComplexType type = element.ElementSchemaType as XmlSchemaComplexType;
                    if (type != null) return true;
                }
            }

            return false;
        }


        /// <summary>
        /// returns true if the xmlnode has attributes
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        public bool HasAttributes(XmlNode node)
        {
            XmlSchemaElement element = Elements.Where(e => e.Name.Equals(node.LocalName)).FirstOrDefault();
            if (element != null)
            {
                XmlSchemaComplexType type = element.ElementSchemaType as XmlSchemaComplexType;

                if (type != null)
                {
                    // If the complex type has any attributes, get an enumerator 
                    // and write each attribute name to the Debug.
                    if (type.AttributeUses.Count > 0)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Get a list of all attributes from the xmlnode
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        public List<XmlSchemaAttribute> GetAttributes(XmlNode node)
        {
            List<XmlSchemaAttribute> listOfAttributes = new List<XmlSchemaAttribute>(); 

            XmlSchemaElement element = Elements.Where(e => e.Name.Equals(node.LocalName)).FirstOrDefault();
            if (element != null)
            {
                XmlSchemaComplexType type = element.ElementSchemaType as XmlSchemaComplexType;
                if (type != null)
                { 
                    // If the complex type has any attributes, get an enumerator 
                    // and write each attribute name to the Debug.
                    if (type.AttributeUses.Count > 0)
                    {
                        foreach (XmlSchemaObject obj in type.AttributeUses.Values)
                        { 
                            XmlSchemaAttribute attr = (XmlSchemaAttribute)obj;
                            listOfAttributes.Add(attr);
                        }
                    }
                }
            }

            return listOfAttributes;
        }

        public int GetIndexOfChild(XmlNode node, XmlNode child)
        {
            XmlSchemaElement parent = Elements.Where(e => e.Name.Equals(node.LocalName)).FirstOrDefault();

            if (parent != null)
            {
                Debug.WriteLine("********** CHILDREN fo ONE Node");
                List<XmlSchemaElement> list = XmlSchemaUtility.GetAllElements(parent, false);

                if (list.Where(e => e.Name.Equals(child.Name)).Count() > 0)
                {
                    for (int i = 0; i < list.Count(); i++)
                    {
                        if (list.ElementAt(i).Name.Equals(child.Name)) return i;
                    }
                }
            }
            else
            {
                Debug.WriteLine("PARENT = NULL ---> " + node.LocalName);
                return 0;
            }

                
            return -1;
        }

        //event handler to manage the errors
        private void verifyErrors(object sender, ValidationEventArgs args)
        {
            if (args.Severity == XmlSeverityType.Warning)
            {
                Debug.WriteLine(args.Message);
            }

        }
    }
}
