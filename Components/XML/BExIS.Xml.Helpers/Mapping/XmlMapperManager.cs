using BExIS.Dlm.Entities.Data;
using BExIS.Dlm.Services.Data;
using BExIS.Dlm.Services.MetadataStructure;
using BExIS.IO;
using BExIS.Xml.Models.Mapping;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Schema;
using Vaiona.Utils.Cfg;

namespace BExIS.Xml.Helpers.Mapping
{
    public enum TransactionDirection
    {
        InternToExtern,
        ExternToIntern
    }

    public class XmlMapperManager
    {
        public XmlMapper xmlMapper;
        public XmlDocument mappingFile;

        public TransactionDirection TransactionDirection = TransactionDirection.ExternToIntern;

        private XmlSchemaManager xmlSchemaManager;

        public XmlMapperManager(TransactionDirection transactionDirection)
        {
            this.TransactionDirection = transactionDirection;
        }

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

            #endregion get id and name of standard

            #region create Header as xmlMappingHeader

            XmlMappingHeader xmlMappingHeader = new XmlMappingHeader();

            XmlNode header = mappingFile.GetElementsByTagName(XmlMapperTags.header.ToString())[0];

            foreach (XmlNode xmlNode in header.ChildNodes)
            {
                if (xmlNode.NodeType.Equals(System.Xml.XmlNodeType.Element))
                {
                    #region create destination

                    if (xmlNode.Name.Equals(XmlMapperTags.destination.ToString()))
                    {
                        xmlMappingHeader.Destination = Destination.Convert(xmlNode);
                    }

                    #endregion create destination

                    #region read & add packages

                    if (xmlNode.Name.Equals(XmlMapperTags.packages.ToString()))
                    {
                        foreach (XmlNode childNode in xmlNode.ChildNodes)
                        {
                            if (childNode.Name.Equals(XmlMapperTags.package.ToString()))
                            {
                                xmlMappingHeader.AddToPackages(childNode);
                            }
                        }
                    }

                    #endregion read & add packages

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

                    #endregion read & add Attributes

                    #region read & add schemas

                    if (xmlNode.Name.Equals(XmlMapperTags.schema.ToString()))
                    {
                        xmlMappingHeader.AddToSchemas(xmlNode);
                    }

                    #endregion read & add schemas
                }
            }

            xmlMapper.Header = xmlMappingHeader;

            #endregion create Header as xmlMappingHeader

            #region create Routes

            XmlNodeList routes = mappingFile.GetElementsByTagName(XmlMapperTags.routes.ToString())[0].ChildNodes;
            foreach (XmlNode childNode in routes)
            {
                xmlMapper.Routes.Add(XmlMappingRoute.Convert(childNode));
            }

            #endregion create Routes

            #region xmlschema

            xmlSchemaManager = new XmlSchemaManager();

            if (xmlMapper.Header.Schemas.Count > 0)
            {
                xmlSchemaManager = new XmlSchemaManager();
                string schemaPath = Path.Combine(AppConfiguration.GetModuleWorkspacePath("DCM"), xmlMapper.Header.Schemas.First().Value);
                xmlSchemaManager.Load(schemaPath, username);
            }

            #endregion xmlschema

            return xmlMapper;
        }

        public XmlDocument Generate(XmlDocument metadataXml, long id, bool addEmptyNode = false)
        {
            addAlsoEmptyNode = addEmptyNode;

            #region abcd (metadata from bexis to abcd)

            XmlDocument newMetadata = new XmlDocument();
            XmlDeclaration declaration = newMetadata.CreateXmlDeclaration("1.0", "utf-8", null);
            newMetadata.AppendChild(declaration);

            newMetadata.AppendChild(newMetadata.CreateElement(xmlMapper.Header.Destination.Prefix, xmlMapper.Header.Destination.XPath, xmlMapper.Header.Destination.NamepsaceURI));
            XmlNode root = newMetadata.DocumentElement;

            //FROM DS Comment  it out because of:
            //by generate the namespaces should not added, otherwise single node function by xmldocument is not getting any node
            //XmlAttribute rootAttr = newMetadata.CreateAttribute("xmlns");
            //rootAttr.Value = xmlSchemaManager.Schema.TargetNamespace;
            //root.Attributes.Append(rootAttr);

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

            // the following call to Validate succeeds.
            //document.Validate(eventHandler);

            #endregion abcd (metadata from bexis to abcd)

            return newMetadata;
        }

