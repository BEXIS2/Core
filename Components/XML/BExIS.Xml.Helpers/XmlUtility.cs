using BExIS.Xml.Models.Mapping;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;
using System.Xml.XPath;

namespace BExIS.Xml.Helpers
{
    public class XmlUtility
    {
        public static string GetXPathToNode(XmlNode node)
        {
            string xPath = "";
            if (node == null) return string.Empty;

            if (node.NodeType == System.Xml.XmlNodeType.Attribute)
            {
                XmlAttribute attr = node as XmlAttribute;
                if (attr.OwnerElement == null)
                    return "";
                xPath = GetXPathToNode(attr.OwnerElement);
                return xPath + "/@" + attr.Name;
            }

            if (node.NodeType == System.Xml.XmlNodeType.Element)
            {
                if (node.ParentNode == null && node.NodeType.Equals(System.Xml.XmlNodeType.Document))
                    return string.Empty;

                xPath = GetXPathToNode(node.ParentNode);
                if (xPath == "")
                    return node.LocalName;
                else
                    return xPath + "/" + node.LocalName;
            }

            return string.Empty;
        }

        public static string GetDirectXPathToNode(XmlNode node)
        {
            if (node == null) return string.Empty;

            if (node.NodeType == System.Xml.XmlNodeType.Attribute)
            {
                XmlAttribute attr = node as XmlAttribute;
                if (attr.OwnerElement == null)
                    return "";
                string xPath = GetDirectXPathToNode(attr.OwnerElement);
                return xPath + "/@" + attr.Name;
            }

            if (node.NodeType == System.Xml.XmlNodeType.Element)
            {
                if (node.ParentNode == null && node.NodeType.Equals(System.Xml.XmlNodeType.Document))
                    return "";

                string xPath = GetDirectXPathToNode(node.ParentNode);
                if (xPath == "")
                    return node.LocalName;
                else
                {
                    int index = 1;
                    string nodeName = node.Name;
                    List<XmlNode> tmpChilds = new List<XmlNode>();

                    // finde same childs
                    if (node.ParentNode != null && node.ParentNode.ChildNodes.Count > 1)
                    {
                        for (int i = 0; i < node.ParentNode.ChildNodes.Count; i++)
                        {
                            XmlNode tmpNode = node.ParentNode.ChildNodes[i];
                            if (tmpNode.Name.Equals(node.Name))
                                tmpChilds.Add(tmpNode);
                        }
                    }

                    if (tmpChilds.Count > 0)
                    {
                        index = tmpChilds.IndexOf(node) + 1;
                    }

                    return xPath + "/" + node.LocalName + "[" + index + "]";
                }
            }

            return string.Empty;
        }

        /// <summary>
        /// return true if the childnode is existing
        /// in the parent node
        /// </summary>
        /// <param name="parentNode"></param>
        /// <param name="childNode"></param>
        /// <returns></returns>
        public static bool ExistAsChild(XmlNode parentNode, XmlNode childNode)
        {
            if (parentNode == null) return false;
            if (childNode == null) return false;

            if (parentNode.HasChildNodes)
            {
                foreach (XmlNode child in parentNode.ChildNodes)
                {
                    if (child.LocalName.Equals(childNode.LocalName))
                        return true;
                }

                return false;
            }
            else return false;
        }

       

        public static XmlNode GetXmlNodeByName(XmlNode parentNode, string name, bool recursiv = true)
        {
            if (parentNode == null || string.IsNullOrWhiteSpace(name)) return null;

            return getXmlNodeByName(parentNode, name, recursiv);
        }

        private static XmlNode getXmlNodeByName(XmlNode node, string name, bool recursiv = true)
        {
            if (node.LocalName.Equals(name))
                return node;
            else
            {
                if (node.HasChildNodes)
                {
                    foreach (XmlNode child in node.ChildNodes)
                    {
                        if (recursiv)
                        {
                            var tmp = getXmlNodeByName(child, name);

                            if (tmp != null)
                                return tmp;
                        }
                        else
                        {
                            if (child.LocalName.Equals(name))
                                return node;
                        }
                    }
                }
            }

            return null;
        }

