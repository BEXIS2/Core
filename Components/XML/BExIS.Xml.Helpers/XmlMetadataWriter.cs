using BExIS.Dlm.Entities.Common;
using BExIS.Dlm.Entities.MetadataStructure;
using BExIS.Dlm.Services.MetadataStructure;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using Vaiona.Persistence.Api;

/// <summary>
///
/// </summary>
namespace BExIS.Xml.Helpers
{
    /// <summary>
    ///
    /// </summary>
    public enum XmlNodeType
    {
        MetadataPackage,
        MetadataPackageUsage,
        MetadataAttribute,
        MetadataAttributeUsage,
        MetadataCompoundAttribute,
        MetadataNestedAttributeUsage,
        Other
    }

    /// <summary>
    ///
    /// </summary>
    /// <remarks></remarks>
    public class XmlMetadataWriter : XmlWriter
    {
        private MetadataAttributeManager _metadataAttrManager = null;

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param name="mode"></param>
        public XmlMetadataWriter(XmlNodeMode mode)
        {
            _mode = mode;
        }

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param name="metadataStructureId"></param>
        /// <returns></returns>
        public XDocument CreateMetadataXml(long metadataStructureId, XDocument importXml = null)
        {
            using (IUnitOfWork uow = this.GetUnitOfWork())
            using (MetadataStructureManager metadataStructureManager = new MetadataStructureManager())
            using (MetadataAttributeManager metadataAttributeManager = new MetadataAttributeManager())
            {
                _metadataAttrManager = metadataAttributeManager;

                MetadataStructure metadataStructure = this.GetUnitOfWork().GetReadOnlyRepository<MetadataStructure>().Get(metadataStructureId);

                List<Int64> packageIds = metadataStructureManager.GetEffectivePackageIds(metadataStructureId).ToList();

                // Create xml Document
                // Create the xml document containe
                XDocument doc = new XDocument();// Create the XML Declaration, and append it to XML document
                                                //XDeclaration dec = new XDeclaration("1.0", null, null);
                                                //doc.Add(dec);// Create the root element
                XElement root = new XElement("Metadata");
                root.SetAttributeValue("id", metadataStructure.Id.ToString());
                doc.Add(root);

            
                IList<MetadataPackageUsage> packages = uow.GetReadOnlyRepository<MetadataPackageUsage>().Get(p => packageIds.Contains(p.Id));
                List<MetadataAttributeUsage> attributes;
                foreach (MetadataPackageUsage mpu in packages)
                {
                    XElement package;

                    // create the role
                    XElement role = CreateXElement(mpu.Label, XmlNodeType.MetadataPackageUsage);
                    if (_mode.Equals(XmlNodeMode.xPath)) role.SetAttributeValue("name", mpu.Label);

                    role.SetAttributeValue("id", mpu.Id.ToString());
                    root.Add(role);

                    // create the package
                    package = CreateXElement(mpu.MetadataPackage.Name, XmlNodeType.MetadataPackage);
                    if (_mode.Equals(XmlNodeMode.xPath)) package.SetAttributeValue("name", mpu.MetadataPackage.Name);
                    package.SetAttributeValue("roleId", mpu.Id.ToString());
                    package.SetAttributeValue("id", mpu.MetadataPackage.Id.ToString());
                    package.SetAttributeValue("number", "1");
                    role.Add(package);

                    setChildren(package, mpu, importXml);
                }

                return doc;
            }
        }