        public string Export(XmlDocument metadataXml, long datasetVersionId, string exportTo)
        {
            addAlsoEmptyNode = false;

            #region abcd (metadata from bexis to abcd)

            XmlDocument newMetadata = new XmlDocument();
            XmlDeclaration declaration = newMetadata.CreateXmlDeclaration("1.0", "utf-8", null);
            newMetadata.AppendChild(declaration);
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

            // create nodes
            newMetadata = mapNode(newMetadata, null, metadataXml.DocumentElement);

            // add required attributes
            newMetadata = addAttributes(newMetadata, newMetadata.DocumentElement);

            XmlNode root = newMetadata.DocumentElement;

            //add root attributes
            foreach (KeyValuePair<string, string> attribute in xmlMapper.Header.Attributes)
            {
                XmlAttribute attr = newMetadata.CreateAttribute(attribute.Key);
                attr.Value = attribute.Value;
                root.Attributes.Append(attr);
            }

            XmlAttribute rootAttr = newMetadata.CreateAttribute("xmlns");
            rootAttr.Value = xmlSchemaManager.Schema.TargetNamespace;
            root.Attributes.Append(rootAttr);

            //add root namespaces
            foreach (KeyValuePair<string, string> package in xmlMapper.Header.Packages)
            {
                XmlAttribute attr = newMetadata.CreateAttribute(package.Key);
                attr.Value = package.Value;
                root.Attributes.Append(attr);
            }

            string path = Path.Combine(AppConfiguration.DataPath, getStorePath(datasetVersionId, exportTo));

            string dircectory = Path.GetDirectoryName(path);
            FileHelper.CreateDicrectoriesIfNotExist(dircectory);

            newMetadata.Save(path);

            #endregion abcd (metadata from bexis to abcd)

            return path;
        }

        public XmlDocument Export(XmlDocument metadataXml, long datasetVersionId, string exportTo, bool save = false)
        {
            addAlsoEmptyNode = false;

            #region abcd (metadata from bexis to abcd)

            XmlDocument newMetadata = new XmlDocument();

            //add declaration
            XmlDeclaration declaration = newMetadata.CreateXmlDeclaration("1.0", "utf-8", null);
            newMetadata.AppendChild(declaration);

            if (xmlSchemaManager.SchemaSet != null)
            {
                // Add Schema
                newMetadata.Schemas = xmlSchemaManager.SchemaSet;

                //create namespaces
                this.xmlSchemaManager.XmlNamespaceManager = new XmlNamespaceManager(newMetadata.NameTable);
                foreach (var ns in xmlSchemaManager.Schema.Namespaces.ToArray())
                {
                    this.xmlSchemaManager.XmlNamespaceManager.AddNamespace(ns.Name, ns.Namespace);
                }

            }

             // create nodes
            newMetadata = mapNode(newMetadata, null, metadataXml.DocumentElement);

            // add required attributes
            newMetadata = addAttributes(newMetadata, newMetadata.DocumentElement);

            XmlNode root = newMetadata.DocumentElement;
            //root.Prefix = "";
            XmlAttribute rootAttr = null;

            if (xmlSchemaManager.Schema != null)
            {
                //create NameSpaces
                foreach (var nsp in xmlSchemaManager.Schema.Namespaces.ToArray())
                {
                    string attrName = "xmlns";

                    if (!string.IsNullOrEmpty(nsp.Name)) attrName += ":" + nsp.Name;

                    if (root.Attributes[attrName] == null)
                    {
                        rootAttr = newMetadata.CreateAttribute(attrName);
                        rootAttr.Value = nsp.Namespace;
                        root.Attributes.Append(rootAttr);

                        //root.

                        //Add Prefix to root
                        if (nsp.Namespace.Equals(xmlSchemaManager.Schema.TargetNamespace))
                        {
                            if (!string.IsNullOrEmpty(nsp.Name))
                                root.Prefix = nsp.Name;
                            //root.NamespaceURI = xmlSchemaManager.Schema.TargetNamespace;
                        }
                    }
                }
            }

            if (xmlMapper.Header != null)
            {
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
            }

            string path = getStorePath(datasetVersionId, exportTo);
            string fullpath = Path.Combine(AppConfiguration.DataPath, path);

            FileHelper.CreateDicrectoriesIfNotExist(Path.GetDirectoryName(fullpath));

            newMetadata.Save(fullpath);

            #endregion abcd (metadata from bexis to abcd)

            return newMetadata;
        }

