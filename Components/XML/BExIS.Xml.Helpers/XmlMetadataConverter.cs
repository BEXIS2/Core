using BExIS.Dlm.Entities.Common;
using BExIS.Dlm.Entities.MetadataStructure;
using BExIS.Dlm.Services.MetadataStructure;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Linq;

namespace BExIS.Xml.Helpers
{
    public class XmlMetadataConverter
    {
        private Dictionary<string, string> mappings = new Dictionary<string, string>();

        #region XML to JSON

        /// <summary>
        /// converts a metadata xml docment to a simplified json form.
        /// The cumbersome metadata structure with usage/type is combined into one entry.
        ///
        /// e.g. Name/NameType - Name
        ///
        /// there are two ways to get the json back.
        ///  1. with empty fields(good for structural representation).
        /// This json contains a version of each attribute even if the entries are empty.
        ///
        /// 2. without empty fields (good for validation)
        /// Here a json is created which only contains attributes that have entries.
        ///
        /// </summary>
        /// <param name="metadata">xml document</param>
        /// <param name="includeEmpty">default is false, include or not attributes without values</param>
        /// <returns>JObject</returns>
        /// <exception cref="ArgumentNullException"></exception>
        public JObject ConvertTo(XmlDocument metadata, bool includeEmpty = false)
        {
            var metadataJson = new JObject();

            var root = metadata.DocumentElement;

            long metadataStructureId = 0;

            // add first node metadata

            if (root.HasAttributes)
            {
                foreach (XmlAttribute attr in root.Attributes)
                {
                    JProperty p = new JProperty("@" + attr.Name, attr.Value);

                    metadataJson.Add(p);

                    // if attr is id, means id of the metadata Strutcure
                    if (attr.Name.ToLower().Equals("id"))
                    {
                        if (Int64.TryParse(attr.Value, out metadataStructureId) == false)
                        {
                            throw new ArgumentNullException("Metadata structure id not exist in the root element of the metadata xml.");
                        }
                    }
                }
            }

            if (root.HasChildNodes)
            {
                using (var metadataStructureManager = new MetadataStructureManager())
                {
                    // load metadata structure
                    var metadataStructure = metadataStructureManager.Repo.Get(metadataStructureId);

                    // check if metadat structure exist
                    if (metadataStructure == null) throw new ArgumentNullException("metadata structure with id " + metadataStructureId + " not exist");


                    foreach (var usage in metadataStructure.MetadataPackageUsages)
                    {
                        XmlNode node = XmlUtility.FindNodeByLabel(root.ChildNodes, usage.Label);

                        var packageUsageJson = _convertPackageUsage(node, usage, includeEmpty);
                        if (packageUsageJson != null)
                            metadataJson.Add(usage.Label, packageUsageJson);
                    }


                    //for (int i = 0; i < root.ChildNodes.Count; i++)
                    //{
                    //    XmlNode node = root.ChildNodes[i];
                    //    long usageId = metadataStructure.MetadataPackageUsages.ElementAt(i).Id;
                    //    var usage = metadataStructureManager.PackageUsageRepo.Get(usageId);

                    //    var packageUsageJson = _convertPackageUsage(node, usage, includeEmpty);
                    //    if (packageUsageJson != null)
                    //        metadataJson.Add(usage.Label, packageUsageJson);
                    //}
                }
            }

            return metadataJson;
        }

