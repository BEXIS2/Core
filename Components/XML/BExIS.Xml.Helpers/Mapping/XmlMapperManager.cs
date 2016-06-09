using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;
using BExIS.Dlm.Entities.Data;
using BExIS.Dlm.Services.Data;
using BExIS.Dlm.Services.MetadataStructure;
using BExIS.Xml.Models;
using BExIS.Xml.Models.Mapping;
using BExIS.Xml.Services;
using Vaiona.Utils.Cfg;


namespace BExIS.Xml.Helpers.Mapping
{
    public class XmlMapperManager
    {
        public XmlMapper xmlMapper;
        public XmlDocument mappingFile;

        private XmlSchemaManager xmlSchemaManager;

        private bool addAlsoEmptyNode = false;

        /// <summary>
        /// Load from mapping file
        /// create a XmlMapper
        /// </summary>
        /// <returns></returns>
        public XmlMapper Load(string mappingFilePath, string username)
        {
            xmlMapper = new XmlMapper();

            mappingFile = new XmlDocument();
            mappingFile.Load(mappingFilePath);

            XmlNode root = mappingFile.DocumentElement;

            #region get id and name of standard

            XmlNode mapping = mappingFile.GetElementsByTagName(XmlMapperTags.mapping.ToString())[0];

            if (mapping.Attributes.Count > 0)
            {
                foreach (XmlAttribute attr in mapping.Attributes)
                {
                    if (attr.Name.Equals(XmlMapperAttributes.id.ToString()))
                        xmlMapper.Id = Convert.ToInt32(attr.Value);

                    if (attr.Name.Equals(XmlMapperAttributes.name.ToString()))
                        xmlMapper.Name = attr.Value;
                }
            }

            #endregion

            #region create Header as xmlMappingHeader

            XmlMappingHeader xmlMappingHeader = new XmlMappingHeader();

            XmlNode header = mappingFile.GetElementsByTagName(XmlMapperTags.header.ToString())[0];

            foreach(XmlNode xmlNode in header.ChildNodes)
            {
                if(xmlNode.NodeType.Equals(System.Xml.XmlNodeType.Element))
                {
                    #region create destination

                    if(xmlNode.Name.Equals(XmlMapperTags.destination.ToString()))
                    {
                        xmlMappingHeader.Destination = Destination.Convert(xmlNode);
                    }

                    #endregion

                    #region read & add packages
                    if (xmlNode.Name.Equals(XmlMapperTags.packages.ToString()))
                    {
                        foreach(XmlNode childNode in xmlNode.ChildNodes)
                        {
                            if (childNode.Name.Equals(XmlMapperTags.package.ToString()))
                            {
                                xmlMappingHeader.AddToPackages(childNode);
                            }
                        
                        }
                    }

                    #endregion

                    #region read & add Attributes
                        if (xmlNode.Name.Equals(XmlMapperTags.attributes.ToString()))
                        {
                            foreach (XmlNode childNode in xmlNode.ChildNodes)
                            {
                                if (childNode.Name.Equals(XmlMapperTags.attribute.ToString()))
                                {
                                    xmlMappingHeader.AddToAttributes(childNode);
                                }

                            }
                        }
                    #endregion

                    #region read & add schemas

                    if (xmlNode.Name.Equals(XmlMapperTags.schema.ToString()))
                    {
                        xmlMappingHeader.AddToSchemas(xmlNode);
                    }

                    #endregion
                }
            }

            xmlMapper.Header = xmlMappingHeader;

            #endregion

            #region create Routes

            XmlNodeList routes = mappingFile.GetElementsByTagName(XmlMapperTags.routes.ToString())[0].ChildNodes;
            foreach (XmlNode childNode in routes)
            {
                xmlMapper.Routes.Add(XmlMappingRoute.Convert(childNode));
            }

            #endregion

            #region xmlschema

            xmlSchemaManager = new XmlSchemaManager();

            if (xmlMapper.Header.Schemas.Count > 0)
            {
                xmlSchemaManager = new XmlSchemaManager();
                string schemaPath = Path.Combine(AppConfiguration.GetModuleWorkspacePath("DCM"), xmlMapper.Header.Schemas.First().Value);
                xmlSchemaManager.Load(schemaPath, username);
            }

            #endregion

            return xmlMapper;
        }