        public static IEnumerable<XmlNode> FindChildrenRecursive(XmlNode parent, string name)
        {
            // Check if the current node has children.
            if (parent.HasChildNodes)
            {
                // Iterate through all the children of the current node.
                foreach (XmlNode child in parent.ChildNodes)
                {
                    // If the child's name matches the target name, yield it.
                    if (child is XmlElement && child.Name == name)
                    {
                        yield return child;
                    }

                    // Recursively call the method on the current child and yield its results.
                    foreach (XmlNode foundChild in FindChildrenRecursive(child, name))
                    {
                        yield return foundChild;
                    }
                }
            }
        }

        public static XmlNode GetXmlNodeByAttribute(XmlNode parentNode, string name, string attrName, string attrValue)
        {
            if (string.IsNullOrEmpty(name) ||
                string.IsNullOrEmpty(attrName) ||
                string.IsNullOrEmpty(attrValue) ||
                parentNode == null) return null;

            return getXmlNodeByAttribute(parentNode, name, attrName, attrValue);
        }

        private static XmlNode getXmlNodeByAttribute(XmlNode node, string name, string attrName, string attrValue)
        {
            if (node.LocalName.Equals(name) &&
                node.Attributes.Count > 0 &&
                node.Attributes[attrName] != null &&
                node.Attributes[attrName].Value.Equals(attrValue))
            {
                return node;
            }
            else
            {
                if (node.HasChildNodes)
                {
                    foreach (XmlNode child in node.ChildNodes)
                    {
                        XmlNode tmp = getXmlNodeByAttribute(child, name, attrName, attrValue);
                        if (tmp != null) return tmp;
                    }
                }
            }

            return null;
        }

        public static XmlNode GetXmlNodeByAttribute(string path, XmlDocument metadata)
        {
            var n = metadata.SelectSingleNode(path);
            return n;
        }

        public static XmlNode CreateNode(string nodeName, XmlDocument doc)
        {
            if (string.IsNullOrWhiteSpace(nodeName) || doc == null) return null;

            //doc.DocumentElement.NamespaceURI
            return doc.CreateElement(nodeName);
        }

