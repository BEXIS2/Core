using BExIS.Dlm.Entities.MetadataStructure;
using BExIS.Dlm.Services.MetadataStructure;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Xml;
using System.Xml.Linq;

namespace BExIS.Xml.Helpers
{
    public class XmlMetadataImportHelper
    {
        public static string GetMappingFileName(long id, TransmissionType transmissionType, string name)
        {
            using (MetadataStructureManager msm = new MetadataStructureManager())
            {
                MetadataStructure metadataStructure = msm.Repo.Get(id);

                // get MetadataStructure
                XDocument xDoc = XmlUtility.ToXDocument((XmlDocument)metadataStructure.Extra);

                List<XElement> tmpList =
                    XmlUtility.GetXElementsByAttribute(nodeNames.convertRef.ToString(), new Dictionary<string, string>()
                    {
                    {AttributeNames.name.ToString(), name},
                    {AttributeNames.type.ToString(), transmissionType.ToString()}
                    }, xDoc).ToList();

                if (tmpList.Count >= 1)
                {
                    return tmpList.FirstOrDefault().Attribute("value").Value.ToString();
                }

                return null;
            }
        }

        #region update new xml metadata with a base template

        public static XmlDocument FillInXmlValues(XmlDocument source, XmlDocument destination)
        {
            // add missing nodes
            //doc = manipulate(doc);

            // add the xml attributes
            setValues(source, destination);

            return destination;
        }

        // rekursive Funktion
        private static void setValues(XmlNode root, XmlDocument doc)
        {
            foreach (XmlNode node in root.ChildNodes)
            {
                Debug.WriteLine(node.Name);///////////////////////////////////////////////////////////////////////////
                if (!node.HasChildNodes)
                {
                    if (node.NodeType == System.Xml.XmlNodeType.Text)
                    {
                        string xpath = XmlUtility.GetDirectXPathToNode(node.ParentNode);
                        string value = node.Value;
                        if (value != null)
                        {
                            XmlNode tmpNode = doc.SelectSingleNode(xpath);

                            if (tmpNode != null && !string.IsNullOrEmpty(value))
                            {
                                tmpNode.InnerText = value;
                            }
                        }
                    }
                    else
                    {
                        setValues(node, doc); // next level recursively
                    }
                }
                else
                {
                    setValues(node, doc); // next level recursively
                }
            }
        }

        public static XmlDocument FillInXmlAttributes(XmlDocument metadataXml, XmlDocument metadataXmlTemplate)
        {
            // add missing nodes
            //doc = manipulate(doc);

            // add the xml attributes
            handle(metadataXml, metadataXml, metadataXmlTemplate);

            return metadataXml;
        }

        // rekursive Funktion
        private static void handle(XmlNode root, XmlDocument doc, XmlDocument temp)
        {
            foreach (XmlNode node in root.ChildNodes)
            {
                Debug.WriteLine(node.Name);///////////////////////////////////////////////////////////////////////////
                if (node.HasChildNodes)
                {
                    string xpath = XmlUtility.FindXPath(node); // xpath in doc
                    long number = 1;
                    List<xpathProp> xpathDict = dismantle(xpath); // divide xpath
                    string xpathTemp = "";
                    for (int i = 1; i < xpathDict.Count; i++)
                        xpathTemp += "/" + xpathDict[i].nodeName + "[1]"; // atapt xpath to template
                    for (int j = xpathDict.Count - 1; j >= 0; j--)
                    {
                        xpathProp xp = xpathDict[j];
                        if (xp.nodeIndex > number)
                            number = xp.nodeIndex; // get node index "number"
                    }

                    XmlNode tempNode = temp.SelectSingleNode(xpathTemp); // get node from template

                    if (tempNode != null && tempNode.Attributes.Count > 0)
                    {
                        foreach (XmlAttribute a in tempNode.Attributes)
                        {
                            try // transfer all attributes from tamplate node to doc node
                            {
                                XmlAttribute b = doc.CreateAttribute(a.Name);
                                if (a.Name == "number") // handle node index "number"
                                    b.Value = number.ToString();
                                else
                                    b.Value = a.Value;
                                node.Attributes.Append(b);
                            }
                            catch
                            {
                            }
                        }
                    }

                    handle(node, doc, temp); // next level recursively
                }
            }
        }

        private static List<xpathProp> dismantle(string xpath)
        {
            String[] xpathArray = xpath.Split('/');
            List<xpathProp> xpathDict = new List<xpathProp>();
            foreach (string s in xpathArray)
            {
                xpathProp xp = new xpathProp();
                if (s.Length > 0)
                {
                    xp.nodeName = s.Substring(0, s.IndexOf('['));
                    string subs = s.Substring(s.IndexOf('[') + 1, s.IndexOf(']') - s.IndexOf('[') - 1);
                    xp.nodeIndex = long.Parse(subs);
                }
                else
                {
                    xp.nodeName = s;
                    xp.nodeIndex = 1;
                }
                xpathDict.Add(xp);
            }
            return xpathDict;
        }

        private static XmlDocument manipulate(XmlDocument doc)
        {
            XmlNode root = doc.SelectSingleNode("/Metadata");
            for (int i = 0; i < root.ChildNodes.Count; i++)
            {
                XmlNode oldNode = root.ChildNodes[i].Clone();
                XmlNode newNode = doc.CreateNode(oldNode.NodeType, oldNode.Name, oldNode.NamespaceURI);
                newNode.AppendChild(oldNode);
                root.ReplaceChild(newNode, root.ChildNodes[i]);
            }
            return doc;
        }

        #endregion update new xml metadata with a base template
    }

    internal class xpathProp
    {
        public string nodeName { get; set; }
        public long nodeIndex { get; set; }
    }
}