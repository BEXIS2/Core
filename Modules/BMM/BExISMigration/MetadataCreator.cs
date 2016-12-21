using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.IO;
using Vaiona.Utils.Cfg;
using BExIS.Xml.Helpers.Mapping;
using System.Xml;
using System.Xml.Linq;

using IBM.Data.DB2;
using IBM.Data.DB2Types;
using System.Diagnostics;
using BExIS.Dlm.Services.MetadataStructure;
using BExIS.Dlm.Entities.MetadataStructure;
using BExIS.Pms.Entities.Publication;

namespace BExISMigration
{
    class xpathProp
    {
        public string nodeName { get; set; }
        public long nodeIndex { get; set; }
    }


    public class MetadataCreator
    {
        public XmlDocument fillInXmlAttributes(XmlDocument metadataXml, XmlDocument metadataXmlTemplate)
        {
            // add missing nodes
            //doc = manipulate(doc);

            // add the xml attributes
            handle(metadataXml, metadataXml, metadataXmlTemplate);

            return metadataXml;
        }


        public XmlDocument createMetadata(string dataSetID, string filePath, string DataBase, ref List<string> variableNames, ref string fileType)
        {
            string path_mappingFile = filePath + @"\bexis_metadata_mapping.xml";

            // query Bexis1 metadata from DB
            XmlDocument metadataBexis1 = getMetadataXml(dataSetID, DataBase);

            // Get variable names
            XmlNamespaceManager xnm = new XmlNamespaceManager(metadataBexis1.NameTable);
            xnm.AddNamespace("bgc", "http://www.bgc-jena.mpg.de");
            XmlNodeList variables = metadataBexis1.SelectNodes("bgc:metaProfile/bgc:data/bgc:dataStructure/bgc:variables/bgc:variable/bgc:name", xnm);
            foreach (XmlNode variable in variables)
            {
                variableNames.Add(variable.InnerText);
            }

            // is this data structure structured?
            XmlNode dataFileType = metadataBexis1.SelectSingleNode("bgc:metaProfile/bgc:data/bgc:fileType", xnm);
            fileType = dataFileType.InnerText;

            // XML mapper + mapping file
            XmlMapperManager xmlMapperManager = new XmlMapperManager();
            xmlMapperManager.Load(path_mappingFile, "BExIS");

            // generate Bpp metadata 
            XmlDocument metadataBpp = xmlMapperManager.Generate(metadataBexis1, 99);

            return metadataBpp;
        }


        // create xml template corresponding to bexis metadata structure
        public System.Xml.Linq.XDocument createXmlTemplate(long metadataStructureId)
        {
            BExIS.Xml.Helpers.XmlMetadataWriter xmlMetadatWriter = new BExIS.Xml.Helpers.XmlMetadataWriter(BExIS.Xml.Helpers.XmlNodeMode.xPath);
            System.Xml.Linq.XDocument metadataXml = xmlMetadatWriter.CreateMetadataXml(metadataStructureId);

            return metadataXml;
        }