        public XmlDocument Generate(XmlDocument metadataXml, long id,bool addEmptyNode = false)
        {
            addAlsoEmptyNode = addEmptyNode;

            #region abcd (metadata from bexis to abcd)

            XmlDocument newMetadata = new XmlDocument();

            newMetadata.AppendChild(newMetadata.CreateElement(xmlMapper.Header.Destination.Prefix, xmlMapper.Header.Destination.XPath, xmlMapper.Header.Destination.NamepsaceURI));
            XmlNode root = newMetadata.DocumentElement;



            // create nodes
            newMetadata = mapNode(newMetadata, newMetadata.DocumentElement, metadataXml.DocumentElement);

            // add required attributes
            newMetadata = addAttributes(newMetadata, newMetadata.DocumentElement);

            //add root attributes
            foreach (KeyValuePair<string, string> attribute in xmlMapper.Header.Attributes)
            {
                XmlAttribute attr = newMetadata.CreateAttribute(attribute.Key);
                attr.Value = attribute.Value;
                root.Attributes.Append(attr);
            }

            //add root namespaces
            foreach (KeyValuePair<string, string> package in xmlMapper.Header.Packages)
            {
                XmlAttribute attr = newMetadata.CreateAttribute(package.Key);
                attr.Value = package.Value;
                root.Attributes.Append(attr);
            }

            string path = Path.Combine(AppConfiguration.GetModuleWorkspacePath("DIM"), "Metadata " + id + ".xml");

            newMetadata.Save(path);
            ValidationEventHandler eventHandler = new ValidationEventHandler(validationEventHandler);

            // the following call to Validate succeeds.
            //document.Validate(eventHandler);

            #endregion

            return newMetadata;
        }

        public string Export(XmlDocument metadataXml , long datasetVersionId)
        {
            #region abcd (metadata from bexis to abcd)

            XmlDocument newMetadata = new XmlDocument();
            //newMetadata.Load(defaultFilePath);
            //XmlNode root = newMetadata.DocumentElement;

            if (!String.IsNullOrEmpty(xmlMapper.Header.Destination.XPath))
            {
                newMetadata.AppendChild(newMetadata.CreateElement(xmlMapper.Header.Destination.Prefix, xmlMapper.Header.Destination.XPath, xmlMapper.Header.Destination.NamepsaceURI));
            }
            else
            {
                newMetadata.AppendChild(newMetadata.CreateElement("root"));
            }

            XmlNode root = newMetadata.DocumentElement;

            

            // create nodes
            newMetadata = mapNode(newMetadata, newMetadata.DocumentElement, metadataXml.DocumentElement);

            // add required attributes
            newMetadata = addAttributes(newMetadata, newMetadata.DocumentElement);

            //add root attributes
            foreach (KeyValuePair<string, string> attribute in xmlMapper.Header.Attributes)
            {
                XmlAttribute attr = newMetadata.CreateAttribute(attribute.Key);
                attr.Value = attribute.Value;
                root.Attributes.Append(attr);
            }

            //add root namespaces
            foreach( KeyValuePair<string,string> package  in xmlMapper.Header.Packages )
            {
                XmlAttribute attr = newMetadata.CreateAttribute(package.Key);
                attr.Value = package.Value;
                root.Attributes.Append(attr);
            }

            string path = Path.Combine(AppConfiguration.DataPath, getStorePath(datasetVersionId));

            newMetadata.Save(path);

            //XmlReaderSettings settings = new XmlReaderSettings();
            //settings.Schemas.Add(xmlSchemaManager.Schema);
            //settings.ValidationType = ValidationType.Schema;

            //XmlReader reader = XmlReader.Create(path, settings);
            //XmlDocument document = new XmlDocument();
            //document.Load(reader);

            ValidationEventHandler eventHandler = new ValidationEventHandler(validationEventHandler);

            // the following call to Validate succeeds.
            //document.Validate(eventHandler);

            #endregion

            return path;
        }

