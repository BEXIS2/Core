using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

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
    }
}
