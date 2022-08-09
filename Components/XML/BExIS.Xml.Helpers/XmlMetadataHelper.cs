using BExIS.Dlm.Entities.Common;
using BExIS.Dlm.Entities.MetadataStructure;
using BExIS.Dlm.Services.MetadataStructure;
using BExIS.Utils.Models;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using Vaiona.Persistence.Api;

namespace BExIS.Xml.Helpers
{
    public class XmlMetadataHelper
    {

        /// <summary>
        /// Returns a List of SearchMetadataNode
        /// for each Metadata Simple Attribute
        /// </summary>
        /// <param name="id">Metadata Structure Id</param>
        /// <returns> List<SearchMetadataNode> </returns>
        public List<SearchMetadataNode> GetAllXPathsOfSimpleAttributes(long id)
        {
            List<SearchMetadataNode> list = new List<SearchMetadataNode>();

            // load metadatastructure with all packages and attributes

            using (var uow = this.GetUnitOfWork())
            {
                string title = uow.GetReadOnlyRepository<MetadataStructure>().Get(id).Name;

                XmlMetadataWriter xmlMetadatWriter = new XmlMetadataWriter(XmlNodeMode.xPath);
                XDocument metadataXml = xmlMetadatWriter.CreateMetadataXml(id);

                List<XElement> elements = metadataXml.Root.Descendants().Where(e => e.HasElements.Equals(false)).ToList();

                foreach (XElement element in elements)
                {
                    list.Add(
                        new SearchMetadataNode(title, XExtentsions.GetAbsoluteXPath(element).Substring(1))
                        );
                }

                return list;
            }
        }

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
        public JObject ConvertTo(XmlDocument metadata, bool includeEmpty=false)
        { 

            var metadataJson = new JObject();

            var root = metadata.DocumentElement;


            long metadataStructureId = 0;

            // add first node metadata


            if (root.HasAttributes)
            {
                foreach (XmlAttribute attr in root.Attributes)
                {

                    JProperty p = new JProperty("@"+attr.Name,attr.Value);

                    metadataJson.Add(p);

                    // if attr is id, means id of the metadata Strutcure
                    if (attr.Name.ToLower().Equals("id"))
                    {
                        if(Int64.TryParse(attr.Value, out metadataStructureId) == false)
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

                    for (int i = 0; i < root.ChildNodes.Count; i++)
                    {
                        XmlNode node = root.ChildNodes[i];
                        long usageId = metadataStructure.MetadataPackageUsages.ElementAt(i).Id;
                        var usage = metadataStructureManager.PackageUsageRepo.Get(usageId);

                        var packageUsageJson = _convertPackageUsage(node, usage, includeEmpty);
                        if(packageUsageJson!=null)  
                            metadataJson.Add(usage.Label, packageUsageJson);

                    }
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
                        setReference(complex,(XmlElement)tCHild, includeEmpty);

                        for (int i = 0; i < tCHild.ChildNodes.Count; i++)
                        {
                            XmlNode child = tCHild.ChildNodes[i];
                            var childUsage = children[i];

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
                        if (!string.IsNullOrEmpty(type.InnerText) || includeEmpty )
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
                            array.Add(addSimple(usage.Label,type.InnerText, (XmlElement)type, includeEmpty));
                        }

                        if (array.Count == 0) return null;

                        if (array.Count > 0) return array;
                    }
                }

                #endregion

                #region complex

                List<BaseUsage> children = getChildren(usage);

                foreach (XmlNode tCHild in element.ChildNodes) // loop over the list of entry
                {
                    if (tCHild != null && tCHild.HasChildNodes)
                    {
                        // complex stuff
                        // add all children nodes
                        JObject complex = new JObject();
                        setReference(complex,(XmlElement)tCHild, includeEmpty);

                        for (int i = 0; i < tCHild.ChildNodes.Count; i++)
                        {
                            XmlNode child = tCHild.ChildNodes[i];
                            var childUsage = children[i];

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

                        if(!complex.Children().Any() ) return null;

                        if (usage.MaxCardinality <= 1) return complex;
                        else
                        {
                            array.Add(complex); // add each element to a array 
                        }
                    }   
                }

                return array;

                #endregion
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

                if(metadataAttribute.Self is MetadataCompoundAttribute)
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
        }
    }
}