        // import metadata structure
        public long importMetadataStructure(string filePath, string userName, string schemaFile = "", string schemaName = "", string titlePath = "", string descriptionPath = "")
        {
            if (string.IsNullOrEmpty(schemaFile))
                schemaFile = filePath + @"\schema_toImport.xsd";

            MetadataStructureManager metadataStructureManager = new MetadataStructureManager();
            XmlSchemaManager xmlSchemaManager = new XmlSchemaManager();
            MetadataStructure metadataStructure = new MetadataStructure();
            if (string.IsNullOrEmpty(schemaName))
                schemaName = "BExIS";
            string root = "";
            if (string.IsNullOrEmpty(titlePath))
                titlePath = "Metadata/general/general/title/title";
            if (string.IsNullOrEmpty(descriptionPath))
                descriptionPath = "Metadata/methodology/methodology/introduction/introduction";

            MetadataStructure existMetadataStructures = metadataStructureManager.Repo.Get(m => m.Name.Equals(schemaName)).FirstOrDefault();

            if (existMetadataStructures == null)
            {
                // load schema xsd
                long metadataStructureid = 0;
                xmlSchemaManager.Load(schemaFile, userName);
                try
                {
                    metadataStructureid = xmlSchemaManager.GenerateMetadataStructure(root, schemaName);
                }
                catch
                {
                    xmlSchemaManager.Delete(schemaName);
                }
                metadataStructure = metadataStructureManager.Repo.Get(metadataStructureid);
                try
                {
                    // set parameters:
                    XmlDocument xmlDoc = new XmlDocument();
                    if (metadataStructure.Extra != null)
                    {
                        xmlDoc = (XmlDocument)metadataStructure.Extra;
                    }

                    // add title Node
                    xmlDoc = AddReferenceToMetadatStructure(metadataStructure, "title", titlePath, "extra/nodeReferences/nodeRef", xmlDoc);
                    // add Description
                    xmlDoc = AddReferenceToMetadatStructure(metadataStructure, "description", descriptionPath, "extra/nodeReferences/nodeRef", xmlDoc);


                    metadataStructure.Extra = xmlDoc;
                    metadataStructureManager.Update(metadataStructure);
                }
                catch
                {
                    //
                }
            }
            else
            {
                metadataStructure = existMetadataStructures;
            }

            return metadataStructure.Id;
        }



        ///////////////////////////////////////////////////////////////////////////////////////////////
        // helper methods
        #region helper methods


        // out of BExIS.Web.Shell.Areas.DCM.Controllers.ImportMetadataStructureSetParametersController
        private XmlDocument AddReferenceToMetadatStructure(MetadataStructure metadataStructure, string nodeName, string nodePath, string destinationPath, XmlDocument xmlDoc)
        {

            XmlDocument doc = xmlDoc;
            XmlNode extra;

            if (doc.DocumentElement == null)
            {
                if (metadataStructure.Extra != null)
                {

                    extra = ((XmlDocument)metadataStructure.Extra).DocumentElement;
                }
                else
                {
                    extra = doc.CreateElement("extra", "");
                }

                doc.AppendChild(extra);
            }

            XmlNode x = createMissingNodes(destinationPath, doc.DocumentElement, doc, nodeName);

            //check attrviute of the xmlnode
            if (x.Attributes.Count > 0)
            {


                foreach (XmlAttribute attr in x.Attributes)
                {
                    if (attr.Name == "name") attr.Value = nodeName;
                    if (attr.Name == "value") attr.Value = nodePath;
                }
            }
            else
            {
                XmlAttribute name = doc.CreateAttribute("name");
                name.Value = nodeName;
                XmlAttribute value = doc.CreateAttribute("value");
                value.Value = nodePath;

                x.Attributes.Append(name);
                x.Attributes.Append(value);

            }

            return doc;

        }


        // out of BExIS.Web.Shell.Areas.DCM.Controllers.ImportMetadataStructureSetParametersController
        private XmlNode createMissingNodes(string destinationParentXPath, XmlNode parentNode, XmlDocument doc, string name)
        {
            string dif = destinationParentXPath;

            List<string> temp = dif.Split('/').ToList();
            temp.RemoveAt(0);

            XmlNode parentTemp = parentNode;

            foreach (string s in temp)
            {
                if (BExIS.Xml.Helpers.XmlUtility.GetXmlNodeByName(parentTemp, s) == null)
                {
                    XmlNode t = BExIS.Xml.Helpers.XmlUtility.CreateNode(s, doc);

                    parentTemp.AppendChild(t);
                    parentTemp = t;
                }
                else
                {
                    XmlNode t = BExIS.Xml.Helpers.XmlUtility.GetXmlNodeByName(parentTemp, s);

                    if (temp.Last().Equals(s))
                    {
                        if (!t.Attributes["name"].Equals(name))
                        {
                            t = BExIS.Xml.Helpers.XmlUtility.CreateNode(s, doc);
                            parentTemp.AppendChild(t);
                        }

                    }

                    parentTemp = t;
                }
            }

            return parentTemp;
        }


