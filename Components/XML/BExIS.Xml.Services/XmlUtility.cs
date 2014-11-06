using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace BExIS.Xml.Services
{
    public class XmlUtility
    {
        public static string GetXPathToNode(XmlNode node)
        {
            if (node.ParentNode == null && node.NodeType.Equals(System.Xml.XmlNodeType.Document))
                return "";

            string xPath = GetXPathToNode(node.ParentNode);
            if (xPath == "")
                return node.LocalName;
            else
                return xPath + "/" + node.LocalName;
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

        public static XmlNode GetXmlNodeByName(XmlNode parentNode, string name)
        {
            return getXmlNodeByName(parentNode, name);
        }

        private static XmlNode getXmlNodeByName(XmlNode node, string name)
        {
            if (node.LocalName.Equals(name))
                return node;
            else
            {
                if (node.HasChildNodes)
                {

                    foreach (XmlNode child in node.ChildNodes)
                    {
                        if (getXmlNodeByName(child, name) != null)
                            return child;
                    }


                }
            }

            return null;
        }

        public static XmlNode CreateNode(string nodeName, XmlDocument doc)
        {
            //doc.DocumentElement.NamespaceURI
            return doc.CreateElement(nodeName);
        }

        #region xdoc

            /// <summary>
            /// 
            /// </summary>
            /// <remarks></remarks>
            /// <seealso cref=""/>
            /// <param name="name"></param>
            /// <returns></returns>
            public static IEnumerable<XElement> GetXElementByNodeName(string nodeName, XDocument xDoc)
            {
                return xDoc.Root.Descendants(nodeName);
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
                return xDoc.Root.Descendants(nodeName).Where(p => p.Attribute(attrName).Value.Equals(value)).FirstOrDefault();
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
                using (var nodeReader = new XmlNodeReader(xmlDocument))
                {
                    nodeReader.MoveToContent();
                    return XDocument.Load(nodeReader);
                }
            }

        #endregion
    }
}