        public XmlDocument Export(XmlDocument metadataXml, long datasetVersionId, bool save = false)
        {
            #region abcd (metadata from bexis to abcd)

            XmlDocument newMetadata = new XmlDocument();
            //newMetadata.Load(defaultFilePath);
            //XmlNode root = newMetadata.DocumentElement;

            if (!String.IsNullOrEmpty(xmlMapper.Header.Destination.XPath))
            {
                newMetadata.AppendChild(newMetadata.CreateElement(xmlMapper.Header.Destination.Prefix, xmlMapper.Header.Destination.XPath, xmlMapper.Header.Destination.NamepsaceURI));
            }
            else
            {
                newMetadata.AppendChild(newMetadata.CreateElement("root"));
            }

            XmlNode root = newMetadata.DocumentElement;



            // create nodes
            newMetadata = mapNode(newMetadata, newMetadata.DocumentElement, metadataXml.DocumentElement);

            // add required attributes
            newMetadata = addAttributes(newMetadata, newMetadata.DocumentElement);

            //add root attributes
            foreach (KeyValuePair<string, string> attribute in xmlMapper.Header.Attributes)
            {
                XmlAttribute attr = newMetadata.CreateAttribute(attribute.Key);
                attr.Value = attribute.Value;
                root.Attributes.Append(attr);
            }

            //add root namespaces
            foreach (KeyValuePair<string, string> package in xmlMapper.Header.Packages)
            {
                XmlAttribute attr = newMetadata.CreateAttribute(package.Key);
                attr.Value = package.Value;
                root.Attributes.Append(attr);
            }

            string path = getStorePath(datasetVersionId);

            newMetadata.Save(Path.Combine(AppConfiguration.DataPath, path));

            //XmlReaderSettings settings = new XmlReaderSettings();
            //settings.Schemas.Add(xmlSchemaManager.Schema);
            //settings.ValidationType = ValidationType.Schema;

            //XmlReader reader = XmlReader.Create(path, settings);
            //XmlDocument document = new XmlDocument();
            //document.Load(reader);

            ValidationEventHandler eventHandler = new ValidationEventHandler(validationEventHandler);

            // the following call to Validate succeeds.
            //document.Validate(eventHandler);

            #endregion

            return newMetadata;
        }


        private void validationEventHandler(object sender, ValidationEventArgs e)
        {
            switch (e.Severity)
            {
                case XmlSeverityType.Error:
                    Debug.WriteLine("Error: {0}", e.Message);
                    break;
                case XmlSeverityType.Warning:
                    Debug.WriteLine("Warning {0}", e.Message);
                    break;
            }

        }