        public string Validate(XmlDocument doc)
        {
            string msg = "";

            try
            {
                doc.Schemas = this.xmlSchemaManager.SchemaSet;
                doc.Validate(null);
            }
            catch (Exception ex)
            {
                return ex.Message;
            }

            return msg;
        }

        private XmlDocument mapNode(XmlDocument destinationDoc, XmlNode destinationParentNode, XmlNode sourceNode)
        {
            try
            {
                // load the xpath from source node
                // x[1]\y[2]\f[1]
                string sourceXPath = XmlUtility.GetDirectXPathToNode(sourceNode);
                // x\y\f
                string sourceXMppingFilePath = XmlUtility.GetXPathToNode(sourceNode);

                //load route from mappingFile of the source node
                XmlMappingRoute route = xmlMapper.GetRoute(sourceXMppingFilePath);

                XmlNode destinationPNode = destinationParentNode;

                // check if this source xpath is mapped in the xmlMapper
                if (xmlMapper.SourceExist(sourceXMppingFilePath))
                {
                    if (!string.IsNullOrEmpty(sourceNode.InnerText) || addAlsoEmptyNode)
                    {
                        // get name of the destination node
                        string destinationTagName = route.GetDestinationTagNames();

                        // get xpath of the destination node
                        // X\XType\Y\F
                        string destinationXMppingFilePath = route.GetDestinationXPath();

                        //ToDo checkif the way to map is intern to extern or extern to intern
                        // X[1]\XType[2]\Y[1]\yType[4]\F[1]\yType[2]
                        string destinationXPath = "";

                        if (this.TransactionDirection == TransactionDirection.ExternToIntern)
                            destinationXPath = mapExternPathToInternPathWithIndex(sourceXPath, destinationXMppingFilePath);
                        else
                            destinationXPath = mapInternPathToExternPathWithIndex(sourceXPath, destinationXMppingFilePath);

                        // create xmlnode in document

                        XmlNode destinationNode = XmlUtility.GenerateNodeFromXPath(destinationDoc, destinationDoc as XmlNode,
                            destinationXPath, xmlSchemaManager.Elements, xmlSchemaManager.XmlNamespaceManager); //XmlUtility.CreateNode(destinationTagName, destinationDoc);
                        destinationNode.InnerText = sourceNode.InnerText;

                        //if (type == element), get content
                        if (xmlMapper.IsSourceElement(sourceXPath))
                        {
                        }

                        //destinationParentNode = createMissingNodes(destinationParentXPath, currentParentXPath, destinationParentNode, destinationDoc);

                        ////check if nodes in destination not exist, create them
                        //string destinationParentXPath = route.GetDestinationParentXPath();
                        //string currentParentXPath = XmlUtility.GetDirectXPathToNode(destinationParentNode);
                    }
                }

                if (sourceNode.HasChildNodes)
                {
                    foreach (XmlNode childNode in sourceNode.ChildNodes)
                    {
                        destinationDoc = mapNode(destinationDoc, destinationPNode, childNode);
                    }
                }
            }
            catch (Exception exception)
            {
                Debug.WriteLine(exception.Message);
            }

            return destinationDoc;
        }