        private XElement setChildren(XElement element, BaseUsage usage, XDocument importDocument = null)
        {
            MetadataAttribute metadataAttribute = null;
            MetadataPackage metadataPackage = null;

            if (usage is MetadataAttributeUsage)
            {
                MetadataAttributeUsage metadataAttributeUsage = (MetadataAttributeUsage)usage;
                metadataAttribute = metadataAttributeUsage.MetadataAttribute;
            }
            else if (usage is MetadataNestedAttributeUsage)
            {
                MetadataNestedAttributeUsage mnau = (MetadataNestedAttributeUsage)usage;
                metadataAttribute = mnau.Member;
            }
            else
            {
                MetadataPackageUsage mpu = (MetadataPackageUsage)usage;
                metadataPackage = mpu.MetadataPackage;
            }

            if (metadataAttribute != null && metadataAttribute.Self is MetadataCompoundAttribute)
            {
                MetadataCompoundAttribute mca = this.GetUnitOfWork().GetReadOnlyRepository<MetadataCompoundAttribute>().Get(metadataAttribute.Self.Id);

                foreach (MetadataNestedAttributeUsage nestedUsage in mca.MetadataNestedAttributeUsages)
                {
                    if (importDocument != null)
                    {
                        string parentPath = element.GetAbsoluteXPathWithIndex();

                        string usagePath = parentPath + "/" + nestedUsage.Label;

                        XElement usageElement = importDocument.XPathSelectElement(usagePath);
                        List<XElement> typeList = new List<XElement>();

                        if (usageElement != null && usageElement.HasElements)
                        {
                            int num = usageElement.Elements().Count();

                            if (num == 0)
                            {
                                typeList = addAndReturnAttribute(element, nestedUsage, 1, 1);
                            }
                            else
                            {
                                typeList = addAndReturnAttribute(element, nestedUsage, 1, num);
                            }
                        }
                        else
                        {
                            Debug.WriteLine("NULL OR EMPTY:------> " + usagePath);

                            typeList = addAndReturnAttribute(element, nestedUsage, 1, 1);
                        }

                        

                        foreach (var type in typeList)
                        {
                            setChildren(type, nestedUsage, importDocument);
                        }
                    }
                    else
                    {
                        List<XElement> typeList = new List<XElement>();

                        typeList = addAndReturnAttribute(element, nestedUsage, 1, 1);
                        setChildren(typeList.FirstOrDefault(), nestedUsage, importDocument);
                    }
                }
            }
            else
            {
                if (metadataPackage != null)
                {
                    foreach (MetadataAttributeUsage attrUsage in metadataPackage.MetadataAttributeUsages)
                    {
                        if (importDocument != null)
                        {
                            string parentPath = element.GetAbsoluteXPathWithIndex();

                            string usagePath = parentPath + "/" + attrUsage.Label;

                            XElement usageElement = importDocument.XPathSelectElement(usagePath);
                            List<XElement> typeList = new List<XElement>();

                            if (usageElement != null && usageElement.HasElements)
                            {
                                int num = usageElement.Elements().Count();

                                if (num == 0)
                                {
                                    typeList = addAndReturnAttribute(element, attrUsage, 1, 1);
                                }
                                else
                                {
                                    typeList = addAndReturnAttribute(element, attrUsage, 1, num);
                                }
                            }
                            else
                            {
                                Debug.WriteLine("NULL OR EMPTY:------> " + usagePath);

                                typeList = addAndReturnAttribute(element, attrUsage, 1, 1);
                            }

                            foreach (var type in typeList)
                            {
                                setChildren(type, attrUsage, importDocument);
                            }
                        }
                        else
                        {
                            List<XElement> typeList = new List<XElement>();

                            typeList = addAndReturnAttribute(element, attrUsage, 1, 1);
                            setChildren(typeList.FirstOrDefault(), attrUsage, importDocument);
                        }
                    }
                }
            }

            return element;
        }