        // add content from nodes from source to destination
        private XmlDocument mapNode(XmlDocument destinationDoc, XmlNode destinationParentNode, XmlNode sourceNode)
        {

            // load the xpath from source node
            string sourceXPath = XmlUtility.GetXPathToNode(sourceNode);

            //load route from mappingFile of the source node
            XmlMappingRoute route = xmlMapper.GetRoute(sourceXPath);

            XmlNode destinationPNode = destinationParentNode;

            // check if this source xpath is mapped in the xmlMapper
            if (xmlMapper.SourceExist(sourceXPath))
            {
                //check if there is text inside the source node
                if (!sourceNode.InnerText.Equals("") || addAlsoEmptyNode)
                {
                    // get name of the destination node
                    string destinationTagName = route.GetDestinationTagNames();

                    // get xpath of the destination node
                    string destinationXPath = route.GetDestinationXPath();

                    // create xmlnode in document
                    XmlNode destinationNode = XmlUtility.CreateNode(destinationTagName, destinationDoc);

                    //if (type == element), get content
                    if (xmlMapper.IsSourceElement(sourceXPath))
                    {
                        destinationNode.InnerText = sourceNode.InnerText;
                    }

                    //check if nodes in destination not exist, create them
                    string destinationParentXPath = route.GetDestinationParentXPath();
                    string currentParentXPath = XmlUtility.GetXPathToNode(destinationParentNode);
                    if (currentParentXPath != destinationParentXPath)
                    {
                        destinationParentNode = createMissingNodes(destinationParentXPath, currentParentXPath, destinationParentNode, destinationDoc);
                    }

                    //if destinationNode exist , it must be e sequence
                    if (XmlUtility.ExistAsChild(destinationParentNode, destinationNode))
                    {
                        #region child exist
                            //get parent sequence or sequence below
                            XmlNode parent = destinationParentNode;
                            XmlNode current = destinationNode;

                            List<string> temp = new List<string>();
                            
                            while (!current.Name.Equals(route.Destination.ParentSequence))
                            {
                                temp.Add(current.LocalName);
                                current = parent;
                                parent = parent.ParentNode;
                            }

                            // is current a sequence?
                            if (current.Name.Equals(route.Destination.ParentSequence))
                            {
                                XmlNode newCurrent = XmlUtility.CreateNode(current.LocalName, destinationDoc);

                                //destinatenode exits in every child of the parent?
                                if(parent.ChildNodes.Count>0)
                                {
                                    bool foundNodeWithoutDestionationNode = false;

                                    foreach (XmlNode child in parent.ChildNodes)
                                    {
                                        if (route.Destination.XPath.Contains(child.Name))
                                        {
                                            //if destination node not exits
                                            if (XmlUtility.GetXmlNodeByName(child, destinationTagName) == null)
                                            {
                                                newCurrent = child;
                                                foundNodeWithoutDestionationNode = true;
                                            }
                                            else
                                            {
                                                if(sourceNode.Attributes.Count>0 )
                                                {
                                                    if (sourceNode.Attributes["number"] != null)
                                                    {
                                                        int number = Convert.ToInt32(sourceNode.Attributes["number"].Value);
                                                        if (number > 1)
                                                        {
                                                            foundNodeWithoutDestionationNode = true;
                                                        }

                                                    }
                                                }
                                            }

                                        }
                                    }

                                    if ((temp.Count>=2 && foundNodeWithoutDestionationNode) || !foundNodeWithoutDestionationNode)
                                    {
                                        addChild(parent,newCurrent);
                                    }
                                }
                                else
                                {
                                    addChild(parent, newCurrent);
                                }

                                temp.Reverse();

                                if (temp.Count > 0)
                                {
                                    parent = newCurrent;
                                    //add child ToString parent which is a sequence
                                    foreach (string newName in temp)
                                    {
                                        if (!temp.Last().Equals(newName))
                                        {
                                            XmlNode newNode = XmlUtility.GetXmlNodeByName(parent, newName);

                                            if (newNode == null)
                                            {
                                                newNode = XmlUtility.CreateNode(newName, destinationDoc);
                                                addChild(parent, newNode);
                                            }

                                            parent = newNode;
                                        }
                                        else
                                        {
                                            addChild(parent, destinationNode);
                                        }
                                    }
                                }
                                else
                                {
                                    newCurrent.InnerText = destinationNode.InnerText;
                                    addChild(parent, destinationNode);
                                }

                                destinationPNode = parent;
                            }
                            else
                                destinationPNode = current;

                        #endregion
                    }
                    else
                    {
                        addChild(destinationParentNode,destinationNode);
                        destinationPNode = destinationNode;
                    }

                }
            }

            if (sourceNode.HasChildNodes)
            {
                foreach (XmlNode childNode in sourceNode.ChildNodes)
                {
                   destinationDoc = mapNode(destinationDoc, destinationPNode, childNode);
                }
            }

            return destinationDoc;
        }