        private string mapExternPathToInternPathWithIndex(string source, string destination)
        {
            string destinationPathWithIndex = "";
            // load the xpath from source node
            // x[1]\y[2]\f[1]
            // X[1]\XType[2]\Y[1]\yType[4]\F[1]\FType[2]\

            string[] sourceSplitWidthIndex = source.Split('/');

            // f[1]\y[2]\x[1]
            Array.Reverse(sourceSplitWidthIndex);

            string[] destinationSplit = destination.Split('/');

            // XFType\F\yType\Y\XType\x
            Array.Reverse(destinationSplit);
            int j = 0;
            for (int i = 0; i < sourceSplitWidthIndex.Length; i++)
            {
                string tmp = sourceSplitWidthIndex[i];

                if (tmp.Contains("["))
                {
                    string tmpIndex = tmp.Split('[')[1];
                    string index = tmpIndex.Remove(tmpIndex.IndexOf(']'));

                    //set to destination array

                    //set j
                    if (i == 0) j = 0;
                    else if (i == 1) j = i + 1;
                    else j = i * 2;

                    if (destinationSplit.Length > j + 1)
                    {
                        string destinationTemp = destinationSplit[j];
                        destinationSplit[j] = destinationTemp + "[" + index + "]";
                        //set parent
                        string destinationTempParent = destinationSplit[j + 1];
                        destinationSplit[j + 1] = destinationTempParent + "[" + 1 + "]";
                    }
                }
            }

            Array.Reverse(destinationSplit);

            // XFType[2]\F[1]\yType[4]\Y[1]\XType[2]\x[1]
            return String.Join("/", destinationSplit); ;
        }

        private string mapInternPathToExternPathWithIndex(string source, string destination)
        {
            //SOURCE
            // X[1]\XType[2]\Y[1]\yType[4]\F[1]\FType[2]\
            //DESTINATION
            // x[1]\y[2]\f[1]

            string[] sourceSplitWidthIndex = source.Split('/');

            // XFType[2]\F[1]\yType[4]\Y[1]\XType[2]\x[1]
            Array.Reverse(sourceSplitWidthIndex);

            string[] destinationSplit = destination.Split('/');

            // XFType\F\yType\Y\XType\x
            Array.Reverse(destinationSplit);
            int j = 0;
            for (int i = 0; i < sourceSplitWidthIndex.Length; i = i + 2)
            {
                string tmp = sourceSplitWidthIndex[i];

                if (tmp.Contains("["))
                {
                    string tmpIndex = tmp.Split('[')[1];
                    string index = tmpIndex.Remove(tmpIndex.IndexOf(']'));

                    //set to destination array

                    //set j
                    if (i == 0) j = 0;
                    else if (i == 1) j = i + 1;
                    else j = i / 2;

                    if (destinationSplit.Length >= j)
                    {
                        string destinationTemp = destinationSplit[j];
                        destinationSplit[j] = destinationTemp + "[" + index + "]";
                        ////set parent
                        //string destinationTempParent = destinationSplit[j + 1];
                        //destinationSplit[j + 1] = destinationTempParent + "[" + 1 + "]";
                    }
                }
            }

            Array.Reverse(destinationSplit);

            // f[1]\y[2]\x[1]
            return String.Join("/", destinationSplit); ;
        }

        // add content from nodes from source to destination
        //private XmlDocument mapNodeOld(XmlDocument destinationDoc, XmlNode destinationParentNode, XmlNode sourceNode)
        //{
        //    // load the xpath from source node
        //    // x[1]\y[2]\f[1]
        //    string sourceXPath = XmlUtility.GetDirectXPathToNode(sourceNode);
        //    // x\y\f
        //    string sourceXMppingFilePath = XmlUtility.GetXPathToNode(sourceNode);