        public static XmlNode GenerateNodeFromXPath(XmlDocument doc, XmlNode parent, string xpath)
        {
            try
            {
                if (doc == null) return null;
                //if (parent == null) return null;

                // grab the next node name in the xpath; or return parent if empty
                string[] partsOfXPath = xpath.Trim('/').Split('/');

                if (partsOfXPath.Length == 0)
                    return parent;

                string nextNodeInXPath = partsOfXPath[0];
                if (string.IsNullOrEmpty(nextNodeInXPath))
                    return parent;

                // get or create the node from the name

                int index = 1;
                string nodeName = nextNodeInXPath;
                if (nextNodeInXPath.Contains("["))
                {
                    string[] tmp = nextNodeInXPath.Split('[');
                    nodeName = tmp[0];
                    index = Int32.Parse(tmp[1].Remove(tmp[1].IndexOf("]")));
                }

                XmlNodeList nodes = null;
  
                if(parent == null || parent.Name.ToLowerInvariant().Equals(nodeName))
                    // if parent is null, we are creating the root node
                    nodes = doc.SelectNodes(nodeName);
                else
                    nodes = parent.SelectNodes(nodeName);

                XmlNode node = nodes[index - 1];

                if (node == null)
                {
                    if (nextNodeInXPath.StartsWith("@"))
                    {
                        XmlAttribute anode = doc.CreateAttribute(nextNodeInXPath.Substring(1));
                        node = parent.Attributes.Append(anode);
                    }
                    else
                    {
                        int add = index;
                        if(nodes != null && index > nodes.Count) add = index - nodes.Count;

                        for (int i = 0; i < add; i++)
                        {
                            if (parent != null) node = parent.AppendChild(doc.CreateElement(nodeName));
                            else if (doc != null && doc.DocumentElement == null)
                            {
                                node = doc.AppendChild(doc.CreateElement(nodeName));
                            }
                            else return null;
                        }
                    }
                }
            

                // rejoin the remainder of the array as an xpath expression and recurse
                string rest = String.Join("/", partsOfXPath, 1, partsOfXPath.Length - 1);
                return GenerateNodeFromXPath(doc, node, rest);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Generate all xml elements from a xpath if  there are not exsiting
        /// with the xsd elements list you can get the prefix and the
        /// namespace the a xml node when you need to create it
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="parent"></param>
        /// <param name="xpath"></param>
        /// <param name="Elements"></param>
        /// <returns></returns>
        public static XmlNode GenerateNodeFromXPath(XmlDocument doc, XmlNode parent, string xpath, List<XmlSchemaElement> XsdElements, XmlNamespaceManager nsManager)
        {
            Debug.WriteLine("-------------------------------");
            Debug.WriteLine(xpath);

            // grab the next node name in the xpath; or return parent if empty
            string[] partsOfXPath = xpath.Trim('/').Split('/');

            if (partsOfXPath.Length == 0)
                return parent;

            string nextNodeInXPath = partsOfXPath[0];
            if (string.IsNullOrEmpty(nextNodeInXPath))
                return parent;

            // get or create the node from the name

            int index = 1;
            string nodeName = nextNodeInXPath;
            if (nextNodeInXPath.Contains("["))
            {
                string[] tmp = nextNodeInXPath.Split('[');
                nodeName = tmp[0];
                index = Int32.Parse(tmp[1].Remove(tmp[1].IndexOf("]")));
            }

            XmlSchemaElement xsdelement = XsdElements?.FirstOrDefault(x => x.Name.Equals(nodeName));

            string name = nodeName;
            string prefix = "";
            string nameSpace = "";
            XmlNodeList nodes = null;

            // if xml element is defined by a xsd element
            if (xsdelement != null && nsManager != null)
            {
                nameSpace = xsdelement.QualifiedName.Namespace;
                prefix = nsManager.LookupPrefix(nameSpace);
                name = xsdelement.QualifiedName.Name;

                Debug.WriteLine(nameSpace);
                Debug.WriteLine(prefix);
                Debug.WriteLine(name);

                string searchName = String.IsNullOrEmpty(prefix) ? name : prefix + ":" + name;

                Debug.WriteLine(searchName);

                if (!String.IsNullOrEmpty(nameSpace) && !String.IsNullOrEmpty(prefix))
                    nodes = parent.SelectNodes(searchName, nsManager);
                else
                    nodes = parent.SelectNodes(nodeName);
            }
            else
            {
                nodes = parent.SelectNodes(nodeName);
            }

            XmlNode node = nodes?[index - 1];

            if (node == null)
            {
                if (nextNodeInXPath.StartsWith("@"))
                {
                    XmlAttribute anode = doc.CreateAttribute(nextNodeInXPath.Substring(1));
                    node = parent.Attributes.Append(anode);
                }
                else
                {
                    //if xsdelement exist and namespace and prefix if exist
                    if (xsdelement != null && !String.IsNullOrEmpty(nameSpace) && !string.IsNullOrEmpty(prefix))
                    {
                        node = parent.AppendChild(doc.CreateElement(prefix, name, nameSpace));
                    }
                    else
                        node = parent.AppendChild(doc.CreateElement(nodeName));
                }
            }

            // rejoin the remainder of the array as an xpath expression and recurse
            string rest = String.Join("/", partsOfXPath, 1, partsOfXPath.Length - 1);
            return GenerateNodeFromXPath(doc, node, rest, XsdElements, nsManager);
        }

        /// <summary>
        /// Add Attribute and Value to XmlNode
        /// return XmlNode
        /// </summary>
        /// <param name="node"></param>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static XmlNode AddAttribute(XmlNode node, string name, string value, XmlDocument xmlDoc)
        {
            if (node == null || xmlDoc == null || string.IsNullOrWhiteSpace(name)) return null;

            XmlAttribute attr = xmlDoc.CreateAttribute(name);
            attr.Value = value;
            node.Attributes.Append(attr);

            return node;
        }

        public static string FindXPath(XmlNode node)
        {
            StringBuilder builder = new StringBuilder();
            while (node != null)
            {
                switch (node.NodeType)
                {
                    case System.Xml.XmlNodeType.Attribute:
                        builder.Insert(0, "/@" + node.Name);
                        node = ((XmlAttribute)node).OwnerElement;
                        break;

                    case System.Xml.XmlNodeType.Element:
                        int index = FindElementIndex((XmlElement)node);
                        builder.Insert(0, "/" + node.Name + "[" + index + "]");
                        node = node.ParentNode;
                        break;

                    case System.Xml.XmlNodeType.Document:
                        return builder.ToString();

                    default:
                        throw new ArgumentException("Only elements and attributes are supported");
                }
            }
            throw new ArgumentException("Node was not in a document");
        }

        private static int FindElementIndex(XmlElement element)
        {
            XmlNode parentNode = element.ParentNode;
            if (parentNode is XmlDocument)
            {
                return 1;
            }
            XmlElement parent = (XmlElement)parentNode;
            int index = 1;
            foreach (XmlNode candidate in parent.ChildNodes)
            {
                if (candidate is XmlElement && candidate.Name == element.Name)
                {
                    if (candidate == element)
                    {
                        return index;
                    }
                    index++;
                }
            }
            throw new ArgumentException("Couldn't find element within parent");
        }

        public static XmlNode FindNodeByLabel(XmlNodeList nodeList, string labelName)
        {
            if (nodeList == null || string.IsNullOrEmpty(labelName))
            {
                return null;
            }

            foreach (XmlNode node in nodeList)
            {
                if (node.Name == labelName)
                {
                    return node;
                }
            }

            return null;
        }

        #region xdoc

        public static bool HasChildren(XElement element)
        {
            if (element.Nodes().OfType<XElement>().Count() > 0)
                return true;

            return false;
        }

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param name="name"></param>
        /// <param name="source"></param>
        /// <returns></returns>
        public static IEnumerable<XElement> GetChildren(XElement source)
        {
            return source != null ? source.Nodes().OfType<XElement>() : null;
        }

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param name="name"></param>
        /// <returns></returns>
        public static IEnumerable<XElement> GetAllChildren(XElement element)
        {
            return element != null ? element.Descendants() : null;
        }

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param name="name"></param>
        /// <returns></returns>
        public static IEnumerable<XElement> GetXElementByNodeName(string nodeName, XDocument xDoc)
        {
            return xDoc != null && !string.IsNullOrWhiteSpace(nodeName) ? xDoc.Root.Descendants(nodeName) : null;
        }

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param name="name"></param>
        /// <returns></returns>
        public static XElement GetXElementByAttribute(string nodeName, string attrName, string value, XDocument xDoc)
        {
            if (string.IsNullOrWhiteSpace(nodeName)) return null;
            if (string.IsNullOrWhiteSpace(attrName)) return null;
            if (string.IsNullOrWhiteSpace(value)) return null;
            if (xDoc == null) return null;

            string name = nodeName.Replace(" ", "");
            return xDoc.Root.Descendants(name).FirstOrDefault(p => p.Attribute(attrName) != null && p.Attribute(attrName).Value.Equals(value));
        }

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param name="name"></param>
        /// <returns></returns>
        public static IEnumerable<XElement> GetXElementsByAttribute(string nodeName, string attrName, string value, XDocument xDoc)
        {
            if (string.IsNullOrWhiteSpace(nodeName)) return null;
            if (string.IsNullOrWhiteSpace(attrName)) return null;
            if (string.IsNullOrWhiteSpace(value)) return null;
            if (xDoc == null) return null;

            string name = nodeName.Replace(" ", "");

            return xDoc.Root.Descendants(name).Where(p => p.Attribute(attrName) != null && p.Attribute(attrName).Value.Equals(value));
        }

        public static IEnumerable<XElement> GetXElementsByAttribute(string attrName, XDocument xDoc)
        {
            if (string.IsNullOrWhiteSpace(attrName)) return null;
            if (xDoc == null) return null;

            return xDoc.Root.Descendants().Where(p => p.Attribute(attrName) != null);
        }

        public static IEnumerable<XElement> GetXElementsByAttribute(string attrName, string value, XDocument xDoc)
        {
            if (string.IsNullOrWhiteSpace(attrName)) return null;
            if (string.IsNullOrWhiteSpace(value)) return null;
            if (xDoc == null) return null;

            return xDoc.Root.Descendants().Where(p => p.Attribute(attrName) != null && p.Attribute(attrName).Value.Equals(value));
        }

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param name="name"></param>
        /// <returns></returns>
        public static XElement GetXElementByAttribute(string nodeName, Dictionary<string, string> AttrValueDic, XElement XElement)
        {
            if (string.IsNullOrWhiteSpace(nodeName) || AttrValueDic == null || XElement == null) return null;

            string name = nodeName.Replace(" ", "");
            IEnumerable<XElement> elements = new List<XElement>();

            foreach (KeyValuePair<string, string> keyValuePair in AttrValueDic)
            {
                IEnumerable<XElement> newElements = GetXElementsByAttribute(nodeName, keyValuePair.Key, keyValuePair.Value, XElement);

                if (elements.Count() > 0)
                    elements = elements.Intersect(newElements);
                else
                    elements = newElements;
            }

            return elements.FirstOrDefault();
        }

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param name="name"></param>
        /// <returns></returns>
        public static IEnumerable<XElement> GetXElementsByAttribute(string nodeName, Dictionary<string, string> AttrValueDic, XElement XElement)
        {
            if (string.IsNullOrWhiteSpace(nodeName) || AttrValueDic == null || XElement == null) return null;

            string name = nodeName.Replace(" ", "");
            IEnumerable<XElement> elements = new List<XElement>();

            foreach (KeyValuePair<string, string> keyValuePair in AttrValueDic)
            {
                IEnumerable<XElement> newElements = GetXElementsByAttribute(nodeName, keyValuePair.Key, keyValuePair.Value, XElement);

                //return a empty list if one of the attr return 0 elements
                if (newElements.Count() == 0) return newElements;

                if (elements.Count() > 0)
                    elements = elements.Intersect(newElements);
                else
                    elements = newElements;
            }

            return elements;
        }

        public static IEnumerable<XElement> GetXElementsByAttribute(string attrName, string value, XElement parent)
        {
            return parent.Descendants().Where(p => p.Attribute(attrName) != null && p.Attribute(attrName).Value.Equals(value));
        }

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param name="name"></param>
        /// <returns></returns>
        public static IEnumerable<XElement> GetXElementsByAttribute(string nodeName, string attrName, string value, XElement parent)
        {
            string name = nodeName.Replace(" ", "");
            return parent.Descendants(name).Where(p => p.Attribute(attrName) != null && p.Attribute(attrName).Value.Equals(value));
        }

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param name="name"></param>
        /// <returns></returns>
        public static XElement GetXElementByAttribute(string nodeName, string attrName, string value, XElement Parent)
        {
            string name = nodeName.Replace(" ", "");
            return Parent.Descendants(name).FirstOrDefault(p => p.Attribute(attrName) != null && p.Attribute(attrName).Value.Equals(value));
        }

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param name="name"></param>
        /// <returns></returns>
        public static IEnumerable<XElement> GetXElementsByAttribute(string nodeName, Dictionary<string, string> AttrValueDic, XDocument xDoc)
        {
            string name = nodeName.Replace(" ", "");
            IEnumerable<XElement> elements = new List<XElement>();

            foreach (KeyValuePair<string, string> keyValuePair in AttrValueDic)
            {
                IEnumerable<XElement> newElements = GetXElementsByAttribute(nodeName, keyValuePair.Key, keyValuePair.Value, xDoc);

                //return a empty list if one of the attr return 0 elements
                if (newElements == null || newElements.Count() == 0) return newElements;

                if (elements.Count() > 0)
                    elements = elements.Intersect(newElements);
                else
                    elements = newElements;
            }

            return elements;
        }

        public static IEnumerable<XElement> GetXElementsByAttribute(Dictionary<string, string> AttrValueDic, XDocument xDoc)
        {
            string name = "";
            IEnumerable<XElement> elements = new List<XElement>();

            foreach (KeyValuePair<string, string> keyValuePair in AttrValueDic)
            {
                IEnumerable<XElement> newElements = GetXElementsByAttribute(keyValuePair.Key, keyValuePair.Value, xDoc);

                //return a empty list if one of the attr return 0 elements
                if (newElements.Count() == 0) return newElements;

                if (elements.Count() > 0)
                {
                    elements = elements.Intersect(newElements);
                    if (elements == null || elements.Count() == 0)
                        return elements;
                }
                else
                    elements = newElements;
            }

            return elements;
        }

        public static IEnumerable<XElement> GetXElementsByAttribute(Dictionary<string, string> AttrValueDic, XElement parent)
        {
            string name = "";
            IEnumerable<XElement> elements = new List<XElement>();

            foreach (KeyValuePair<string, string> keyValuePair in AttrValueDic)
            {
                IEnumerable<XElement> newElements = GetXElementsByAttribute(keyValuePair.Key, keyValuePair.Value, parent);

                //return a empty list if one of the attr return 0 elements
                if (newElements.Count() == 0) return newElements;

                if (elements.Count() > 0)
                {
                    elements = elements.Intersect(newElements);
                    if (elements == null || elements.Count() == 0)
                        return elements;
                }
                else
                    elements = newElements;
            }

            return elements;
        }

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param name="name"></param>
        /// <returns></returns>
        public static IEnumerable<XElement> GetXElementsByAttribute(string nodeName, Dictionary<string, string> AttrValueDic, XDocument xDoc, string parentXPath)
        {
            string name = nodeName.Replace(" ", "");
            IEnumerable<XElement> elements = new List<XElement>();
            XElement parent = GetXElementByXPath(parentXPath, xDoc);

            foreach (KeyValuePair<string, string> keyValuePair in AttrValueDic)
            {
                IEnumerable<XElement> newElements = GetXElementsByAttribute(nodeName, keyValuePair.Key, keyValuePair.Value, parent);

                //return a empty list if one of the attr return 0 elements
                if (newElements.Count() == 0) return newElements;

                if (elements.Count() > 0)
                    elements = elements.Intersect(newElements);
                else
                    elements = newElements;
            }

            return elements;
        }

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param name="name"></param>
        /// <returns></returns>
        public static XElement GetXElementByXPath(string xpath, XDocument xDoc)
        {
            return xDoc.XPathSelectElement(xpath.Replace(" ", string.Empty));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param name="name"></param>
        /// <returns></returns>
        public static XAttribute GetXAttributeByXPath(string xpath, XDocument xDoc)
        {
            var tmp =  (IEnumerable<object>)xDoc.XPathEvaluate(xpath.Replace(" ", string.Empty));
            var x = tmp.FirstOrDefault();
            return x as XAttribute;
        }

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param name="xDocument"></param>
        /// <returns></returns>
        public static XmlDocument ToXmlDocument(XDocument xDocument)
        {
            var xmlDocument = new XmlDocument();
            using (var xmlReader = xDocument.CreateReader())
            {
                xmlDocument.Load(xmlReader);
            }
            return xmlDocument;
        }

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param name="xmlDocument"></param>
        /// <returns></returns>
        public static XDocument ToXDocument(XmlDocument xmlDocument)
        {
            if (xmlDocument != null)
            {
                using (var nodeReader = new XmlNodeReader(xmlDocument))
                {
                    nodeReader.MoveToContent();
                    return XDocument.Load(nodeReader);
                }
            }

            return null;
        }

        #endregion xdoc
    }
}