        private JToken _convertPackageUsage(XmlNode node, MetadataPackageUsage usage, bool includeEmpty = false)
        {
            if (node is XmlElement)
            {
                var element = (XmlElement)node;

                //var usageType = usage.MetadataPackage;
                List<BaseUsage> children = getChildren(usage);

                JArray array = new JArray();

                foreach (XmlNode tCHild in element.ChildNodes) // loop over the list of entry
                {
                    if (tCHild != null && tCHild.HasChildNodes)
                    {
                        // complex stuff
                        // add all children nodes
                        JObject complex = new JObject();
                        setReference(complex, (XmlElement)tCHild, includeEmpty);

                        for (int i = 0; i < tCHild.ChildNodes.Count; i++)
                        {
                            XmlNode child = tCHild.ChildNodes[i];
                            var childUsage = children.FirstOrDefault(c=>c.Label.Equals(child.LocalName));

                            var childJson = _convertElementUsage(child, childUsage, includeEmpty);

                            if (childJson != null)
                            {
                                if (childJson is JProperty)
                                    complex.Add(childJson);

                                if (childJson is JObject || childJson is JArray)
                                    complex.Add(child.Name, childJson);
                            }
                        }

                        if (!complex.Children().Any()) return null;

                        if (usage.MaxCardinality <= 1) return complex;
                        else
                        {
                            array.Add(complex); // add each element to a array
                        }
                    }
                }

                return array;
            }

            return null;
        }

        private bool isEmpty(XmlElement xmlElement)
        {
            

            return true;
        }

        private JToken _convertElementUsage(XmlNode node, BaseUsage usage, bool includeEmpty = false)
        {
            if (node is XmlElement)
            {
                var element = (XmlElement)node;

                JArray array = new JArray();

                #region simple

                if (isSimple(element.FirstChild))
                {
                    //property or array
                    if (usage.MaxCardinality <= 1) // property
                    {
                        XmlNode type = element.FirstChild; // has also reference
                        //JProperty p = new JProperty(usage.Label, type.InnerText);
                        if (!string.IsNullOrEmpty(type.InnerText) || includeEmpty)
                            return addSimple(usage.Label, type.InnerText, (XmlElement)type, includeEmpty);
                        else
                            return null;
                    }
                    else // array
                    {
                        foreach (XmlNode type in element.ChildNodes)
                        {
                            if (!string.IsNullOrEmpty(type.InnerText) || includeEmpty)
                                // check if
                                array.Add(addSimple(usage.Label, type.InnerText, (XmlElement)type, includeEmpty));
                        }

                        if (array.Count == 0) return null;

                        if (array.Count > 0) return array;
                    }
                }

                #endregion simple

                #region complex

                List<BaseUsage> children = getChildren(usage);

                if (children.Any())
                {
                    foreach (XmlNode tCHild in element.ChildNodes) // loop over the list of entry
                    {
                        if (tCHild != null && tCHild.HasChildNodes)
                        {
                            // complex stuff
                            // add all children nodes
                            JObject complex = new JObject();
                            setReference(complex, (XmlElement)tCHild, includeEmpty);

                            for (int i = 0; i < tCHild.ChildNodes.Count; i++)
                            {
                                XmlNode child = tCHild.ChildNodes[i];
                                //var childUsage = children[i];
                                var childUsage = children.FirstOrDefault(c => c.Label.Equals(child.LocalName));

                                var childJson = _convertElementUsage(child, childUsage, includeEmpty);

                                if (childJson != null)
                                {
                                    if (childJson is JProperty)
                                        complex.Add(childJson);

                                    if (childJson is JObject || childJson is JArray)
                                    {
                                        complex.Add(child.Name, childJson);
                                    }
                                }
                            }

                            if (!complex.Children().Any()) return null;

                            if (usage.MaxCardinality <= 1) return complex;
                            else
                            {
                                array.Add(complex); // add each element to a array
                            }
                        }
                    }
                }

                return array;

                #endregion complex
            }

            return null;
        }

        private List<BaseUsage> getChildren(BaseUsage usage)
        {
            if (usage is MetadataPackageUsage)
            {
                MetadataPackage metadataPackage = ((MetadataPackageUsage)usage).MetadataPackage;
                return metadataPackage.MetadataAttributeUsages.ToList<BaseUsage>();
            }
            else
            {
                MetadataAttribute metadataAttribute = getType(usage);

                if (metadataAttribute.Self is MetadataCompoundAttribute)
                    return ((MetadataCompoundAttribute)metadataAttribute.Self).MetadataNestedAttributeUsages.ToList<BaseUsage>();

                return new List<BaseUsage>();
            }
        }