        //    //load route from mappingFile of the source node
        //    XmlMappingRoute route = xmlMapper.GetRoute(sourceXMppingFilePath);

        //    XmlNode destinationPNode = destinationParentNode;

        //    // check if this source xpath is mapped in the xmlMapper
        //    if (xmlMapper.SourceExist(sourceXMppingFilePath))
        //    {
        //        //check if there is text inside the source node
        //        if (!sourceNode.InnerText.Equals("") || addAlsoEmptyNode)
        //        {
        //            // get name of the destination node
        //            string destinationTagName = route.GetDestinationTagNames();

        //            // get xpath of the destination node
        //            // X\Y\F
        //            string destinationXMppingFilePath = route.GetDestinationXPath();
        //            // X[1]\XType[2]\Y[1]\yType[4]\F[1]\yType[2]
        //            string destinationXPath; //XmlUtility.GetDirectXPathToNode(sourceNode);

        //            // create xmlnode in document
        //            XmlNode destinationNode = XmlUtility.CreateNode(destinationTagName, destinationDoc);

        //            //if (type == element), get content
        //            if (xmlMapper.IsSourceElement(sourceXPath))
        //            {
        //                destinationNode.InnerText = sourceNode.InnerText;
        //            }

        //            //check if nodes in destination not exist, create them
        //            string destinationParentXPath = route.GetDestinationParentXPath();
        //            string currentParentXPath = XmlUtility.GetDirectXPathToNode(destinationParentNode);
        //            if (currentParentXPath != destinationParentXPath)
        //            {
        //                destinationParentNode = createMissingNodes(destinationParentXPath, currentParentXPath, destinationParentNode, destinationDoc);
        //            }

        //            //if destinationNode exist , it must be e sequence
        //            if (XmlUtility.ExistAsChild(destinationParentNode, destinationNode))
        //            {
        //                #region child exist
        //                    //get parent sequence or sequence below
        //                    XmlNode parent = destinationParentNode;
        //                    XmlNode current = destinationNode;

        //                    List<string> temp = new List<string>();

        //                    while (!current.Name.Equals(route.Destination.ParentSequence))
        //                    {
        //                        temp.Add(current.LocalName);
        //                        current = parent;
        //                        parent = parent.ParentNode;
        //                    }

        //                    // is current a sequence?
        //                    if (current.Name.Equals(route.Destination.ParentSequence))
        //                    {
        //                        XmlNode newCurrent = XmlUtility.CreateNode(current.LocalName, destinationDoc);

        //                        //destinatenode exits in every child of the parent?
        //                        if(parent.ChildNodes.Count>0)
        //                        {
        //                            bool foundNodeWithoutDestionationNode = false;

        //                            foreach (XmlNode child in parent.ChildNodes)
        //                            {
        //                                if (route.Destination.XPath.Contains(child.Name))
        //                                {
        //                                    //if destination node not exits
        //                                    if (XmlUtility.GetXmlNodeByName(child, destinationTagName) == null)
        //                                    {
        //                                        newCurrent = child;
        //                                        foundNodeWithoutDestionationNode = true;
        //                                    }
        //                                    else
        //                                    {
        //                                        if(sourceNode.Attributes.Count>0 )
        //                                        {
        //                                            if (sourceNode.Attributes["number"] != null)
        //                                            {
        //                                                int number = Convert.ToInt32(sourceNode.Attributes["number"].Value);
        //                                                if (number > 1)
        //                                                {
        //                                                    foundNodeWithoutDestionationNode = true;
        //                                                }

        //                                            }
        //                                        }
        //                                    }

        //                                }
        //                            }

        //                            if ((temp.Count>=2 && foundNodeWithoutDestionationNode) || !foundNodeWithoutDestionationNode)
        //                            {
        //                                addChild(parent,newCurrent);
        //                            }
        //                        }
        //                        else
        //                        {
        //                            addChild(parent, newCurrent);
        //                        }

