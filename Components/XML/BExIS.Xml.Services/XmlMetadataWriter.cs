using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using Vaiona.Entities.Common;
using BExIS.Dlm.Entities.MetadataStructure;
using BExIS.Dlm.Services.MetadataStructure;

/// <summary>
///
/// </summary>        
namespace BExIS.Xml.Services
{
    /// <summary>
    /// 
    /// </summary>
    public enum XmlNodeType
    { 
        MetadataPackage,
        MetadataPackageUsage,
        MetadataAttribute,
        MetadataAttributeUsage
    }

    /// <summary>
    ///
    /// </summary>
    /// <remarks></remarks>        
    public class XmlMetadataWriter:XmlWriter
    {
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
        public XDocument CreateMetadataXml(long metadataStructureId)
        {
            MetadataStructureManager metadataStructureManager = new MetadataStructureManager();
            MetadataStructure metadataStructure = metadataStructureManager.Repo.Get(metadataStructureId);
            MetadataPackageManager metadataPackageManager = new MetadataPackageManager();
            MetadataAttributeManager metadataAttributeManager = new MetadataAttributeManager();

            List<MetadataPackageUsage> packages = metadataStructureManager.GetEffectivePackages(metadataStructureId).ToList();

            // Create xml Document
            // Create the xml document containe
            XDocument doc = new XDocument();// Create the XML Declaration, and append it to XML document
            //XDeclaration dec = new XDeclaration("1.0", null, null);
            //doc.Add(dec);// Create the root element
            XElement root =new XElement("Metadata");
            root.SetAttributeValue("id", metadataStructure.Id.ToString());
            doc.Add(root);

            
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


                attributes = mpu.MetadataPackage.MetadataAttributeUsages.ToList();

                foreach (MetadataAttributeUsage mau in attributes)
                {
                    XElement attribute;

                    XElement attributeRole = CreateXElement(mau.Label, XmlNodeType.MetadataAttributeUsage);
                    if (_mode.Equals(XmlNodeMode.xPath)) attributeRole.SetAttributeValue("name", mau.Label);
                    package.Add(attributeRole);

                    attribute = CreateXElement(mau.MetadataAttribute.Name, XmlNodeType.MetadataAttribute);
                    if (_mode.Equals(XmlNodeMode.xPath)) attribute.SetAttributeValue("name", mau.MetadataAttribute.Name);

                    attribute.SetAttributeValue("roleId", mau.Id.ToString());
                    attribute.SetAttributeValue("id", mau.MetadataAttribute.Id.ToString());
                    attribute.SetAttributeValue("number", "1");
                    attributeRole.Add(attribute);

                }
            }