        private MetadataAttribute getType(BaseUsage usage)
        {
            if (usage is MetadataAttributeUsage)
            {
                MetadataAttributeUsage mau = (MetadataAttributeUsage)usage;
                return mau.MetadataAttribute;
            }

            if (usage is MetadataNestedAttributeUsage)
            {
                MetadataNestedAttributeUsage mnau = (MetadataNestedAttributeUsage)usage;
                return mnau.Member;
            }

            return null;
        }

        private bool isSimple(XmlNode type)
        {
            if (type != null && type.HasChildNodes == false)
            {
                return true;
            }
            else//simple e.g. Name/NameType/#Text
            if (type != null && type.HasChildNodes && type.ChildNodes.Count == 1 && type.ChildNodes[0] is XmlText)
            {
                return true;
            }

            return false;
        }

        private JObject addSimple(string label, string value, XmlElement reference, bool includeEmpty)
        {
            JObject simple = new JObject();

            setReference(simple, reference, includeEmpty);
            simple.Add(new JProperty("#text", value));

            return simple;
        }

        private void setReference(JObject target, XmlElement element, bool includeEmpty)
        {
            if (element.HasAttributes && element.HasAttribute("ref"))
            {
                target.Add("@ref", element.Attributes["ref"].Value);
            }
            else if (includeEmpty)
            {
                target.Add("@ref", "");
            }

            if (element.HasAttributes && element.HasAttribute("partyid"))
            {
                target.Add("@partyid", element.Attributes["partyid"].Value);
            }
            else if (includeEmpty)
            {
                target.Add("@partyid", "");
            }
        }

        #endregion XML to JSON

        #region JSON to XML

        public XmlDocument ConvertTo(JObject metadataJson)
        {
            if (metadataJson == null) throw new ArgumentNullException("metadataJson", "MetadataJson must not be null.");

            // get structure id
            long id = 0;
            if (metadataJson.ContainsKey("@id"))
            {
                if (Int64.TryParse(metadataJson.Property("@id").Value.ToString(), out id))
                {
                    XmlMetadataWriter writer = new XmlMetadataWriter(XmlNodeMode.xPath);
                    XmlDocument target = XmlUtility.ToXmlDocument(writer.CreateMetadataXml(id));

                    var source = JsonConvert.DeserializeXmlNode(metadataJson.ToString(), "Metadata");

                    // generate dictionary with source path as key and target path as value
                    mappings = getXPathMapping(target);

                    /// put the incoming xml to the internal structure
                    /// BUT if there are elements with index >1 then the attributes like id,roleid are not set
                    mapNode(target, target.DocumentElement, source.DocumentElement);

                    // generate intern template metadata xml with needed attribtes
                    // also every object with index > 1 is generate with attribtes but without values
                    var xmlMetadatWriter = new XmlMetadataWriter(BExIS.Xml.Helpers.XmlNodeMode.xPath);
                    var metadataWithAttributesXml = xmlMetadatWriter.CreateMetadataXml(id, XmlUtility.ToXDocument(target));

                    // merge the metadata with attributes and the metadata with values together
                    var completeMetadata = XmlMetadataImportHelper.FillInXmlValues(target,
                        XmlMetadataWriter.ToXmlDocument(metadataWithAttributesXml));

                    // additional attributes like partyid or ref
                    // the source has attributes that are currently not set but set from the ui,
                    // find this attributes and set it in the complete metadata
                    addDynamicAttributes(target, completeMetadata);

                    return completeMetadata;
                }
                else
                {
                    throw new ArgumentException("the id property of the incoming metadata json is not a number.");
                }
            }
            else
            {
                throw new ArgumentException("incoming json has no id.");
            }

            // gerenate template xml

            // go throw the xml
            // get xpath of node
            // get json path
            // cardinality?
            // check if json object exist
            // transfer value
        }