        //                        temp.Reverse();

        //                        if (temp.Count > 0)
        //                        {
        //                            parent = newCurrent;
        //                            //add child ToString parent which is a sequence
        //                            foreach (string newName in temp)
        //                            {
        //                                if (!temp.Last().Equals(newName))
        //                                {
        //                                    XmlNode newNode = XmlUtility.GetXmlNodeByName(parent, newName);

        //                                    if (newNode == null)
        //                                    {
        //                                        newNode = XmlUtility.CreateNode(newName, destinationDoc);
        //                                        addChild(parent, newNode);
        //                                    }

        //                                    parent = newNode;
        //                                }
        //                                else
        //                                {
        //                                    addChild(parent, destinationNode);
        //                                }
        //                            }
        //                        }
        //                        else
        //                        {
        //                            newCurrent.InnerText = destinationNode.InnerText;
        //                            addChild(parent, destinationNode);
        //                        }

        //                        destinationPNode = parent;
        //                    }
        //                    else
        //                        destinationPNode = current;

        //                #endregion
        //            }
        //            else
        //            {
        //                addChild(destinationParentNode,destinationNode);
        //                destinationPNode = destinationNode;
        //            }

        //        }
        //    }

        //    if (sourceNode.HasChildNodes)
        //    {
        //        foreach (XmlNode childNode in sourceNode.ChildNodes)
        //        {
        //           destinationDoc = mapNode(destinationDoc, destinationPNode, childNode);
        //        }
        //    }

        //    return destinationDoc;
        //}

        // add required attributes
        private XmlDocument addAttributes(XmlDocument doc, XmlNode parentNode)
        {
            if (parentNode == null) return doc;
            if (doc == null) new ArgumentNullException("can not add attributes because xml document is null");

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
                string attrName = "";
                attrName = String.IsNullOrEmpty(attribute.Name) ? "" : attribute.Name;

                if (String.IsNullOrEmpty(attribute.Name))
                {
                    attrName = String.IsNullOrEmpty(attribute.QualifiedName.Name) ? attrName : attribute.QualifiedName.Name;
                }

                if (!String.IsNullOrEmpty(attrName))//&& attribute.DefaultValue!=null)
                {
                    XmlAttribute attr = doc.CreateAttribute(attrName);
                    if (attrName.Equals("language"))
                    {
                        attr.InnerXml = "en";
                    }
                    else
                    {
                        if (attribute.DefaultValue != null)
                        {
                            attr.InnerXml = attribute.DefaultValue;
                        }
                        else
                        {
                            XmlSchemaAttribute tmp =
                                xmlSchemaManager.Attributes.Where(a => a.Name.Equals(attrName)).FirstOrDefault();

                            if (tmp != null)
                                attr.InnerXml = tmp.DefaultValue;
                        }
                    }

                    node.Attributes.Append(attr);
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

            List<string> temp = dif.Split('/').ToList();
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

        private string getStorePath(long datasetVersionId, string exportTo)
        {
            using (DatasetManager datasetManager = new DatasetManager())
            using (MetadataStructureManager metadataStructureManager = new MetadataStructureManager())
            {
                DatasetVersion datasetVersion = datasetManager.GetDatasetVersion(datasetVersionId);

                Dataset dataset = datasetManager.GetDataset(datasetVersionId);

                string md_title = metadataStructureManager.Repo.Get(datasetVersion.Dataset.MetadataStructure.Id).Name;

                string path;

                int versionNr = datasetManager.GetDatasetVersionNr(datasetVersion);

                if (string.IsNullOrEmpty(exportTo) || exportTo.ToLower().Equals("generic"))
                    path = IOHelper.GetDynamicStorePath(datasetVersion.Dataset.Id, versionNr, "metadata", ".xml");
                else
                    path = IOHelper.GetDynamicStorePath(datasetVersion.Dataset.Id, versionNr, "metadata_" + exportTo, ".xml");

                return path;
            }
        }
    }
}