        // rekursive Funktion
        private static void handle(XmlNode root, XmlDocument doc, XmlDocument temp)
        {
            foreach (XmlNode node in root.ChildNodes)
            {
                Debug.WriteLine(node.Name);///////////////////////////////////////////////////////////////////////////
                if (node.HasChildNodes)
                {
                    string xpath = FindXPath(node); // xpath in doc
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


        static string FindXPath(XmlNode node)
        {
            StringBuilder builder = new StringBuilder();
            while (node != null)
            {
                switch (node.NodeType)
                {
                    case XmlNodeType.Attribute:
                        builder.Insert(0, "/@" + node.Name);
                        node = ((XmlAttribute)node).OwnerElement;
                        break;
                    case XmlNodeType.Element:
                        int index = FindElementIndex((XmlElement)node);
                        builder.Insert(0, "/" + node.Name + "[" + index + "]");
                        node = node.ParentNode;
                        break;
                    case XmlNodeType.Document:
                        return builder.ToString();
                    default:
                        throw new ArgumentException("Only elements and attributes are supported");
                }
            }
            throw new ArgumentException("Node was not in a document");
        }


        static int FindElementIndex(XmlElement element)
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
        public class BexisPublication
        {
            public int Id { get; set; }
            public XmlDocument MetaDataXml { get; set; }
            public List<PublicationContentDescriptor> PublicationContentDescriptors = new List<PublicationContentDescriptor>();
        }
        public BexisPublication[] getPublicationsMetadataXml(string DataBase)
        {
            var bexisPublications = new List<BexisPublication>();
            var docs = new List<XmlDocument>();
            var doc = new XmlDocument();
            string mySelectQuery = "SELECT ID,DATA  FROM \"EXPLORER\".\"PUBLICATIONLIST\"";
            DB2Connection connect = new DB2Connection(DataBase);
            DB2Command myCommand = new DB2Command(mySelectQuery, connect);
            connect.Open();
            DB2DataReader myReader = myCommand.ExecuteReader();
            while (myReader.Read())
            {
                doc.LoadXml(myReader.GetString(1));
                var bp = new BexisPublication();
                bp.Id = (int)(myReader.GetValue(0));
                bp.MetaDataXml = doc;
                var fileCommand = new DB2Command("SELECT FILENAME, MIMETYPE ,FILE  FROM \"EXPLORER\".\"PUBLICATIONFILES\" where PUBID=" + bp.Id, connect);
                var fileReader = fileCommand.ExecuteReader();
                string dataPath = AppConfiguration.DataPath;
                string storepath = Path.Combine(dataPath, "Temp", "Administrator");
                // if folder not exist
                if (!Directory.Exists(storepath))
                    Directory.CreateDirectory(storepath);
                var index = 1;
                if (fileReader.Read())
                {
                    PublicationContentDescriptor pubContent = new PublicationContentDescriptor()
                    {
                        OrderNo = index++,
                        Name = fileReader.GetString(0),
                        MimeType = fileReader.GetString(1)
                    };
                    var filePath = Path.Combine(storepath, Guid.NewGuid().ToString() + Path.GetExtension(pubContent.Name));
                    File.WriteAllBytes(filePath, ((Byte[])fileReader.GetValue(2)));
                    pubContent.URI = filePath;
                    bp.PublicationContentDescriptors.Add(pubContent);
                }
                fileReader.Close();
                bexisPublications.Add(bp);
            }
            myReader.Close();
            connect.Close();
            return bexisPublications.ToArray();
        }

        public XmlDocument getMetadataXml(string dataSetID, string DataBase)
        {
            XmlDocument doc = new XmlDocument();

            string mySelectQuery = "select datasetid, metadata";
            mySelectQuery += " from explorer.datasets where datasetid = " + dataSetID + ";";
            DB2Connection connect = new DB2Connection(DataBase);
            DB2Command myCommand = new DB2Command(mySelectQuery, connect);
            connect.Open();
            DB2DataReader myReader = myCommand.ExecuteReader();
            while (myReader.Read())
            {
                doc.LoadXml(myReader.GetString(1));
            }
            myReader.Close();
            connect.Close();

            return doc;
        }


        #endregion
    }
}
