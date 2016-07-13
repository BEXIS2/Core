using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Schema;
using Vaiona.Utils.Cfg;

/// <summary>
///
/// </summary>        
namespace BExIS.Xml.Helpers
{
    /// <summary>
    ///
    /// </summary>
    /// <remarks></remarks>        
    public class XsdSchemaReader
    {
        private List<string> metadataAttributeNames = new List<string>();
        private List<string> metadataPackageNames = new List<string>();
        private bool ChildIsLast = false;
        private int area = 0;
        private int packages = 0;


        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>        
        public void Read()
        {
            string path = Path.Combine(AppConfiguration.GetModuleWorkspacePath("DCM"), "Metadata", "schema.xsd");
            //string path = Path.Combine(AppConfiguration.GetModuleWorkspacePath("DCM"), "Metadata", "ABCD", "ABCD_2.06.XSD");
            //string path = Path.Combine(AppConfiguration.GetModuleWorkspacePath("DCM"), "Metadata", "eml", "eml.xsd");

            XmlTextReader xsd_file = new XmlTextReader(path);
            XmlSchema schema_file = new XmlSchema();
            schema_file = XmlSchema.Read(xsd_file, ValidationCallback);

            // Add the customer schema to a new XmlSchemaSet and compile it.
            // Any schema validation warnings and errors encountered reading or 
            // compiling the schema are handled by the ValidationEventHandler delegate.
            XmlSchemaSet schemaSet = new XmlSchemaSet();
            schemaSet.ValidationEventHandler += new ValidationEventHandler(ValidationCallback);
            schemaSet.Add(schema_file);
            schemaSet.Compile();

            // Retrieve the compiled XmlSchema object from the XmlSchemaSet
            // by iterating over the Schemas property.
            XmlSchema customerSchema = null;
            foreach (XmlSchema schema in schemaSet.Schemas())
            {
                customerSchema = schema;
            }

            // Iterate over each XmlSchemaElement in the Values collection
            // of the Elements property.
            foreach (XmlSchemaElement element in customerSchema.Elements.Values)
            {
                readXmlSchemaElement(element, null);
            }
            Debug.WriteLine("-------------------------------------------------");
            Debug.WriteLine("-------------metadataAttributeNames--------------");
            Debug.WriteLine("-------------------------------------------------");
            if (metadataAttributeNames.Count > 0)
            {
                metadataAttributeNames.ForEach(p => Debug.WriteLine(p));
            }

            Debug.WriteLine("-------------metadataPackageNames--------------");
            Debug.WriteLine("-------------------------------------------------");
            if (metadataPackageNames.Count > 0)
            {
                metadataPackageNames.ForEach(p => Debug.WriteLine(p));
            }
            Debug.WriteLine("-------------------------------------------------");
            Debug.WriteLine("Packages :" + packages);
            Debug.WriteLine("PackagesAsParents :" + metadataPackageNames.Count);
            Debug.WriteLine("Attributes :" + metadataAttributeNames.Count);
        }

        private void readXmlSchemaElement(XmlSchemaElement element, XmlSchemaElement parent)
        {
            //Debug.WriteLine("----------------------------------------------");

            string name = "";

            for (int i = 0; i < area; i++)
            {
                name += "-";
            }

            name += element.Name;
            Debug.WriteLine(name);

            // Get the complex type of the Customer element.
            XmlSchemaComplexType complexType = element.ElementSchemaType as XmlSchemaComplexType;

            if (complexType != null)
            {
                packages++;
                //Debug.WriteLine("     Type : " + complexType.GetType().FullName);

                // If the complex type has any attributes, get an enumerator 
                // and write each attribute name to the Debug.
                if (complexType.AttributeUses.Count > 0)
                {
                    IDictionaryEnumerator enumerator =
                        complexType.AttributeUses.GetEnumerator();

                    while (enumerator.MoveNext())
                    {
                        XmlSchemaAttribute attribute =
                            (XmlSchemaAttribute)enumerator.Value;

                        //Debug.WriteLine("Attribute: " + attribute.Name);
                    }
                }

                if (complexType.ContentModel != null)
                {
                    //Debug.WriteLine("    ContentModel Type : " + complexType.ContentModel.GetType().FullName);
                    if (complexType.ContentModel.GetType().Equals(typeof(XmlSchemaSimpleContent)))
                    {
                            metadataAttributeNames.Add(element.Name);

                            int count = GetXmlSchemaElementCount(parent);

                            //if (count == 1)
                            //{ 
                            //    string parentName = findParentElement(parent.Parent).Name;
                            //    if (!metadataPackageNames.Contains(parentName)) metadataPackageNames.Add(parentName);
                            //}
                            //else
                                if (!metadataPackageNames.Contains(parent.Name)) metadataPackageNames.Add(parent.Name);

                        //ChildIsLast = true;

                    }

                }
                
                   //Debug.WriteLine("    ContentModel Type : null");



                // Get the sequence particle of the complex type.
                XmlSchemaSequence sequence = complexType.ContentTypeParticle as XmlSchemaSequence;
                if (sequence != null)
                {

                    area++;
                    // Iterate over each XmlSchemaElement in the Items collection.
                    foreach (XmlSchemaObject childElement in sequence.Items)
                    {
                        //Debug.WriteLine("         ChildElement: "+ childElement.Name);
                        if (childElement.GetType().Equals(typeof(XmlSchemaElement)))
                            readXmlSchemaElement((XmlSchemaElement)childElement, element);

                        else if (childElement.GetType().Equals(typeof(XmlSchemaAny)))
                        {
                            XmlSchemaAny xsy = (XmlSchemaAny)childElement;
                            Debug.WriteLine("+++++++++++++++++++++++++SPECIAL XMLSCHEMAANY : " + xsy.ToString());
                        }
                        else
                            Debug.WriteLine("**************************XMLSCHEMAOBJECT: " + childElement.LineNumber + " : "+ childElement.LinePosition);

                        

                    }

                    if(ChildIsLast)
                    {
                        metadataPackageNames.Add(element.Name);
                        ChildIsLast = false;
                    }

                    area--;
                }


            }

            //Debug.WriteLine("----------------------------------------------");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param name="xmlSchemaElement"></param>
        /// <returns></returns>
        private int GetXmlSchemaElementCount(XmlSchemaElement xmlSchemaElement)
        {
            // Get the complex type of the Customer element.
            XmlSchemaComplexType complexType = xmlSchemaElement.ElementSchemaType as XmlSchemaComplexType;

            if (complexType != null)
            { 
               return  (complexType.ContentTypeParticle as XmlSchemaSequence).Items.OfType<XmlSchemaElement>().Count();
                     
            }

            return 0;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param name="xmlSchemaObject"></param>
        /// <returns></returns>
        private XmlSchemaComplexType findParentElement(XmlSchemaObject xmlSchemaObject)
        {
            if(!xmlSchemaObject.GetType().Equals(typeof(XmlSchemaComplexType)))
            {
                if(xmlSchemaObject.Parent!=null)
                    return findParentElement(xmlSchemaObject.Parent);
                else
                    return (XmlSchemaComplexType)xmlSchemaObject;
            }
            else
                return (XmlSchemaComplexType)xmlSchemaObject;

        }

        /// <summary>
        /// 
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void ValidationCallback(object sender, ValidationEventArgs args)
        {
            if (args.Severity == XmlSeverityType.Warning)
                Debug.WriteLine("WARNING: ");
            else if (args.Severity == XmlSeverityType.Error)
                Debug.WriteLine("ERROR: ");

            Debug.WriteLine(args.Message);
        }
    }
}