        #region package

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param name="metadataXml"></param>
        /// <param name="packageUsage"></param>
        /// <param name="number"></param>
        /// <returns></returns>
        public XDocument AddPackage(XDocument metadataXml, BaseUsage usage, int number, string typeName, long typeId, List<BaseUsage> children, XmlNodeType xmlType, XmlNodeType xmlUsageType, string xpath)
        {
            this._tempXDoc = metadataXml;
            XElement role;
            //check if role exist
            if (Exist(xpath))
            {
                role = Get(xpath);
            }
            else
            {
                // create the role
                role = CreateXElement(usage.Label, xmlUsageType);
                if (_mode.Equals(XmlNodeMode.xPath)) role.SetAttributeValue("name", usage.Label);
                role.SetAttributeValue("id", usage.Id.ToString());
            }

            //root.Add(role);
            string xPathForNewElement = xpath + "//" + typeName + "[" + number + "]"; ;//xpath.Substring (0,xpath.Length-2)+number+"]";

            XElement package;
            // create the package
            package = CreateXElement(typeName, xmlType);

            if (_mode.Equals(XmlNodeMode.xPath)) package.SetAttributeValue("name", typeName);
            package.SetAttributeValue("roleId", usage.Id.ToString());
            package.SetAttributeValue("id", typeId);
            package.SetAttributeValue("number", number);

            //if (!Exist(xPathForNewElement))
            if (!Exist(typeName, number, role))
            {
                role.Add(package);
                foreach (BaseUsage attribute in children)
                {
                    AddAttribute(package, attribute, 1);
                }

                //XElement element = XmlUtility.GetXElementByAttribute(usage.Label, "id", usage.Id.ToString(), metadataXml);

                //element.Add(package);
            }
            else
            {
                role = UpdateNumberOfSameElements(role, package, typeName, number);
                foreach (BaseUsage attribute in children)
                {
                    AddAttribute(package, attribute, 1);
                }
            }

            return metadataXml;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="metadataXml"></param>
        /// <param name="xpath"></param>
        /// <returns></returns>
        public XDocument Remove(XDocument metadataXml, string xpath)
        {
            this._tempXDoc = metadataXml;

            if (this._tempXDoc.XPathSelectElement(xpath) != null)
            {
                XElement element = this._tempXDoc.XPathSelectElement(xpath);

                XElement parent = element.Parent;

                removeAndUpdate(element, parent);
            }

            return metadataXml;
        }

        private void removeAndUpdate(XElement element, XElement parent)
        {
            int number = Convert.ToInt32(element.Attribute("number").Value);

            List<XElement> listOfPackagesAfter = parent.Elements().Where(p => p.Attribute("number") != null && Convert.ToInt64(p.Attribute("number").Value) > number).ToList();

            if (element != null)
            {
                element.Remove();
                listOfPackagesAfter.ForEach(p => p.Attribute("number").SetValue(Convert.ToInt64(p.Attribute("number").Value) - 1));
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="metadataXml"></param>
        /// <param name="xpath"></param>
        /// <returns></returns>
        public XDocument Clean(XDocument metadataXml, string xpath)
        {
            this._tempXDoc = metadataXml;

            if (this._tempXDoc.XPathSelectElement(xpath) != null)
            {
                XElement element = this._tempXDoc.XPathSelectElement(xpath);

                element = clean(element);
            }

            return metadataXml;
        }

        private XElement clean(XElement element)
        {
            if (element != null)
            {
                foreach (XElement e in element.Elements())
                {
                    //some stuff here

                    if (!e.HasElements && e.Value != null) e.Value = String.Empty;
                    else clean(e);
                }

                return element;
            }

            return null;
        }

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param name="metadataXml"></param>
        /// <param name="packageUsage"></param>
        /// <param name="number"></param>
        /// <returns></returns>
        public XDocument RemovePackage(XDocument metadataXml, BaseUsage packageUsage, int number, string typeName)
        {
            this._tempXDoc = metadataXml;
            XElement role;
            //check if role exist
            if (Exist(packageUsage.Label, packageUsage.Id))
            {
                role = Get(packageUsage.Label, packageUsage.Id);

                XElement package = Get(typeName, number, role);
                List<XElement> listOfPackagesAfter = GetChildren(typeName, role).Where(p => p.Attribute("number") != null && Convert.ToInt64(p.Attribute("number").Value) > number).ToList();
                if (package != null)
                {
                    package.Remove();
                }

                listOfPackagesAfter.ForEach(p => p.Attribute("number").SetValue(Convert.ToInt64(p.Attribute("number").Value) - 1));
            }

            return metadataXml;
        }

        #endregion package

        #region attribute

        //Add Attribute to a package return a apackage
        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param name="package"></param>
        /// <param name="attributeUsage"></param>
        /// <param name="number"></param>
        /// <returns></returns>
        private XElement AddAttribute(XElement current, BaseUsage attributeUsage, int number)
        {
            string typeName = "";
            string id = "";
            string roleId = "";
            List<MetadataNestedAttributeUsage> children = new List<MetadataNestedAttributeUsage>();
            ICollection<MetadataParameterUsage> parameters = new List<MetadataParameterUsage>();


            if (attributeUsage is MetadataAttributeUsage)
            {
                MetadataAttributeUsage metadataAttributeUsage = (MetadataAttributeUsage)attributeUsage;
                typeName = metadataAttributeUsage.MetadataAttribute.Name;
                id = metadataAttributeUsage.MetadataAttribute.Id.ToString();

                // changed by Sven & David @ 2021-04-28
                // roleId = metadataAttributeUsage.MetadataAttribute.Id.ToString();
                roleId = metadataAttributeUsage.Id.ToString();

                if (metadataAttributeUsage.MetadataAttribute.Self is MetadataCompoundAttribute)
                {
                    MetadataCompoundAttribute mca = (MetadataCompoundAttribute)metadataAttributeUsage.MetadataAttribute.Self;
                    children = mca.MetadataNestedAttributeUsages.ToList();
                }

                if (metadataAttributeUsage.MetadataAttribute.MetadataParameterUsages.Any())
                    parameters = metadataAttributeUsage.MetadataAttribute.MetadataParameterUsages;
            }
            else
            {
                MetadataNestedAttributeUsage mnau = (MetadataNestedAttributeUsage)attributeUsage;
                typeName = mnau.Member.Name;
                id = mnau.Member.Id.ToString();
                roleId = mnau.Id.ToString();

                if (mnau.Member.Self is MetadataCompoundAttribute)
                {
                    MetadataCompoundAttribute mca = (MetadataCompoundAttribute)mnau.Member.Self;
                    children = mca.MetadataNestedAttributeUsages.ToList();
                }

                if (mnau.Member.MetadataParameterUsages.Any())
                    parameters = mnau.Member.MetadataParameterUsages;
            }

            if (!Exist(typeName, number, current))
            {
                XElement role = Get(attributeUsage.Label, current);
                if (role == null)
                {
                    role = CreateXElement(attributeUsage.Label, XmlNodeType.MetadataAttributeUsage);
                    if (_mode.Equals(XmlNodeMode.xPath)) role.SetAttributeValue("name", attributeUsage.Label);
                    role.SetAttributeValue("id", attributeUsage.Id.ToString());
                }

                XElement element = CreateXElement(typeName, XmlNodeType.MetadataAttribute);

                if (_mode.Equals(XmlNodeMode.xPath)) element.SetAttributeValue("name", typeName);
                element.SetAttributeValue("roleId", roleId);
                element.SetAttributeValue("id", id);
                element.SetAttributeValue("number", number);

                // add parameters
                addParameters(element, parameters);

                if (children.Count > 0)
                {
                    foreach (BaseUsage baseUsage in children)
                    {
                        element = AddAttribute(element, baseUsage, 1);
                    }
                }



                role.Add(element);
                current.Add(role);
            }
            else
            {
                throw new Exception("attribute exist");
            }

            return current;
        }

        //Add Attribute to a package return a apackage
        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param name="package"></param>
        /// <param name="attributeUsage"></param>
        /// <param name="number"></param>
        /// <returns></returns>
        private List<XElement> addAndReturnAttribute(XElement current, BaseUsage attributeUsage, int number, int countOfTypes)
        {
            List<XElement> tmp = new List<XElement>();

            MetadataAttribute _metadataAttribute = null;

            string typeName = "";
            string id = "";
            string roleId = "";
            long metadataAttrId = 0;
            ICollection<MetadataParameterUsage> parameters =  new List<MetadataParameterUsage>();

            if (attributeUsage is MetadataAttributeUsage)
            {
                MetadataAttributeUsage metadataAttributeUsage = (MetadataAttributeUsage)attributeUsage;
                typeName = metadataAttributeUsage.MetadataAttribute.Name;
                metadataAttrId = metadataAttributeUsage.MetadataAttribute.Id;
                id = metadataAttrId.ToString();
                roleId = metadataAttributeUsage.Id.ToString();
                
            }
            else
            {
                MetadataNestedAttributeUsage mnau = (MetadataNestedAttributeUsage)attributeUsage;
                typeName = mnau.Member.Name;
                metadataAttrId = mnau.Member.Id;
                id = metadataAttrId.ToString();
                roleId = mnau.Id.ToString();
            }

            // set parameters
            // reload type from db 
            _metadataAttribute = _metadataAttrManager.MetadataAttributeRepo.Get(metadataAttrId);
            if (_metadataAttribute!=null && _metadataAttribute.MetadataParameterUsages.Any())
                parameters = _metadataAttribute.MetadataParameterUsages;


            if (!Exist(typeName, number, current))
            {
                XElement role = Get(attributeUsage.Label, current);
                if (role == null)
                {
                    role = CreateXElement(attributeUsage.Label, XmlNodeType.MetadataAttributeUsage);
                    if (_mode.Equals(XmlNodeMode.xPath)) role.SetAttributeValue("name", attributeUsage.Label);
                    role.SetAttributeValue("id", attributeUsage.Id.ToString());
                }

                for (int i = 0; i < countOfTypes; i++)
                {
                    XElement element = CreateXElement(typeName, XmlNodeType.MetadataAttribute);

                    if (_mode.Equals(XmlNodeMode.xPath)) element.SetAttributeValue("name", typeName);
                    element.SetAttributeValue("roleId", roleId);
                    element.SetAttributeValue("id", id);
                    element.SetAttributeValue("number", i + 1);

                    // add parameters
                    addParameters(element, parameters);

                    role.Add(element);

                    tmp.Add(element);
                }

                current.Add(role);
                //Debug.WriteLine("Element:            " + element.Name);

    
                return tmp;
            }
            else
            {
                throw new Exception("attribute exist");
            }
        }

        //Add Attribute to a package return a apackage
        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param name="package"></param>
        /// <param name="attributeUsage"></param>
        /// <param name="number"></param>
        /// <returns></returns>
        private XElement addAttributeReturnType(XElement current, BaseUsage attributeUsage, int number)
        {
            string typeName = "";
            string id = "";
            string roleId = "";

            ICollection<MetadataParameterUsage> parameters = new List<MetadataParameterUsage>();


            if (attributeUsage is MetadataAttributeUsage)
            {
                MetadataAttributeUsage metadataAttributeUsage = (MetadataAttributeUsage)attributeUsage;
                typeName = metadataAttributeUsage.MetadataAttribute.Name;
                id = metadataAttributeUsage.MetadataAttribute.Id.ToString();
                roleId = metadataAttributeUsage.MetadataAttribute.Id.ToString();

                if (metadataAttributeUsage.MetadataAttribute.MetadataParameterUsages.Any())
                    parameters = metadataAttributeUsage.MetadataAttribute.MetadataParameterUsages;
            }
            else
            {
                MetadataNestedAttributeUsage mnau = (MetadataNestedAttributeUsage)attributeUsage;
                typeName = mnau.Member.Name;
                id = mnau.Member.Id.ToString();
                roleId = mnau.Member.Id.ToString();

                if (mnau.Member.MetadataParameterUsages.Any())
                    parameters = mnau.Member.MetadataParameterUsages;
            }

            if (!Exist(typeName, number, current))
            {
                XElement role = Get(attributeUsage.Label, current);
                if (role == null)
                {
                    role = CreateXElement(attributeUsage.Label, XmlNodeType.MetadataAttributeUsage);
                    if (_mode.Equals(XmlNodeMode.xPath)) role.SetAttributeValue("name", attributeUsage.Label);
                    role.SetAttributeValue("id", attributeUsage.Id.ToString());
                }

                XElement element = CreateXElement(typeName, XmlNodeType.MetadataAttribute);

                if (_mode.Equals(XmlNodeMode.xPath)) element.SetAttributeValue("name", typeName);
                element.SetAttributeValue("roleId", roleId);
                element.SetAttributeValue("id", id);
                element.SetAttributeValue("number", number);

                // add parameters
                addParameters(element, parameters);

                role.Add(element);
                current.Add(role);

                return current;
            }
            else
            {
                throw new Exception("attribute exist");
            }

            return null;
        }

        private XElement addParameters(XElement current, ICollection<MetadataParameterUsage> parameterUsages)
        {
            foreach (var parameterUsage in parameterUsages)
            {
                current.SetAttributeValue(parameterUsage.Label, parameterUsage.DefaultValue);
            }

            return current;
        }



        //Add Attribute to a package return a apackage
        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param name="metadataXml"></param>
        /// <param name="packageUsage"></param>
        /// <param name="packageNumber"></param>
        /// <param name="attributeUsage"></param>
        /// <param name="number"></param>
        /// <returns></returns>
        public XDocument AddAttribute(XDocument metadataXml, BaseUsage attributeUsage, int number, string attributeTypeName, string attributeId, string parentXPath)
        {
            _tempXDoc = metadataXml;

            /*
             * In the xml the structure is everytime usage/type
             *
             * e.g. personUsage/PersonType/name/NameType
             *
             * has a attribute has cardinality more then one
             * the usage is the sequence node
             *
             * personUsage/PersonType/name/NameType[1]
             * personUsage/PersonType/name/NameType[2]
             *
             * in this function the parent is e.g PersonType
             * so we need to add the usage name to the xpath to select the right node to add the attribute
             */

            string usageXPath = parentXPath + "/" + attributeUsage.Label;
            XElement role = Get(usageXPath);

            XElement element = CreateXElement(attributeTypeName, XmlNodeType.MetadataAttribute);

            if (_mode.Equals(XmlNodeMode.xPath)) element.SetAttributeValue("name", attributeTypeName);
            element.SetAttributeValue("roleId", attributeUsage.Id.ToString());
            element.SetAttributeValue("id", attributeId);
            element.SetAttributeValue("number", number);

            if (!Exist(attributeTypeName, number, role))
            {
                role = UpdateNumberOfSameElements(role, element, attributeTypeName, number);
            }
            else
            {
                role = UpdateNumberOfSameElements(role, element, attributeTypeName, number);
            }

            return _tempXDoc;
        }

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param name="metadataXml"></param>
        /// <param name="packageUsage"></param>
        /// <param name="packageNumber"></param>
        /// <param name="attributeUsage"></param>
        /// <param name="number"></param>
        /// <returns></returns>
        public XDocument RemoveAttribute(XDocument metadataXml, BaseUsage attributeUsage, int number, string attributeName, string parentXPath)
        {
            _tempXDoc = metadataXml;

            /*
            * In the xml the structure is everytime usage/type
            *
            * e.g. personUsage/PersonType/name/NameType
            *
            * has a attribute has cardinality more then one
            * the usage is the sequence node
            *
            * personUsage/PersonType/name/NameType[1]
            * personUsage/PersonType/name/NameType[2]
            *
            * in this function the parent is e.g PersonType
            * so we need to add the usage name to the xpath to select the right node to remove the attribute
            */

            string usageXPath = parentXPath + "/" + attributeUsage.Label;
            XElement role = Get(usageXPath);
            if (role != null)
            {
                if (Exist(attributeName, number, role))
                {
                    XElement attribute = Get(attributeName, number, role);
                    List<XElement> listOfPackagesAfter = GetChildren(attributeName, role).Where(p => p.Attribute("number") != null && Convert.ToInt64(p.Attribute("number").Value) > number).ToList();

                    if (attribute != null)
                    {
                        attribute.Remove();
                    }

                    listOfPackagesAfter.ForEach(p => p.Attribute("number").SetValue(Convert.ToInt64(p.Attribute("number").Value) - 1));
                }
            }
            else
            {
                throw new Exception("attribute exist");
            }

            return _tempXDoc;
        }

        #endregion attribute

        public XDocument Change(XDocument metadataXml, string firstXPath, string secondXPath)
        {
            this._tempXDoc = metadataXml;

            if (this._tempXDoc.XPathSelectElement(firstXPath) != null &&
                this._tempXDoc.XPathSelectElement(secondXPath) != null)
            {
                XmlDocument xmlDocument = XmlUtility.ToXmlDocument(metadataXml);

                XmlNode first = xmlDocument.SelectSingleNode(firstXPath);
                XmlNode next = xmlDocument.SelectSingleNode(secondXPath);

                string contentFromFirst = first.InnerXml;
                string contentFromNext = next.InnerXml;

                first.InnerXml = contentFromNext;
                next.InnerXml = contentFromFirst;

                metadataXml = XmlUtility.ToXDocument(xmlDocument);
            }

            return metadataXml;
        }

        #region update

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param name="metadataXml"></param>
        /// <param name="packageUsage"></param>
        /// <param name="packageNumber"></param>
        /// <param name="attributeUsage"></param>
        /// <param name="number"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public XDocument Update(XDocument metadataXml, BaseUsage attributeUsage, int number, object value, string attributeTypeName, string parentXpath)
        {
            _tempXDoc = metadataXml;

            XElement parent = Get(parentXpath);

            if (parent != null)
            {
                //attribute role exist
                if (Exist(attributeUsage.Label, parent))
                {
                    XElement attributeRole = Get(attributeUsage.Label, parent);
                    if (attributeRole != null)
                    {
                        XElement attribute = Get(attributeTypeName, number, attributeRole);
                        if(value!=null) attribute.SetValue(value.ToString());
                        else attribute.SetValue(string.Empty);
                    }
                }
            }

            return _tempXDoc;
        }

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param name="metadataXml"></param>
        /// <param name="packageUsage"></param>
        /// <param name="packageNumber"></param>
        /// <param name="attributeUsage"></param>
        /// <param name="number"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public XDocument Update(XDocument metadataXml, BaseUsage attributeUsage, int number, object value, string attributeTypeName, string parentXpath, Dictionary<string, string> xmlAttrs)
        {
            _tempXDoc = metadataXml;

            XElement parent = Get(parentXpath);

            if (parent != null)
            {
                //attribute role exist
                if (Exist(attributeUsage.Label, parent))
                {
                    XElement attributeRole = Get(attributeUsage.Label, parent);
                    if (attributeRole != null)
                    {
                        XElement attribute = Get(attributeTypeName, number, attributeRole);
                        if(value!=null) attribute.SetValue(value.ToString());

                        if (xmlAttrs != null)
                        {
                            foreach (var kvp in xmlAttrs)
                            {
                                //create or replace
                                if (attribute.Attributes().Any(a => a.Name.ToString().Equals(kvp.Key)))
                                {
                                    XAttribute xattribute = attribute.Attributes().FirstOrDefault(a => a.Name.ToString().Equals(kvp.Key));

                                    //replace
                                    if (xattribute != null) xattribute.Value = kvp.Value;
                                    //create
                                    else
                                    {
                                        attribute.Add(new XAttribute(kvp.Key, kvp.Value));
                                    }
                                }
                                else
                                {
                                    attribute.Add(new XAttribute(kvp.Key, kvp.Value));
                                }
                            }
                        }
                    }
                }
            }

            return _tempXDoc;
        }

        /// <summary>
        /// Updating the number of all childrens after the number was selected
        /// and adding the element to the parent on the specific plave
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="element"></param>
        /// <param name="typeName"></param>
        /// <param name="number"></param>
        /// <returns></returns>
        private XElement UpdateNumberOfSameElements(XElement parent, XElement element, string typeName, int number)
        {
            List<XElement> listOfPackagesAfter = GetChildren(typeName, parent).Where(p => p.Attribute("number") != null && Convert.ToInt64(p.Attribute("number").Value) >= number).ToList();
            listOfPackagesAfter.ForEach(p => p.Attribute("number").SetValue(Convert.ToInt64(p.Attribute("number").Value) + 1));

            //after element
            XElement afterElement = Get(typeName, number + 1, parent);
            if (afterElement != null)
            {
                afterElement.AddBeforeSelf(element);
            }
            else
            {
                parent.Add(element);
            }

            return parent;
        }

        #endregion update

        #region static

        public static XmlNodeType GetXmlNodeType(string typeName)
        {
            foreach (XmlNodeType type in Enum.GetValues(typeof(XmlNodeType)))
            {
                if (type.ToString().Equals(typeName))
                    return type;
            }

            return XmlNodeType.Other;
        }

        #endregion static
    }
}