        private Dictionary<string, string> getXPathMapping(XmlDocument document)
        {
            Dictionary<string, string> mappings = new Dictionary<string, string>();

            if (document.DocumentElement != null && document.DocumentElement.ChildNodes.Count > 0)
            {
                foreach (XmlNode targetChild in document.DocumentElement.ChildNodes)
                {
                    fillDictinary(targetChild, mappings);
                }
            }

            return mappings;
        }

        private void fillDictinary(XmlNode target, Dictionary<string, string> mappings)
        {
            string targetPath = XmlUtility.GetXPathToNode(target.FirstChild);

            string sourcePath = simplifiedXPath(targetPath);
            if (!mappings.ContainsKey(sourcePath))
                mappings.Add(sourcePath, targetPath);
            else
                mappings[sourcePath] = targetPath;

            if (target.HasChildNodes)
            {
                foreach (XmlNode child in target.ChildNodes)
                {
                    fillDictinary(child, mappings);
                }
            }
        }

        private XmlDocument mapNode(XmlDocument destinationDoc, XmlNode destinationParentNode, XmlNode sourceNode)
        {
            string sourceKey = XmlUtility.GetXPathToNode(sourceNode);
            string sourceXPath = XmlUtility.GetDirectXPathToNode(sourceNode); // from incoming json

            if (mappings.ContainsKey(sourceKey) && !string.IsNullOrEmpty(sourceKey))//&& !string.IsNullOrEmpty(sourceNode.InnerText)) // should be a simple element
            {
                string targetXPath = mappings[sourceKey];

                //ToDo checkif the way to map is intern to extern or extern to intern
                // X[1]\XType[2]\Y[1]\yType[4]\F[1]\yType[2]
                string destinationXPath = mapExternPathToInternPathWithIndex(sourceXPath, targetXPath);
                XmlNode destinationNode = destinationDoc.SelectSingleNode(destinationXPath);

                if (destinationNode == null)
                    destinationNode = XmlUtility.GenerateNodeFromXPath(destinationDoc, destinationDoc as XmlNode, destinationXPath, null, null);

                // if a xml element has text, then there is a child of type xmltext
                if (!string.IsNullOrEmpty(sourceNode.InnerText) == sourceNode.LastChild is XmlText)
                    destinationNode.InnerText = sourceNode.InnerText;

                // add dynamic att
                if (sourceNode.Attributes.Count > 0) // may not add if attr is empty
                {
                    foreach (XmlAttribute attr in sourceNode.Attributes)
                    {
                        var a = destinationNode.OwnerDocument.CreateAttribute(attr.Name);
                        a.Value = attr.Value;
                        destinationNode.Attributes.Append(a);
                    }
                }
            }

            if (sourceNode.HasChildNodes)
            {
                foreach (XmlNode childNode in sourceNode.ChildNodes)
                {
                    destinationDoc = mapNode(destinationDoc, destinationParentNode, childNode);
                }
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

        private void addDynamicAttributes(XmlNode source, XmlNode target)
        {
            if (source.Attributes != null && source.Attributes.Count > 0)
            {
                foreach (XmlNode attr in source.Attributes)
                {
                    if (target != null && !string.IsNullOrEmpty(attr.Value))
                    {
                        if (target.Attributes.GetNamedItem(attr.Name) != null) // attr exist
                        {
                            target.Attributes.GetNamedItem(attr.Name).Value = attr.Value; // update it
                        }
                        else
                        {
                            var newAttr = target.OwnerDocument.CreateAttribute(attr.Name);
                            newAttr.Value = attr.Value;
                            target.Attributes.Append(newAttr);
                        }
                    }
                }
            }

            if (source.HasChildNodes)
            {
                for (int i = 0; i < source.ChildNodes.Count; i++)
                {
                    if (target != null && target.ChildNodes.Count > 0)
                        addDynamicAttributes(source.ChildNodes[i], target.ChildNodes[i]);
                }
            }
        }

        /// <summary>
        /// is not only a direct convertion.
        /// Usage/Type (xml) = Usage (json)
        ///
        /// e.g.
        /// xml =  Metadata/TechnicalContacts[1]/TechnicalContactsType[1]/TechnicalContact[1]/TechnicalContactType[1]/Name[1]/NameType[1]
        /// new xml = Metadata/TechnicalContacts.TechnicalContact.Name.#text
        /// </summary>
        /// <returns></returns>
        private string simplifiedXPath(string xpath)
        {
            // if the expath contains #text, then remove it,
            // this part of the xpath is not needed
            xpath = xpath.Replace(@"/#text", "");

            string[] xpaths = xpath.Split('/');
            string newPath = "";

            /// xml = Metadata/TechnicalContacts/TechnicalContactsType/TechnicalContact/TechnicalContactType/Name/NameType
            ///       0        1                 2                     3                4                    5    6
            for (int i = 0; i < xpaths.Length; i++)
            {
                if (i == 0) newPath += xpaths[i];
                else if (i % 2 > 0) newPath += "/" + xpaths[i];
            }

            return newPath;
        }

        /// <summary>
        /// the incoming metadata json will be checked against the metadata example
        /// this will not check missing reuired nodes, its only count the number of elements that should not be in the metadata
        /// find mismatch in the structure will list in errors. This list contains the names of the elements
        /// </summary>
        /// <param name="metadataJson"></param>
        /// <param name="metadataStructureId"></param>
        /// <param name="errors">name of elements that should not be in the metadata json</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public bool HasValidStructure(JObject metadataJson, long metadataStructureId, out List<string> errors)
        {
            // check incoming parameters
            if (metadataJson == null) throw new ArgumentNullException("metadataJson", "MetadataJson must not be null.");
            if (metadataStructureId <= 0) throw new ArgumentNullException("metadataStructureId", "metadataStructureId must be greater then 0.");

            // converting json to xml
            XmlDocument metadataInput = JsonConvert.DeserializeXmlNode(metadataJson.ToString(), "Metadata");

            // load example xml based on metadata structure
            var xmlMetadatWriter = new XmlMetadataWriter(BExIS.Xml.Helpers.XmlNodeMode.xPath);
            var metadataExample = xmlMetadatWriter.CreateMetadataXml(metadataStructureId);

            // get all elements from xml documents to compare
            var listOfElementsInput = XmlUtility.GetAllChildren(XmlUtility.ToXDocument(metadataInput).Root).Select(e => e.Name.LocalName);
            var listOfElementsExample = XmlUtility.GetAllChildren(metadataExample.Root).Select(e => e.Name.LocalName);

            // lets compare all elements
            // the incoming metadata json will be checked against the metadata example
            // this will not check missing nodes, its only count the number of elements that should not be in the metadata
            errors = listOfElementsInput.Where(e => !listOfElementsExample.Contains(e)).ToList();

            if (errors.Any()) return false;

            return true;
        }

        #endregion JSON to XML

        #region xml to xml based on xsd

        //public XmlDocument ConvertTo(XmlDocument source, string xsdPath)
        //{
        //    if (source == null) throw new ArgumentNullException("source", "source must not be null.");
        //    if (string.IsNullOrEmpty(xsdPath)) throw new ArgumentNullException("xsdPath", "xsdPath must not be null.");
        //    if (!File.Exists(xsdPath)) throw new ArgumentNullException("xsdPath", "file not exist.");

        //    XmlDocument newMetadata = new XmlDocument();

        //    XmlSchemaManager xmlSchemaManager = new XmlSchemaManager();
        //    xmlSchemaManager.Load(xsdPath, "test");

        //    if (source.DocumentElement != null)
        //    {
        //        XmlElement root = source.DocumentElement;

        //        var type = xmlSchemaManager.Elements.Where(e => e.Name == root.Name).ToList().FirstOrDefault();

        //        //if(type != null)

        //            //XmlElement targetRoot = newMetadata.CreateElement()

        //    }

        //    return newMetadata;
        //}

        //private XmlElement addChild(XmlElement parentSource, XmlElement ParentTarget XmlDocument newMetadata)
        //{
        //}

        #endregion xml to xml based on xsd
    }
}