        // add required attributes
        private XmlDocument addAttributes(XmlDocument doc, XmlNode parentNode)
        {
         
            if (xmlSchemaManager.HasAttributes(parentNode))
            {
                addAttributesToXmlNode(doc, parentNode, xmlSchemaManager.GetAttributes(parentNode));
            }

            if (parentNode.ChildNodes.Count > 0)
            {
                foreach (XmlNode child in parentNode.ChildNodes)
                {
                    doc = addAttributes(doc, child);
                }
            }

            

            return doc;
        }

        // add attributes for a XmlNode
        private XmlNode addAttributesToXmlNode(XmlDocument doc, XmlNode node, List<XmlSchemaAttribute> attributes)
        {
            foreach (XmlSchemaAttribute attribute in attributes)
            {
                if (attribute.Name != null )//&& attribute.DefaultValue!=null)
                {
                    XmlAttribute attr = doc.CreateAttribute(attribute.Name);
                    if (attribute.Name.Equals("language"))
                    {
                        attr.InnerXml = "en";
                        node.Attributes.Append(attr);
                    }
                    else
                    {
                        if (attribute.DefaultValue != null)
                        {
                            attr.InnerXml = attribute.DefaultValue;
                            node.Attributes.Append(attr);
                        }
                    }

                    
                }
            }

            return node;
        }

        /// <summary>
        /// Add Child to a node
        /// based on the selected schema
        /// the child will added to the schema defined index
        /// </summary>
        /// <param name="node">parent xmlnode</param>
        /// <param name="child">child xmlnode</param>
        /// <returns>parent with added childnode</returns>
        private XmlNode addChild(XmlNode node, XmlNode child)
        {
            //node has no childrens --> add it
            if (node.ChildNodes.Count == 0)
                node.AppendChild(child);

            //has childrens, need to find the rigth position of the new element
            else
            {
                int index = xmlSchemaManager.GetIndexOfChild(node, child);
                if (index != -1)
                {
                    bool added = false;
                    foreach (XmlNode seqChild in node.ChildNodes)
                    {
                        if (index < xmlSchemaManager.GetIndexOfChild(node, seqChild))
                        {
                            node.InsertBefore(child, seqChild);
                            added = true;
                        }
                    }


                    if (!added)
                    {
                        node.AppendChild(child);
                    }
                }
                else
                {
                    node.AppendChild(child);
                }


            }

            return node;
        }

        /// <summary>
        /// Add missing node to the desitnation document
        /// </summary>
        /// <param name="destinationParentXPath"></param>
        /// <param name="currentParentXPath"></param>
        /// <param name="parentNode"></param>
        /// <param name="doc"></param>
        /// <returns></returns>
        private XmlNode createMissingNodes(string destinationParentXPath, string currentParentXPath, XmlNode parentNode, XmlDocument doc)
        {
                string dif = destinationParentXPath.Substring(currentParentXPath.Length);

                List<string> temp =  dif.Split('/').ToList();
                temp.RemoveAt(0);

                XmlNode parentTemp = parentNode;

                foreach (string s in temp)
                {
                    if (XmlUtility.GetXmlNodeByName(parentTemp, s) == null)
                    {
                        XmlNode t = XmlUtility.CreateNode(s, doc);
         
                        addChild(parentTemp, t);
                        parentTemp = t;
                    }
                    else
                    {
                        XmlNode t = XmlUtility.GetXmlNodeByName(parentTemp, s);
                        parentTemp = t;
                    }
                }

                return parentTemp;
        }

        private string getStorePath(long datasetVersionId)
        {
            DatasetManager datasetManager = new DatasetManager();
            DatasetVersion datasetVersion = datasetManager.GetDatasetVersion(datasetVersionId);

            Dataset dataset = datasetManager.GetDataset(datasetVersionId);

            MetadataStructureManager metadataStructureManager = new MetadataStructureManager();
            string md_title = metadataStructureManager.Repo.Get(datasetVersion.Dataset.MetadataStructure.Id).Name;

            string path = IOHelper.GetDynamicStorePath(datasetVersionId, datasetVersion.Id,
              XmlDatasetHelper.GetInformation(datasetVersion, NameAttributeValues.title) + "_" + md_title, ".xml");

            return path;
        }
    }
}