            return doc;
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
            public XDocument AddPackage(XDocument metadataXml, MetadataPackageUsage packageUsage, int number)
            {
                this._tempXDoc = metadataXml;
                XElement role; 
                //check if role exist
                if(Exist(packageUsage.Label))
                {
                    role = Get(packageUsage.Label);

                }
                else
                {
                    // create the role
                    role = CreateXElement(packageUsage.Label, XmlNodeType.MetadataPackageUsage);
                    if (_mode.Equals(XmlNodeMode.xPath)) role.SetAttributeValue("name", packageUsage.Label);
                    role.SetAttributeValue("id", packageUsage.Id.ToString());
                }

                //root.Add(role);

                if (!Exist(packageUsage.MetadataPackage.Name, number))
                {
                    XElement package;
                    // create the package
                    package = CreateXElement(packageUsage.MetadataPackage.Name, XmlNodeType.MetadataPackage);

                    if (_mode.Equals(XmlNodeMode.xPath)) package.SetAttributeValue("name", packageUsage.MetadataPackage.Name);
                    package.SetAttributeValue("roleId", packageUsage.Id.ToString());
                    package.SetAttributeValue("id", packageUsage.MetadataPackage.Id.ToString());
                    package.SetAttributeValue("number", number);
                    role.Add(package);

                    foreach (MetadataAttributeUsage attribute in packageUsage.MetadataPackage.MetadataAttributeUsages)
                    {
                        package = AddAttribute(package, attribute, 1);
                    }
                }
                else
                {
                    throw new Exception("package exist");
                }

                return metadataXml;
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
            public XDocument RemovePackage(XDocument metadataXml, MetadataPackageUsage packageUsage, int number)
            {
                this._tempXDoc = metadataXml;
                XElement role;
                //check if role exist
                if (Exist(packageUsage.Label))
                {
                    role = Get(packageUsage.Label);

                    XElement package = Get(packageUsage.MetadataPackage.Name, number, role);
                    List<XElement> listOfPackagesAfter = GetChildren(packageUsage.MetadataPackage.Name, role).Where(p=>p.Attribute("number")!=null && Convert.ToInt64(p.Attribute("number").Value) > number).ToList();
                    if (package != null)
                    {
                        package.Remove();
                    }

                    listOfPackagesAfter.ForEach(p => p.Attribute("number").SetValue(Convert.ToInt64(p.Attribute("number").Value) - 1));
                }

                return metadataXml;
            }
        #endregion

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
            private XElement AddAttribute(XElement package, MetadataAttributeUsage attributeUsage, int number)
            {
                if (!Exist(attributeUsage.MetadataAttribute.Name, number, package))
                {
                    XElement role = Get(attributeUsage.Label, package);
                    if (role == null)
                    {
                        role = CreateXElement(attributeUsage.Label, XmlNodeType.MetadataAttributeUsage);
                        if (_mode.Equals(XmlNodeMode.xPath)) role.SetAttributeValue("name", attributeUsage.Label);
                        role.SetAttributeValue("id", attributeUsage.Id.ToString());
                    }

                    XElement element = CreateXElement(attributeUsage.MetadataAttribute.Name, XmlNodeType.MetadataAttribute);

                    if (_mode.Equals(XmlNodeMode.xPath)) element.SetAttributeValue("name", attributeUsage.MetadataAttribute.Name);
                    element.SetAttributeValue("roleId", attributeUsage.Id.ToString());
                    element.SetAttributeValue("id", attributeUsage.MetadataAttribute.Id.ToString());
                    element.SetAttributeValue("number", number);
                    role.Add(element);
                    package.Add(role);
                
                }
                else
                {
                    throw new Exception("attribute exist");
                }

                return package;
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
            public XDocument AddAttribute(XDocument metadataXml, MetadataPackageUsage packageUsage, int packageNumber, MetadataAttributeUsage attributeUsage, int number)
            {
                _tempXDoc = metadataXml;

                XElement packageRole = Get(packageUsage.Label);
                XElement package = Get(packageUsage.MetadataPackage.Name, packageNumber, packageRole);

                if (!Exist(attributeUsage.MetadataAttribute.Name, number, package))
                {
                    XElement role = Get(attributeUsage.Label, package);
                    if (role == null)
                    {
                        role = CreateXElement(attributeUsage.Label, XmlNodeType.MetadataAttributeUsage);
                        if (_mode.Equals(XmlNodeMode.xPath)) role.SetAttributeValue("name", attributeUsage.Label);
                        role.SetAttributeValue("id", attributeUsage.Id.ToString());
                    }

                    XElement element = CreateXElement(attributeUsage.MetadataAttribute.Name, XmlNodeType.MetadataAttribute);

                    if (_mode.Equals(XmlNodeMode.xPath)) element.SetAttributeValue("name", attributeUsage.MetadataAttribute.Name);
                    element.SetAttributeValue("roleId", attributeUsage.Id.ToString());
                    element.SetAttributeValue("id", attributeUsage.MetadataAttribute.Id.ToString());
                    element.SetAttributeValue("number", number);

                
                    List<XElement> listOfPackagesAfter = GetChildren(attributeUsage.MetadataAttribute.Name, role).Where(p => p.Attribute("number") != null && Convert.ToInt64(p.Attribute("number").Value) >= number).ToList();
                    listOfPackagesAfter.ForEach(p => p.Attribute("number").SetValue(Convert.ToInt64(p.Attribute("number").Value) + 1));

                    //after element
                    XElement afterElement = Get(attributeUsage.MetadataAttribute.Name, number+1, role);
                    if (afterElement != null)
                    {
                        afterElement.AddBeforeSelf(element);
                    }
                    else
                    {
                        role.Add(element);
                    }

                    //package.Add(role);

                }
                else
                {
                    throw new Exception("attribute exist");
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
            public XDocument RemoveAttribute(XDocument metadataXml, MetadataPackageUsage packageUsage, int packageNumber, MetadataAttributeUsage attributeUsage, int number)
            {
                _tempXDoc = metadataXml;

                XElement packageRole = Get(packageUsage.Label);
                XElement package = Get(packageUsage.MetadataPackage.Name, packageNumber, packageRole);
                XElement role = Get(attributeUsage.Label, package);
                if (role != null)
                {
                    if (Exist(attributeUsage.MetadataAttribute.Name, number, role))
                    {

                        XElement attribute = Get(attributeUsage.MetadataAttribute.Name, number, role);
                        List<XElement> listOfPackagesAfter = GetChildren(attributeUsage.MetadataAttribute.Name, role).Where(p => p.Attribute("number") != null && Convert.ToInt64(p.Attribute("number").Value) > number).ToList();

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
        #endregion

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
            public XDocument Update(XDocument metadataXml, MetadataPackageUsage packageUsage, int packageNumber, MetadataAttributeUsage attributeUsage, int number, object value)
            {
                _tempXDoc = metadataXml;

                //exist packageRole
                if (Exist(packageUsage.Label))
                {
                    XElement packageRole = Get(packageUsage.Label);

                    //exist package
                    if(Exist(packageUsage.MetadataPackage.Name,packageNumber,packageRole))
                    {
                        XElement package = Get(packageUsage.MetadataPackage.Name, packageNumber, packageRole);

                        //attribute role exist
                        if (Exist(attributeUsage.Label, package))
                        {
                            XElement attributeRole = Get(attributeUsage.Label, package);
                            if (attributeRole != null)
                            {
                                XElement attribute = Get(attributeUsage.MetadataAttribute.Name, number, attributeRole);
                                attribute.SetValue(value.ToString());
                            }
                        }
                    }
                }


                return _tempXDoc;
            }

        #endregion

    }
}
