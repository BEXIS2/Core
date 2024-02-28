using BExIS.Dlm.Entities.DataStructure;
using BExIS.Dlm.Entities.MetadataStructure;
using BExIS.Dlm.Services.DataStructure;
using BExIS.Dlm.Services.MetadataStructure;
using BExIS.IO;
using BExIS.Xml.Models.Mapping;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Schema;
using Vaiona.Persistence.Api;
using Vaiona.Utils.Cfg;

namespace BExIS.Xml.Helpers.Mapping
{
    public class XmlSchemaManager
    {
        public string FileName { get; set; }

        public List<XmlSchemaComplexType> ComplexTypes { get; set; }
        public List<XmlSchemaComplexType> ComplexTypesWithSimpleTypesAsChildrens { get; set; }
        public List<XmlSchemaElement> Elements { get; set; }
        public List<XmlSchemaSimpleType> SimpleTypes { get; set; }
        public List<XmlSchemaGroup> Groups { get; set; }
        public List<XmlSchemaAttribute> Attributes { get; set; }
        public List<string> RefElementNames { get; set; }
        public XmlSchema Schema;
        public XmlSchemaSet SchemaSet;

        //Contraits of system
        public List<MetadataAttribute> MetadataAttributes { get; set; }

        public Dictionary<string, List<Constraint>> ConvertedSimpleTypes { get; set; }

        private List<string> additionalFiles { get; set; }

        private string SchemaName = "";
        private string xsdFilePath = "";
        private string xsdFileName = "";
        private string newXsdFilePath = "";

        public string mappingFileNameImport = "";
        public string mappingFileNameExport = "";
        private XmlMapper mappingFileInternalToExternal = new XmlMapper();
        private XmlMapper mappingFileExternalToInternal = new XmlMapper();

        private Dictionary<long, string> createdAttributesDic { get; set; }
        private Dictionary<long, string> createdPackagesDic { get; set; }
        private Dictionary<long, string> createdCompoundsDic { get; set; }
        private Dictionary<long, string> createdParametersDic { get; set; }

        private string userName = "";
        private string location = "";

        public XmlNamespaceManager XmlNamespaceManager;
        private XmlDatasetHelper xmlDatasetHelper = new XmlDatasetHelper();

        public XmlSchemaManager()
        {
            Elements = new List<XmlSchemaElement>();
            RefElementNames = new List<string>();
            ComplexTypes = new List<XmlSchemaComplexType>();
            ComplexTypesWithSimpleTypesAsChildrens = new List<XmlSchemaComplexType>();
            SimpleTypes = new List<XmlSchemaSimpleType>();
            MetadataAttributes = new List<MetadataAttribute>();
            ConvertedSimpleTypes = new Dictionary<string, List<Constraint>>();
            Groups = new List<XmlSchemaGroup>();
            Attributes = new List<XmlSchemaAttribute>();
            xsdFileName = "";
            additionalFiles = new List<string>();
            mappingFileInternalToExternal = new XmlMapper();
            mappingFileExternalToInternal = new XmlMapper();
            createdAttributesDic = new Dictionary<long, string>();
            createdCompoundsDic = new Dictionary<long, string>();
            createdPackagesDic = new Dictionary<long, string>();
            createdParametersDic = new Dictionary<long, string>();
        }

        public static bool MoveFile(string tempFile, string destinationPath)
        {

            if (tempFile.Equals(destinationPath)) return true; // allready on th place

            if (File.Exists(tempFile))
            {
                if(File.Exists(destinationPath)) File.Delete(destinationPath);

                File.Move(tempFile, destinationPath);

                if (File.Exists(destinationPath))
                {
                    return true;
                }
                else return false;
            }
            else return false;
        }

        #region load schema

        /// <summary>
        /// Load Schema from path
        /// </summary>
        /// <param name="path"></param>
        public void Load(string path, string username)
        {
            if(string.IsNullOrEmpty(path)) throw new ArgumentNullException("path");
            if(!File.Exists(path)) throw new FileNotFoundException("path");
            if (string.IsNullOrEmpty(username)) throw new ArgumentNullException("username");

            userName = username;

            xsdFilePath = path;
            xsdFileName = Path.GetFileName(path);

            FileName = xsdFileName;

            int countedSchemas = 0;

            XmlReaderSettings settings2 = new XmlReaderSettings();
            settings2.DtdProcessing = DtdProcessing.Ignore;

            // need to manage IO exceptions, e.g. pah not found , ...
            XmlReader xsd_file = XmlReader.Create(path, settings2);
            Schema = XmlSchema.Read(xsd_file, verifyErrors);

            countedSchemas = Schema.Includes.Count + 1;

            XmlSchema selectedSchema;

            XmlSchemaSet schemaSet = new XmlSchemaSet();
            schemaSet.ValidationEventHandler += new ValidationEventHandler(verifyErrors);
            schemaSet.Add(Schema);

            schemaSet.Compile();

            if (schemaSet.Count < countedSchemas)
            {
                foreach (XmlSchemaObject additional in Schema.Includes)
                {
                    schemaSet = addToSchemaSet(additional, schemaSet);
                }
            }

            foreach (XmlSchema currentSchema in schemaSet.Schemas())
            {
                //add all additional schemas
                additionalFiles.Add(Path.GetFileName(currentSchema.SourceUri.ToString()));

                selectedSchema = currentSchema;
                Elements.AddRange(GetAllElements(selectedSchema));
                ComplexTypes.AddRange(GetAllComplexTypes(selectedSchema));
                SimpleTypes.AddRange(GetAllSimpleTypes(selectedSchema));
                Groups.AddRange(GetAllGroups(selectedSchema));
                Attributes.AddRange(GetAllAttributes(selectedSchema));
            }

            RefElementNames.AddRange(GetAllRefElementNames(Elements));
            SchemaSet = schemaSet;
            xsd_file.Close();
        }

        private XmlSchemaSet addToSchemaSet(XmlSchemaObject xmlSchemaObject, XmlSchemaSet xmlSchemaSet)
        {
            if (xmlSchemaObject is XmlSchemaInclude)
            {
                XmlSchemaInclude include = (XmlSchemaInclude)xmlSchemaObject;

                if (include.Schema == null)
                {
                    XmlReaderSettings settings = new XmlReaderSettings();
                    settings.DtdProcessing = DtdProcessing.Ignore;

                    string dataPath = Path.Combine(AppConfiguration.DataPath, "Temp", userName, include.SchemaLocation.Split('/').Last());
  
                    using (XmlReader test = XmlReader.Create(dataPath, settings))
                    {
                        include.Schema = XmlSchema.Read(test, verifyErrors);
                    }
                }

                xmlSchemaSet.Add(include.Schema);
                additionalFiles.Add(Path.GetFileName(include.Schema.SourceUri.ToString()));
                // if schema has included schemas
                if (include.Schema.Includes.Count > 0)
                {
                    foreach (XmlSchemaObject additional in include.Schema.Includes)
                    {
                        addToSchemaSet(additional, xmlSchemaSet);
                    }
                }

                return xmlSchemaSet;
            }

            return xmlSchemaSet;
        }

        public void Delete(string schemaName)
        {
            MetadataStructureManager mdsManager = new MetadataStructureManager();
            // create default metadataStructure
            try
            {
                MetadataStructure test = mdsManager.Repo.Get(p => p.Name == SchemaName).FirstOrDefault();
                if (test != null) mdsManager.Delete(test);
            }
            finally
            {
                mdsManager.Dispose();
            }
        }

        private IEnumerable<string> GetAllRefElementNames(List<XmlSchemaElement> elements)
        {
            List<string> names = new List<string>();

            foreach (XmlSchemaElement element in elements)
            {
                if (String.IsNullOrEmpty(element.Name))
                    names.Add(element.RefName.Name);
            }

            return names;
        }

        private IEnumerable<XmlSchemaGroup> GetAllGroups(XmlSchema selectedSchema)
        {
            return XmlSchemaUtility.GetAllGroups(selectedSchema);
        }

        /// <summary>
        /// Return a list of all complext types in the schema
        /// </summary>
        /// <param name="schema"></param>
        /// <returns>List<XmlSchemaComplexType></returns>
        private List<XmlSchemaComplexType> GetAllComplexTypes(XmlSchema schema)
        {
            return XmlSchemaUtility.GetAllComplexTypes(schema);
        }

        public List<XmlSchemaComplexType> GetAllComplextTypesWithSimpleTypesAsChildrens()
        {
            foreach (XmlSchemaComplexType type in ComplexTypes)
            {
                if (isComplexTypeOnlyWithSimpleTpesAsChildrens(type))
                    ComplexTypesWithSimpleTypesAsChildrens.Add(type);
            }

            return ComplexTypesWithSimpleTypesAsChildrens;
        }

        private bool isComplexTypeOnlyWithSimpleTpesAsChildrens(XmlSchemaComplexType type)
        {
            List<XmlSchemaElement> elements = XmlSchemaUtility.GetAllElements(type, false, Elements);

            foreach (XmlSchemaElement element in elements)
            {
                if (!XmlSchemaUtility.IsSimpleType(element))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Return a list of all elements in the schema
        /// </summary>
        /// <param name="schema"></param>
        /// <returns></returns>
        private List<XmlSchemaElement> GetAllElements(XmlSchema schema)
        {
            return XmlSchemaUtility.GetAllElements(schema);
        }

        /// <summary>
        /// Return a list of all elements in the schema
        /// </summary>
        /// <param name="schema"></param>
        /// <returns></returns>
        private List<XmlSchemaSimpleType> GetAllSimpleTypes(XmlSchema schema)
        {
            return XmlSchemaUtility.GetAllSimpleTypes(schema);
        }

        /// <summary>
        /// Return a list of all elements in the schema
        /// </summary>
        /// <param name="schema"></param>
        /// <returns></returns>
        private List<XmlSchemaAttribute> GetAllAttributes(XmlSchema schema)
        {
            return XmlSchemaUtility.GetAllAttributes(schema);
        }

        /// <summary>
        /// Get all elements in a list which have a simpletype as type
        /// </summary>
        /// <returns></returns>
        public List<XmlSchemaElement> GetAllElementsTypeIsSimpleType()
        {
            List<XmlSchemaElement> elementsWithSimpleType = new List<XmlSchemaElement>();

            foreach (XmlSchemaElement element in Elements)
            {
                if (XmlSchemaUtility.IsSimpleType(element))
                {
                    elementsWithSimpleType.Add(element);
                }
            }

            return elementsWithSimpleType;
        }

        /// <summary>
        /// Check if the node is defined as a sequence in the schema
        /// </summary>
        /// <param name="node">xmlnode to check</param>
        /// <returns></returns>
        public bool IsSequence(XmlNode node)
        {
            XmlSchemaElement element = Elements.Where(e => e.Name.Equals(node.LocalName)).FirstOrDefault();

            if (element != null)
            {
                XmlSchemaComplexType type = element.ElementSchemaType as XmlSchemaComplexType;
                if (type != null)
                {
                    XmlSchemaSequence sequence = type.ContentTypeParticle as XmlSchemaSequence;
                    if (sequence != null)
                    {
                        return true;
                    }

                    XmlSchemaChoice choice = type.ContentTypeParticle as XmlSchemaChoice;
                    if (choice != null)
                    {
                        if (!existXmlSchemaElement(choice.Items))
                        {
                            foreach (XmlSchemaObject obj in choice.Items)
                            {
                                return isSequence(obj);
                            }
                        }

                        return false;
                    }
                }
            }

            return false;
        }

        private bool isSequence(XmlSchemaObject element)
        {
            XmlSchemaSequence sequence = element as XmlSchemaSequence;
            if (sequence != null)
            {
                return true;
            }

            XmlSchemaChoice choice = element as XmlSchemaChoice;
            if (choice != null)
            {
                if (!existXmlSchemaElement(choice.Items))
                {
                    foreach (XmlSchemaObject obj in choice.Items)
                    {
                        return isSequence((XmlSchemaElement)obj);
                    }
                }

                return false;
            }

            XmlSchemaElement e = element as XmlSchemaElement;

            if (e != null)
            {
                XmlSchemaComplexType type = e.ElementSchemaType as XmlSchemaComplexType;
                if (type != null)
                {
                    ////Debug.Writeline("Element");
                }
            }

            return false;
        }

        private bool existXmlSchemaElement(XmlSchemaObjectCollection collection)
        {
            foreach (XmlSchemaObject obj in collection)
            {
                XmlSchemaElement element = obj as XmlSchemaElement;
                if (element != null)
                {
                    XmlSchemaComplexType type = element.ElementSchemaType as XmlSchemaComplexType;
                    if (type != null) return true;
                }
            }

            return false;
        }

        /// <summary>
        /// returns true if the xmlnode has attributes
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        public bool HasAttributes(XmlNode node)
        {
            XmlSchemaElement element = Elements.Where(e => e.Name.Equals(node.LocalName)).FirstOrDefault();
            if (element != null)
            {
                XmlSchemaComplexType type = element.ElementSchemaType as XmlSchemaComplexType;

                if (type != null)
                {
                    // If the complex type has any attributes, get an enumerator
                    // and write each attribute name to the Debug.
                    if (type.AttributeUses.Count > 0)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Get a list of all attributes from the xmlnode
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        public List<XmlSchemaAttribute> GetAttributes(XmlNode node)
        {
            List<XmlSchemaAttribute> listOfAttributes = new List<XmlSchemaAttribute>();

            XmlSchemaElement element = Elements.Where(e => e.Name.Equals(node.LocalName)).FirstOrDefault();
            if (element != null)
            {
                XmlSchemaComplexType type = element.ElementSchemaType as XmlSchemaComplexType;
                if (type != null)
                {
                    // If the complex type has any attributes, get an enumerator
                    // and write each attribute name to the Debug.
                    if (type.AttributeUses.Count > 0)
                    {
                        foreach (XmlSchemaObject obj in type.AttributeUses.Values)
                        {
                            XmlSchemaAttribute attr = (XmlSchemaAttribute)obj;
                            listOfAttributes.Add(attr);
                        }
                    }
                }
            }

            return listOfAttributes;
        }

        public int GetIndexOfChild(XmlNode node, XmlNode child)
        {
            XmlSchemaElement parent = Elements.Where(e => e.Name.Equals(node.LocalName)).FirstOrDefault();

            if (parent != null)
            {
                ////Debug.Writeline("********** CHILDREN fo ONE Node");
                List<XmlSchemaElement> list = XmlSchemaUtility.GetAllElements(parent, false, Elements);

                if (list.Where(e => e.Name.Equals(child.Name)).Count() > 0)
                {
                    for (int i = 0; i < list.Count(); i++)
                    {
                        if (list.ElementAt(i).Name.Equals(child.Name)) return i;
                    }
                }
            }
            else
            {
                ////Debug.Writeline("PARENT = NULL ---> " + node.LocalName);
                return 0;
            }

            return -1;
        }

        //event handler to manage the errors
        private void verifyErrors(object sender, ValidationEventArgs args)
        {
            if (args.Severity == XmlSeverityType.Warning)
            {
                //Debug.Writeline(args.Message);
            }
        }

        #endregion load schema

        #region import To MetadatStructure

        public Dictionary<string, List<Constraint>> ConvertSimpleTypes()
        {
            //MetadataAttributeManager mam = new MetadataAttributeManager();
            //List<XmlSchemaElement> elementsWithSimpleType = this.GetAllElementsTypeIsSimpleType();

            //foreach(XmlSchemaElement element in elementsWithSimpleType)
            //{
            //    //ConvertedSimplesTypes.Add(type.Name, ConvertToConstraints(type));
            //}

            return ConvertedSimpleTypes;
        }

        #region metadata structure

        public long GenerateMetadataStructure(string nameOfStartNode, string schemaName)
        {
            using (MetadataStructureManager mdsManager = new MetadataStructureManager())
            using (MetadataPackageManager mdpManager = new MetadataPackageManager())
            using (MetadataAttributeManager mamManager = new MetadataAttributeManager())
            using (DataTypeManager dataTypeManager = new DataTypeManager())
            {
                if (!String.IsNullOrEmpty(schemaName))
                    SchemaName = schemaName;

                string rootElementName = nameOfStartNode;

                newXsdFilePath = Path.Combine(AppConfiguration.GetModuleWorkspacePath("DCM"), "Metadata", schemaName, FileName);

                #region prepare mappingFiles

                #region intern to extern

                // add schema to mappingfile
                mappingFileInternalToExternal.Header.AddToSchemas(schemaName, "Metadata/" + schemaName + "/" + FileName);

                #endregion intern to extern

                #region extern to intern

                mappingFileExternalToInternal.Header.AddToDestination("Metadata");
                mappingFileExternalToInternal.Header.AddToSchemas(schemaName, "Metadata/" + schemaName + "/" + FileName);

                //ToDo id and name of metadatastructure fehlt

                #endregion extern to intern

                #endregion prepare mappingFiles

                //List<MetadataAttribute> metadataAttributes = new List<MetadataAttribute>();
                //metadataAttributes = GenerateAllMetadataAttributes();
                List<XmlSchemaElement> elementsWithSimpleType = GetAllElementsTypeIsSimpleType();
                List<XmlSchemaComplexType> complexTypesWithSimpleTypesAsChildrensOnly = GetAllComplextTypesWithSimpleTypesAsChildrens();

                // create default metadataStructure
                MetadataStructure test = null; //mdsManager.Repo.Get(p => p.Name == SchemaName).FirstOrDefault();
                if (test == null) test = mdsManager.Create(SchemaName, SchemaName, "", "", null);

                XmlSchemaObject root = new XmlSchemaElement();
                string xpathFromRoot = "";

                int count = 0;

                foreach (XmlSchemaObject obj in Schema.Items)
                {
                    if (obj is XmlSchemaElement)
                    {
                        root = (XmlSchemaElement)obj;
                        //HACK find first xsd element
                        break;
                    }
                }

                if (String.IsNullOrEmpty(nameOfStartNode))
                {
                    XmlSchemaElement rootElement = (XmlSchemaElement)root;
                    mappingFileInternalToExternal.Header.AddToDestination(rootElement.Name, rootElement.Name);

                    rootElementName = rootElement.Name;
                    xpathFromRoot = rootElementName;
                }
                else
                {
                    //XXX finde path from rootnode the defined root node
                    //XPath in mapping file needs to be complete based on the original xsd
                    xpathFromRoot = findPathFromRoot((XmlSchemaElement)root, nameOfStartNode, "");

                    root = Elements.Where(e => e.Name.ToLower().Equals(nameOfStartNode.ToLower())).FirstOrDefault();
                    if (root == null)
                    {
                        root = Groups.Where(g => g.Name.ToLower().Equals(nameOfStartNode.ToLower())).FirstOrDefault();
                    }

                    if (nameOfStartNode != "")
                    {
                        XmlSchemaElement rootElement = (XmlSchemaElement)root;
                        mappingFileInternalToExternal.Header.AddToDestination(nameOfStartNode, rootElement.Name);
                    }
                }

                List<XmlSchemaElement> childrenOfRoot = XmlSchemaUtility.GetAllElements(root, false, Elements);
                List<XmlSchemaElement> packages = new List<XmlSchemaElement>();

                if (XmlSchemaUtility.IsAllSimpleType(childrenOfRoot))
                {
                    #region root with only simple type childrens

                    XmlSchemaAnnotation annotation = null;
                    string name = "";

                    if (root is XmlSchemaElement)
                    {
                        XmlSchemaElement rootAsElement = (XmlSchemaElement)root;
                        name = rootAsElement.Name;
                        annotation = rootAsElement.Annotation;
                    }
                    else
                    if (root is XmlSchemaGroup)
                    {
                        XmlSchemaGroup rootAsGroup = (XmlSchemaGroup)root;
                        name = rootAsGroup.Name;
                        annotation = rootAsGroup.Annotation;
                    }

                    MetadataPackage package = getExistingMetadataPackage(name);

                    if (package == null)
                    {
                        package = mdpManager.Create(name, GetDescription(annotation), true);
                        createdPackagesDic.Add(package.Id, package.Name);
                    }

                    if (test.MetadataPackageUsages.Where(p => p.MetadataPackage == package).Count() <= 0)
                    {
                        string xpath = "Metadata/" + name;

                        foreach (XmlSchemaElement child in childrenOfRoot)
                        {
                            //Debug.Writeline("packageChild : " + child.Name);
                            //Debug.Writeline("-->");

                            if (XmlSchemaUtility.IsSimpleType(child))
                            {
                                addMetadataAttributeToMetadataPackageUsage(package, child, xpath, name);
                            }
                            else
                            {
                                List<string> parents = new List<string>();
                                parents.Add(name);

                                MetadataCompoundAttribute compoundAttribute = get(child, parents, xpath, name, mamManager, dataTypeManager);

                                // add compound to package
                                addUsageFromMetadataCompoundAttributeToPackage(package, compoundAttribute, child);
                            }
                        }

                        mdsManager.AddMetadataPackageUsage(test, package, name, GetDescription(annotation), 1, 1);
                    }

                    #endregion root with only simple type childrens
                }
                else
                {
                    packages.AddRange(childrenOfRoot);

                    #region packages with complext types

                    #region create a basic package of SimpleAttributes from Root Node

                    // get all simpleTypes
                    // for the main package
                    List<XmlSchemaElement> simpleElements = XmlSchemaUtility.GetAllSimpleElements(packages);
                    string rootNodePackageUsage = "Basic";
                    string rootNodePackage = "BasicType";

                    string rootNodePackageDescription = "Attributes from the root node";

                    if (simpleElements.Count > 0)
                    {
                        MetadataPackage package = getExistingMetadataPackage(rootNodePackage);// = mdpManager.MetadataPackageRepo.Get(p => p.Name == rootNodePackage).FirstOrDefault();
                        if (package == null)
                        {
                            package = mdpManager.Create(rootNodePackage, rootNodePackageDescription, true);
                            createdPackagesDic.Add(package.Id, package.Name);
                        }

                        if (test.MetadataPackageUsages.Where(p => p.MetadataPackage == package).Count() <= 0)
                        {
                            foreach (XmlSchemaElement child in simpleElements)
                            {
                                if (XmlSchemaUtility.IsSimpleType(child))
                                {
                                    addMetadataAttributeToMetadataPackageUsage(package, child, "Metadata/Basic/BasicType", xpathFromRoot);
                                }
                            }

                            mdsManager.AddMetadataPackageUsage(test, package, rootNodePackageUsage, rootNodePackageDescription, 1, 1);
                        }
                    }

                    #endregion create a basic package of SimpleAttributes from Root Node

                    #region create Packages

                    List<XmlSchemaElement> otherElements = XmlSchemaUtility.GetAllComplexElements(packages);

                    foreach (XmlSchemaElement element in otherElements)
                    {
                        //Debug.Writeline("package : " + element.Name);
                        //Debug.Writeline("--------------------------");

                        string typeName = GetTypeOfName(element.Name);
                        string rootName = ((XmlSchemaElement)root).Name;

                        string xpathInternal = "Metadata/" + element.Name + "/" + typeName;
                        string xpathExternal = xpathFromRoot + "/" + element.Name;

                        if (!XmlSchemaUtility.IsSimpleType(element))
                        {
                            #region complexType

                            MetadataPackage package = getExistingMetadataPackage(element.Name);
                            //Debug.WriteLine("-->" + element.Name);
                            if (package == null)
                            {
                                package = mdpManager.Create(typeName, GetDescription(element.Annotation), true);
                                createdPackagesDic.Add(package.Id, package.Name);
                            }
                            // add package to structure
                            if (test.MetadataPackageUsages != null && test.MetadataPackageUsages.Where(p => p.Label.Equals(element.Name)).Count() > 0)
                            {
                                if (test.MetadataPackageUsages.Where(p => p.MetadataPackage == package).Count() <= 0)
                                {
                                    List<XmlSchemaElement> childrens = XmlSchemaUtility.GetAllElements(element, false, Elements);

                                    foreach (XmlSchemaElement child in childrens)
                                    {
                                        //Debug.Writeline("packageChild : " + child.Name);
                                        //Debug.Writeline("-->");

                                        if (XmlSchemaUtility.IsSimpleType(child))
                                        {
                                            addMetadataAttributeToMetadataPackageUsage(package, child, xpathInternal, xpathExternal);
                                        }
                                        else
                                        {
                                            List<string> parents = new List<string>();
                                            parents.Add(element.Name);

                                            MetadataCompoundAttribute compoundAttribute = get(child, parents, xpathInternal, xpathExternal, mamManager, dataTypeManager);

                                            // add compound to package
                                            addUsageFromMetadataCompoundAttributeToPackage(package, compoundAttribute, child);
                                        }
                                    }

                                    int min = 0;

                                    if (element.MinOccurs > int.MinValue)
                                        min = Convert.ToInt32(element.MinOccurs);
                                    else
                                        min = int.MinValue;

                                    int max = 0;
                                    if (element.MaxOccurs < int.MaxValue)
                                        max = Convert.ToInt32(element.MaxOccurs);
                                    else
                                        max = int.MaxValue;

                                    mdsManager.AddMetadataPackageUsage(test, package, element.Name, GetDescription(element.Annotation), min, max);
                                }
                            }
                            else
                            {
                                List<XmlSchemaElement> childrens = XmlSchemaUtility.GetAllElements(element, false, Elements);

                                foreach (XmlSchemaElement child in childrens)
                                {
                                    if (XmlSchemaUtility.IsSimpleType(child))
                                    {
                                        addMetadataAttributeToMetadataPackageUsage(package, child, xpathInternal, xpathExternal);
                                    }
                                    else
                                    {
                                        List<string> parents = new List<string>();
                                        parents.Add(element.Name);
                                        //Debug.WriteLine("--->" + element.Name);

                                        MetadataCompoundAttribute compoundAttribute = get(child, parents, xpathInternal, xpathExternal, mamManager, dataTypeManager);

                                        // add compound to package
                                        addUsageFromMetadataCompoundAttributeToPackage(package, compoundAttribute, child);
                                    }
                                }

                                int min = 0;

                                if (element.MinOccurs > int.MinValue)
                                    min = Convert.ToInt32(element.MinOccurs);
                                else
                                    min = int.MinValue;

                                int max = 0;
                                if (element.MaxOccurs < int.MaxValue)
                                    max = Convert.ToInt32(element.MaxOccurs);
                                else
                                    max = int.MaxValue;

                                //check if element is a choice
                                if (!XmlSchemaUtility.IsChoiceType(element))
                                {
                                    MetadataPackageUsage mpu = mdsManager.AddMetadataPackageUsage(test, package,
                                        element.Name, GetDescription(element.Annotation), min, max);
                                }
                                else
                                {
                                    Dictionary<string, string> additionalAttributes = new Dictionary<string, string>();

                                    XmlSchemaComplexType ct = element.ElementSchemaType as XmlSchemaComplexType;

                                    if (ct != null)
                                    {
                                        #region choice

                                        // check if it is e choice
                                        XmlSchemaChoice choice = ct.ContentTypeParticle as XmlSchemaChoice;
                                        if (choice != null)
                                        {
                                            additionalAttributes.Add("min", choice.MinOccurs.ToString());
                                            if (choice.MaxOccurs > 10)
                                                additionalAttributes.Add("max", "10");
                                            else
                                                additionalAttributes.Add("max", choice.MaxOccurs.ToString());
                                        }

                                        #endregion choice
                                    }

                                    // if mpu is a choice, add a info to extra
                                    MetadataPackageUsage mpu = mdsManager.AddMetadataPackageUsage(test, package,
                                            element.Name, GetDescription(element.Annotation), min, max,
                                            xmlDatasetHelper.AddReferenceToXml(new XmlDocument(), "choice", "true", "elementType", @"extra/type", additionalAttributes));
                                }
                            }

                            #endregion complexType
                        }
                    }

                    #endregion create Packages

                    #endregion packages with complext types
                }

                if (!File.Exists(newXsdFilePath))
                {
                    checkDirectory(newXsdFilePath);
                    MoveFile(xsdFilePath, newXsdFilePath);
                }

                #region store additionaly xsds

                string tmpDestinationPath = Path.GetDirectoryName(newXsdFilePath);
                string tmpSourcePath = Path.GetDirectoryName(xsdFilePath);

                if (additionalFiles != null)
                {
                    foreach (var filename in additionalFiles.Distinct())
                    {
                        MoveFile(Path.Combine(tmpSourcePath, filename), Path.Combine(tmpDestinationPath, filename));
                    }
                }

                #endregion store additionaly xsds

                #region Generate Mapping File

                string internalMetadataStructrueName = schemaName;
                mappingFileExternalToInternal.Id = test.Id;

                //generate mapping file Xml Document
                generateXmlMappingFile(mappingFileInternalToExternal, internalMetadataStructrueName, FileName);
                generateXmlMappingFile(mappingFileExternalToInternal, FileName, internalMetadataStructrueName, 1);

                #endregion Generate Mapping File

                return test.Id;
            }

        }

        public bool GenerateMappingFile(long id, string name, string concept, List<XmlMappingRoute> routes)
        {
  
            //mappingFileInternalToExternal.Header.AddToSchemas(name, "Metadata/" + name + "/" + concept);

            mappingFileInternalToExternal.Routes.AddRange(routes);

            generateXmlMappingFile(mappingFileInternalToExternal, name, concept);

            // register
            //mappingFileNameExport;
            using (var metadataStructureManager = new MetadataStructureManager())
            {
                MetadataStructure metadataStructure = this.GetUnitOfWork().GetReadOnlyRepository<MetadataStructure>().Get(id);

                XmlDocument xmlDoc = new XmlDocument();

                if (metadataStructure.Extra != null)
                {
                    xmlDoc = (XmlDocument)metadataStructure.Extra;
                }

                XmlDatasetHelper xmlDatasetHelper = new XmlDatasetHelper();
                XmlNode convertRef = XmlUtility.GetXmlNodeByAttribute(xmlDoc.DocumentElement, "convertRef", "name", concept);

                if (convertRef == null)
                {
                    xmlDoc = xmlDatasetHelper.AddReferenceToXml(xmlDoc, concept, mappingFileNameExport, "mappingFileExport", "extra/convertReferences/convertRef");

                    metadataStructure.Extra = xmlDoc;
                    metadataStructureManager.Update(metadataStructure);
                }
            }

            return false;
        }

        private string findPathFromRoot(XmlSchemaElement element, string name, string path)
        {
            path = String.IsNullOrEmpty(path) ? element.Name : path + "/" + element.Name;

            if (element.Name.ToLower().Equals(name.ToLower())) return path;

            List<XmlSchemaElement> childrens = XmlSchemaUtility.GetAllElements(element, false, Elements);

            foreach (var child in childrens)
            {
                if (!XmlSchemaUtility.IsSimpleType(child))
                {
                    string tmp = findPathFromRoot(child, name, path);
                    if (!String.IsNullOrEmpty(tmp))
                    {
                        path = tmp;
                        break;
                    }
                }
            }

            return path;
        }

        private MetadataCompoundAttribute get(XmlSchemaElement element, List<string> parents, string internalXPath, string externalXPath, MetadataAttributeManager metadataAttributeManager, DataTypeManager dataTypeManager)
        {
            //Debug.Writeline("element :" + element.Name);

            XmlSchemaComplexType ct = XmlSchemaUtility.GetComplextType(element);

            string nameOfType = "";
            if (ct.Name != null)
                nameOfType = ct.Name;
            else
                nameOfType = GetTypeOfName(element.Name);

            MetadataCompoundAttribute metadataCompountAttr = getExistingMetadataCompoundAttribute(nameOfType);
            string currentInternalXPath = internalXPath + "/" + element.Name + "/" + nameOfType;
            string currentExternalXPath = externalXPath + "/" + element.Name;

            // create and map
            if (metadataCompountAttr == null)
            {
                //try to stop a infinit loop

                parents.Add(element.Name);
                if (ct.Name != null)
                {
                    metadataCompountAttr = createMetadataCompoundAttribute(ct);
                }
                else
                {
                    metadataCompountAttr = createMetadataCompoundAttribute(element);
                }

                List<XmlSchemaElement> childrens = XmlSchemaUtility.GetAllElements(element, false, Elements);

                #region childrens

                foreach (XmlSchemaElement child in childrens)
                {
                    //Debug.Writeline("child :" + child.Name);

                    // simple element
                    if (XmlSchemaUtility.IsSimpleType(child))
                    {
                        //Debug.WriteLine(child.Name);

                        metadataCompountAttr = addMetadataAttributeToMetadataCompoundAttribute(
                            metadataCompountAttr, child, currentInternalXPath, currentExternalXPath);
                    }
                    //complex element
                    else
                    {
                        XmlSchemaComplexType complexTypeOfChild = XmlSchemaUtility.GetComplextType(child);

                        //break if infinity loop
                        if (currentInternalXPath.Split('/').Contains(complexTypeOfChild.Name))
                        {
                            Debug.WriteLine("save for infinit loop");
                        }
                        else
                        {
                            if (ct.Name == null || complexTypeOfChild.Name == null)
                            {
                                //Debug.WriteLine(child.Name);
                                //--> create compountAttribute
                                MetadataCompoundAttribute compoundAttributeChild = get(child, parents,
                                    currentInternalXPath, currentExternalXPath, metadataAttributeManager, dataTypeManager);

                                // add compound to compount
                                metadataCompountAttr =
                                    addUsageFromMetadataCompoundAttributeToMetadataCompoundAttribute(
                                        metadataCompountAttr, compoundAttributeChild, child);
                            }
                            else
                            {
                                if (ct.Name != null && complexTypeOfChild.Name != null)
                                {
                                    if (!ct.Name.Equals(complexTypeOfChild.Name))
                                    {
                                        //--> create compountAttribute
                                        MetadataCompoundAttribute compoundAttributeChild = get(child, parents,
                                            currentInternalXPath, currentExternalXPath, metadataAttributeManager, dataTypeManager);
                                        // add compound to compount
                                        metadataCompountAttr =
                                            addUsageFromMetadataCompoundAttributeToMetadataCompoundAttribute(
                                                metadataCompountAttr, compoundAttributeChild, child);
                                    }
                                }
                            }
                        }
                    }
                }

                #endregion childrens

                

                if (
                    metadataAttributeManager.MetadataCompoundAttributeRepo.Get()
                        .Where(m => m.Name.Equals(metadataCompountAttr.Name))
                        .Count() > 0 && metadataCompountAttr.Id > 0)
                {
                    //Debug.WriteLine(metadataCompountAttr.Name);

                    metadataAttributeManager.Update(metadataCompountAttr);
                }
                else
                {
                    //Debug.WriteLine(metadataCompountAttr.Name);

                    metadataCompountAttr = metadataAttributeManager.Create(metadataCompountAttr);
                    createdCompoundsDic.Add(metadataCompountAttr.Id, metadataCompountAttr.Name);
                }


                #region attributes

                if (ct.Attributes.Count > 0)
                {
                    foreach (var attr in ct.Attributes)
                    {
                          var metadataParameteUsage = createMetadataParameterUsage(metadataCompountAttr, attr, dataTypeManager, metadataAttributeManager);
                          addMetadataParameterToMappingFile( attr as XmlSchemaAttribute, currentInternalXPath, currentExternalXPath);
                    }
                }

                #endregion
            }
            //only map
            else
            {
                //try to stop a infinit loop

                // if its there need to map
                List<XmlSchemaElement> childrens = XmlSchemaUtility.GetAllElements(element, false, Elements);

                parents.Add(element.Name);

                #region children

                foreach (XmlSchemaElement child in childrens)
                {
                    //Debug.Writeline("child :" + child.Name);

                    if (XmlSchemaUtility.IsSimpleType(child))
                    {
                        //add existing Simple elements to mappingFile
                        addMetadataAttributeToMappingFile(metadataCompountAttr, child, currentInternalXPath,
                            currentExternalXPath);
                    }
                    else
                    {
                        //break if infinity loop
                        if (parents.Contains(child.Name))
                        {
                            //MetadataCompoundAttribute compoundAttributeChild =
                            //    getExistingMetadataCompoundAttribute(child.Name);
                            Debug.WriteLine("save for infinit loop");
                        }
                        else
                        {
                            XmlSchemaComplexType complexTypeOfChild = XmlSchemaUtility.GetComplextType(child);

                            if (ct.Name == null || complexTypeOfChild.Name == null)
                            {
                                MetadataCompoundAttribute compoundAttributeChild = get(child, parents,
                                    currentInternalXPath, currentExternalXPath, metadataAttributeManager, dataTypeManager);
                            }
                            else
                            {
                                if (ct.Name != null && complexTypeOfChild.Name != null)
                                {
                                    if (!ct.Name.Equals(complexTypeOfChild.Name))
                                    {
                                        MetadataCompoundAttribute compoundAttributeChild = get(child, parents,
                                            currentInternalXPath, currentExternalXPath, metadataAttributeManager, dataTypeManager);
                                    }
                                }
                            }
                        }
                    }

                    #endregion children
                }
            }

            return metadataCompountAttr;
        }

        private void addMetadataAttributesToMetadataPackageUsage(MetadataPackage packageUsage, List<XmlSchemaElement> elements, string internalXPath, string externalXPath)
        {
            for (int i = 0; i < elements.Count(); i++)
            {
                addMetadataAttributeToMetadataPackageUsage(packageUsage, elements.ElementAt(i), internalXPath, externalXPath);
            }
        }

        private void addMetadataAttributeToMetadataPackageUsage(MetadataPackage packageUsage, XmlSchemaElement element, string internalXPath, string externalXPath)
        {
            MetadataPackageManager metadataPackageManager = new MetadataPackageManager();

            try
            {
                MetadataAttribute attribute = createMetadataAttribute(element, internalXPath,externalXPath);

                if (attribute != null)
                {
                    int min = 0;

                    if (element.MinOccurs > int.MinValue)
                        min = Convert.ToInt32(element.MinOccurs);
                    else
                        min = int.MinValue;

                    int max = 0;
                    if (element.MaxOccurs < int.MaxValue)
                        max = Convert.ToInt32(element.MaxOccurs);
                    else
                        max = int.MaxValue;

                    if (packageUsage.MetadataAttributeUsages.Where(p => p.MetadataAttribute == attribute).Count() <= 0)
                        metadataPackageManager.AddMetadataAtributeUsage(packageUsage, attribute, element.Name, attribute.Description, min, max, element.DefaultValue, element.FixedValue);

                    #region generate  MappingRoute

                    addToExportMappingFile(mappingFileInternalToExternal, internalXPath, externalXPath, max, element.Name, attribute.Name);
                    addToImportMappingFile(mappingFileExternalToInternal, externalXPath, internalXPath, max, element.Name, attribute.Name);

                    #endregion generate  MappingRoute
                }
            }
            finally
            {
                metadataPackageManager.Dispose();
            }
        }

        private void addUsageFromMetadataCompoundAttributeToPackage(MetadataPackage package, MetadataCompoundAttribute compoundAttribute, XmlSchemaElement element)
        {
            MetadataPackageManager metadataPackageManager = new MetadataPackageManager();

            try
            {
                if (package.MetadataAttributeUsages.Where(p => p.Label == element.Name).Count() <= 0)
                {
                    int min = 0;

                    if (element.MinOccurs > int.MinValue)
                        min = Convert.ToInt32(element.MinOccurs);
                    else
                        min = int.MinValue;

                    int max = 0;
                    if (element.MaxOccurs < int.MaxValue)
                        max = Convert.ToInt32(element.MaxOccurs);
                    else
                        max = int.MaxValue;

                    //ToDo Choice in XSD required Element

                    /*
                     * In some xsds elements in a choice are required.
                     * but the choice itself say at least one element should be selected
                     * and this means all childrens need to be optional
                     * and one must be selected
                     *
                     */

                    if (XmlSchemaUtility.IsChoiceType(element))
                    {
                        min = 0;
                    }

                    metadataPackageManager.AddMetadataAtributeUsage(package, compoundAttribute, element.Name, GetDescription(element.Annotation), min, max, element.DefaultValue, element.FixedValue);
                }
            }
            finally
            {
                metadataPackageManager.Dispose();
            }
        }

        private MetadataCompoundAttribute addUsageFromMetadataCompoundAttributeToMetadataCompoundAttribute(MetadataCompoundAttribute parent, MetadataCompoundAttribute compoundAttribute, XmlSchemaElement element)
        {
            if (parent.MetadataNestedAttributeUsages.Where(p => p.Label == element.Name).Count() <= 0)
            {
                //get max
                int max = Int32.MaxValue;
                int min = Convert.ToInt32(element.MinOccurs);
                if (element.MaxOccurs < Int32.MaxValue)
                {
                    max = Convert.ToInt32(element.MaxOccurs);
                }

                #region choice

                //ToDo Choice in XSD required Element

                /*
                 * In some xsds elements in a choice are required.
                 * but the choice itself say at least one element should be selected
                 * and this means all childrens need to be optional
                 * and one must be selected
                 *                  *
                 */

                //if element is a choise
                XmlDocument extra = new XmlDocument();
                //check if element is a choice
                if (XmlSchemaUtility.IsChoiceType(element))
                {
                    min = 0;
                    Dictionary<string, string> additionalAttributes = new Dictionary<string, string>();

                    XmlSchemaComplexType ct = element.ElementSchemaType as XmlSchemaComplexType;

                    if (ct != null)
                    {
                        #region choice

                        // check if it is e choice
                        XmlSchemaChoice choice = ct.ContentTypeParticle as XmlSchemaChoice;
                        if (choice != null)
                        {
                            additionalAttributes.Add("min", choice.MinOccurs.ToString());
                            if (choice.MaxOccurs > 10)
                                additionalAttributes.Add("max", "10");
                            else
                                additionalAttributes.Add("max", choice.MaxOccurs.ToString());
                        }

                        #endregion choice
                    }

                    extra = xmlDatasetHelper.AddReferenceToXml(new XmlDocument(), "choice", "true", "elementType", @"extra/type", additionalAttributes);
                }

                MetadataNestedAttributeUsage usage = new MetadataNestedAttributeUsage()
                {
                    Label = element.Name,
                    Description = GetDescription(element.Annotation),
                    MinCardinality = min,
                    MaxCardinality = max,
                    Master = parent,
                    Member = compoundAttribute,
                    DefaultValue = element.DefaultValue,
                    FixedValue = element.FixedValue
                };

                if (extra.DocumentElement != null) usage.Extra = extra;

                #endregion choice

                parent.MetadataNestedAttributeUsages.Add(usage);
            }

            return parent;
        }

        private MetadataCompoundAttribute addMetadataAttributesToMetadataCompoundAttribute(MetadataCompoundAttribute compoundAttribute, List<XmlSchemaElement> elements, string internalXPath, string externalXPath)
        {
            for (int i = 0; i < elements.Count(); i++)
            {
                addMetadataAttributeToMetadataCompoundAttribute(compoundAttribute, elements.ElementAt(i), internalXPath, externalXPath);
            }

            return compoundAttribute;
        }

        private MetadataCompoundAttribute addMetadataAttributeToMetadataCompoundAttribute(MetadataCompoundAttribute compoundAttribute, XmlSchemaElement element, string internalXPath, string externalXPath)
        {
            MetadataAttributeManager metadataAttributeManager = new MetadataAttributeManager();

            try
            {
                MetadataAttribute attribute;

                if (metadataAttributeManager.MetadataAttributeRepo != null &&
                    getExistingMetadataAttribute(GetTypeOfName(element.Name)) != null)
                {
                    attribute = getExistingMetadataAttribute(GetTypeOfName(element.Name));
     
                }
                else
                {
                    attribute = createMetadataAttribute(element, internalXPath, externalXPath);
                }

                if (attribute != null)
                {
                    int min = 0;
                    if (element.MinOccurs > int.MinValue)
                    {
                        min = Convert.ToInt32(element.MinOccurs);
                    }
                    int max = 0;
                    if (element.MaxOccurs < int.MaxValue)
                        max = Convert.ToInt32(element.MaxOccurs);
                    else
                        max = int.MaxValue;

                    #region choice

                    //if element is a choise
                    XmlDocument extra = new XmlDocument();
                    //check if element is a choice
                    //ToDo Choice in XSD required Element

                    /*
                     * In some xsds elements in a choice are required.
                     * but the choice itself say at least one element should be selected
                     * and this means all childrens need to be optional
                     * and one must be selected
                     *                  *
                     */

                    if (XmlSchemaUtility.IsChoiceType(element))
                    {
                        min = 0;

                        Dictionary<string, string> additionalAttributes = new Dictionary<string, string>();

                        XmlSchemaComplexType ct = element.ElementSchemaType as XmlSchemaComplexType;

                        if (ct != null)
                        {
                            #region choice

                            // check if it is e choice
                            XmlSchemaChoice choice = ct.ContentTypeParticle as XmlSchemaChoice;
                            if (choice != null)
                            {
                                additionalAttributes.Add("min", choice.MinOccurs.ToString());
                                if (choice.MaxOccurs > 10)
                                    additionalAttributes.Add("max", "10");
                                else
                                    additionalAttributes.Add("max", choice.MaxOccurs.ToString());
                            }

                            #endregion choice
                        }

                        extra = xmlDatasetHelper.AddReferenceToXml(new XmlDocument(), "choice", "true", "elementType", @"extra/type", additionalAttributes);
                    }

                    #endregion choice

                    MetadataNestedAttributeUsage u1 = new MetadataNestedAttributeUsage()
                    {
                        Label = element.Name,
                        Description = attribute.Description,
                        MinCardinality = min,
                        MaxCardinality = max,
                        Master = compoundAttribute,
                        Member = attribute,
                        DefaultValue = element.DefaultValue,
                        FixedValue = element.FixedValue
                    };

                    if (extra.DocumentElement != null) u1.Extra = extra;

                    #region generate  MappingRoute

                    addToExportMappingFile(mappingFileInternalToExternal, internalXPath, externalXPath, element.MaxOccurs, element.Name, attribute.Name);
                    addToImportMappingFile(mappingFileExternalToInternal, externalXPath, internalXPath, element.MaxOccurs, element.Name, attribute.Name);

                    #endregion generate  MappingRoute

                    if (compoundAttribute.MetadataNestedAttributeUsages.Where(n => n.Label.Equals(attribute.Name)).Count() == 0)
                        compoundAttribute.MetadataNestedAttributeUsages.Add(u1);
                }

                return compoundAttribute;
            }
            finally
            {
                metadataAttributeManager.Dispose();
            }
        }


        private MetadataAttribute createMetadataAttribute(XmlSchemaElement element, string currentInternalXPath, string currentExternalXPath)
        {
            UnitManager unitManager = new UnitManager();
            MetadataAttributeManager metadataAttributeManager = new MetadataAttributeManager();
            DataTypeManager dataTypeManager = new DataTypeManager();

            try
            {
                MetadataAttribute temp = null;

                if (element.ElementSchemaType is XmlSchemaSimpleType)
                {
                    XmlSchemaSimpleType type = (XmlSchemaSimpleType)element.ElementSchemaType;

                    string name = GetTypeOfName(element.Name);
                    string description = "";

                    if (element.Annotation != null)
                    {
                        description = GetDescription(element.Annotation);
                    }
                    //datatype
                    string datatype = type.Datatype.ValueType.Name;
                    string typeCodename = type.Datatype.TypeCode.ToString();
                    DataType dataType = GetDataType(datatype, typeCodename);

                    //unit
                    Unit noneunit = unitManager.Repo.Query().Where(u => u.Name.ToLower().Equals("none")).FirstOrDefault();
                    if (noneunit == null)
                    {
                        Dimension dimension = null;

                        if (!unitManager.DimensionRepo.Get().Any(d => d.Name.Equals("none")))
                        {
                            dimension = unitManager.Create("none", "none", "If no unit is used."); // the null dimension should be replaced bz a proper valid one. Javad 11.06
                        }
                        else
                        {
                            dimension = unitManager.DimensionRepo.Query().Where(d => d.Name.Equals("none")).FirstOrDefault();
                        }

                        unitManager.Create("none", "none", "If no unit is used.", dimension, MeasurementSystem.Unknown);
                        // the null dimension should be replaced bz a proper valid one. Javad 11.06
                    }
                    temp = getExistingMetadataAttribute(name);// = metadataAttributeManager.MetadataAttributeRepo.Query().Where(m => m.Name.Equals(name)).FirstOrDefault();

                    if (temp == null)
                    {
                        temp = metadataAttributeManager.Create(name, name, description, false, false, "David Blaa", MeasurementScale.Categorial, DataContainerType.ValueType, "", dataType, noneunit, null, null, null, null);

                        //add constraints to the metadataAttribute
                        List<Constraint> constraints = convertToConstraints(type.Content, temp);
                        if (constraints != null && constraints.Count() > 0)
                            temp.Constraints = constraints;

                        //add to dic for reuse of created attributes
                        createdAttributesDic.Add(temp.Id, temp.Name);

                        //Debug.WriteLine(temp.Name);

                        return temp;

                        //return metadataAttributeManager.Update(temp);
                    }

                    //Debug.Writeline(temp.Name);

                    return temp;
                }

                if (element.ElementSchemaType is XmlSchemaComplexType)
                {
                    XmlSchemaComplexType type = (XmlSchemaComplexType)element.ElementSchemaType;
        
                    // load type if exist sepearte in the xsd 
                    var t = ComplexTypes.FirstOrDefault(c => c.Name.Equals(type.Name));
                    if (t!=null) type = t;

                    string name = GetTypeOfName(element.Name);
                    string description = "";

                    if (element.Annotation != null)
                    {
                        description = GetDescription(element.Annotation);
                    }
                    //datatype
                    string datatype = type.Datatype!=null? type.Datatype.ValueType.Name:"string";
                    string typeCodename = type.Datatype != null ? type.Datatype.TypeCode.ToString():"string";
                    DataType dataType = GetDataType(datatype, typeCodename);

                    //unit
                    // it is the second time I am seeing this cose segment, would be good to factor it out to a function
                    Unit noneunit = unitManager.Repo.Query().Where(u => u.Name.Equals("none")).FirstOrDefault();
                    if (noneunit == null)
                        unitManager.Create("none", "none", "If no unit is used.", null, MeasurementSystem.Unknown); // null diemsion to be replaced

                    temp = getExistingMetadataAttribute(name);// = metadataAttributeManager.MetadataAttributeRepo.Query().Where(m => m.Name.Equals(name)).FirstOrDefault();

                    if (temp == null)
                    {
                        temp = metadataAttributeManager.Create(name, name, description, false, false, "David Blaa", MeasurementScale.Categorial, DataContainerType.ValueType, "", dataType, noneunit, null, null, null, null);

                        //add to dic for reuse of created attributes
                        createdAttributesDic.Add(temp.Id, temp.Name);


                        XmlSchemaSimpleContentRestriction simpleContentRestriction;
                        List<Constraint> constraints = new List<Constraint>();

                        if (type.ContentModel != null)
                        {
                            if (type.ContentModel.Content is XmlSchemaSimpleContentRestriction)
                            {
                                simpleContentRestriction = (XmlSchemaSimpleContentRestriction)type.ContentModel.Content;
                                constraints = convertToConstraints(simpleContentRestriction, temp);
                            }

                            if (constraints != null && constraints.Count() > 0)
                                temp.Constraints = constraints;
                        }


                        // Add attributes as metadata packages

                        string internalPath = currentInternalXPath + "/" + element.Name + "/" + temp.Name;
                        string externalPath = currentExternalXPath + "/" + element.Name;

                        if (type.Attributes.Count > 0)
                        {
                            foreach (var attr in type.Attributes)
                            {
                                createMetadataParameterUsage(temp, attr, dataTypeManager, metadataAttributeManager);
                                addMetadataParameterToMappingFile(attr as XmlSchemaAttribute, internalPath, externalPath);

                            }
                        }

                        if (type.AttributeUses.Count > 0)
                        {
                            foreach (DictionaryEntry attr in type.AttributeUses)
                            {
                                createMetadataParameterUsage(temp, attr.Value, dataTypeManager, metadataAttributeManager);
                                addMetadataParameterToMappingFile( attr.Value as XmlSchemaAttribute, internalPath, externalPath);

                            }
                        }

                    }

                    return temp;
                    
                }

                return null;
            }
            finally
            {
                unitManager.Dispose();
                metadataAttributeManager.Dispose();
                dataTypeManager.Dispose();
            }
        }

        private MetadataCompoundAttribute createMetadataCompoundAttribute(XmlSchemaComplexType complexType)
        {
            DataTypeManager dataTypeManager = new DataTypeManager();

            try
            {
                // create a compoundAttribute
                MetadataCompoundAttribute mca = getExistingMetadataCompoundAttribute(complexType.Name);// = metadataAttributeManager.MetadataCompoundAttributeRepo.Get(p => p.Name == complexType.Name).FirstOrDefault();

                DataType dt1 = dataTypeManager.Repo.Get(p => p.Name.ToLower().Equals("string")).FirstOrDefault();
                if (dt1 == null)
                {
                    dt1 = dataTypeManager.Create("string", "A test String", System.TypeCode.String);
                }

                if (mca == null)
                {
                    mca = new MetadataCompoundAttribute
                    {
                        ShortName = complexType.Name,
                        Name = complexType.Name,
                        Description = GetDescription(complexType.Annotation),
                        DataType = dt1
                    };
                }

                return mca;
            }
            finally
            {
                dataTypeManager.Dispose();
            }
        }

        private MetadataCompoundAttribute createMetadataCompoundAttribute(XmlSchemaElement element)
        {
            DataTypeManager dataTypeManager = new DataTypeManager();

            try
            {
                // create a compoundAttribute
                int i = 0;
                MetadataCompoundAttribute mca = getExistingMetadataCompoundAttribute(element.Name + "Type"); ;// = metadataAttributeManager.MetadataCompoundAttributeRepo.Get(p => p.Name == element.Name+"Type").FirstOrDefault();
                                                                                                              //Debug.WriteLine("createMetadataCompoundAttribute" + i++);
                DataType dt1 = dataTypeManager.Repo.Get(p => p.Name.ToLower().Equals("string")).FirstOrDefault();
                if (dt1 == null)
                {
                    dt1 = dataTypeManager.Create("string", "A test String", System.TypeCode.String);
                }

                if (mca == null)
                {
                    mca = new MetadataCompoundAttribute
                    {
                        ShortName = GetTypeOfName(element.Name),
                        Name = GetTypeOfName(element.Name),
                        Description = "",
                        DataType = dt1
                    };
                }

                return mca;
            }
            finally
            {
                dataTypeManager.Dispose();
            }
        }

        private MetadataParameterUsage createMetadataParameterUsage(MetadataAttribute metadataAttribute, object xmlAttribute, DataTypeManager dataTypeManager, MetadataAttributeManager metadataAttributeManager)
        {
            string parameterUsageName = "";
            string description = "";
            List<Constraint> constraints = new List<Constraint>();
            MetadataParameter parameter;
            string defaultValue = "";
            string fixedValue = "";

            if (xmlAttribute is XmlSchemaAttribute)
            {
                var x = ((XmlSchemaAttribute)xmlAttribute);
                if (!string.IsNullOrEmpty(x.Name)) parameterUsageName = x.Name;
                if (!string.IsNullOrEmpty(x.DefaultValue)) defaultValue = x.DefaultValue;
                if (!string.IsNullOrEmpty(x.FixedValue)) defaultValue = x.FixedValue;

                xmlAttribute = x;

                // if xmlAttribute is null, load type from SimpleTypeList by SchemaTypeNameDefault
                if (xmlAttribute == null && !string.IsNullOrEmpty(x.SchemaTypeName.Name))
                {
                    xmlAttribute = SimpleTypes.Where(t => t.Name.Equals(x.SchemaTypeName.Name)).FirstOrDefault();
                }
                else
                // if xmlAttribute is null, load type from SimpleTypeList by RefName
                if (xmlAttribute == null && !string.IsNullOrEmpty(x.RefName.Name))
                {
                    xmlAttribute = SimpleTypes.Where(t => t.Name.Equals(x.SchemaTypeName.Name)).FirstOrDefault();
                }// if xmlAttribute is null, load type from SimpleTypeList by SchemaTypeNameDefault


            }

            XmlSchemaSimpleType xmlSchemaSimpleType = null;

            if (xmlAttribute is XmlSchemaSimpleType) xmlSchemaSimpleType = (XmlSchemaSimpleType)xmlAttribute;
            else if (xmlAttribute is XmlSchemaAttribute) xmlSchemaSimpleType = ((XmlSchemaAttribute)xmlAttribute).AttributeSchemaType;

            if (xmlSchemaSimpleType != null)
            {
                
                if (string.IsNullOrEmpty(parameterUsageName) && xmlSchemaSimpleType.Name != null) parameterUsageName = xmlSchemaSimpleType.Name;

                if (xmlSchemaSimpleType.Annotation != null) description = GetDescription(xmlSchemaSimpleType.Annotation);

                parameter = createMetadataParameter(metadataAttribute, parameterUsageName, description, xmlSchemaSimpleType.Datatype);
                if (parameter != null)
                {
                    // if not exist then create in db
                    if (parameter.Id == 0)
                    {
                        // create
                        parameter = metadataAttributeManager.Create(parameter);

                        constraints = convertToConstraints(xmlSchemaSimpleType.Content, parameter);
                        parameter.Constraints = constraints;

                        createdParametersDic.Add(parameter.Id, parameter.Name);
                    }

                    if (parameter != null)
                    {
                        // create parameter Usage beweteen MetadataAttribute and Parameter
                        metadataAttributeManager.AddParameterUsage(metadataAttribute, parameter, true, defaultValue, fixedValue);
                    }
                }
                

            }
            else // catch all ref attributes with only name info and without type info in schema
                 // as default set to string with no restictions & optional
            {
                if (xmlAttribute is XmlSchemaAttribute)
                {
                    var xAttr = ((XmlSchemaAttribute)xmlAttribute);

                    if (!String.IsNullOrEmpty(xAttr.Name)) parameterUsageName = xAttr.Name;
                    else if (xAttr.RefName != null && !String.IsNullOrEmpty(xAttr.RefName.Name)) parameterUsageName = xAttr.RefName.Name;
                    else if (xAttr.QualifiedName != null && !String.IsNullOrEmpty(xAttr.QualifiedName.Name)) parameterUsageName = xAttr.QualifiedName.Name;
                    parameter = createMetadataParameter(metadataAttribute, parameterUsageName, "", null);

                    // if not exist then create in db
                    if (parameter.Id == 0)
                    {
                        // create
                        parameter = metadataAttributeManager.Create(parameter);

                        createdParametersDic.Add(parameter.Id, parameter.Name);
                    }

                    if (parameter != null)
                    {
                        // create parameter Usage beweteen MetadataAttribute and Parameter 
                        metadataAttributeManager.AddParameterUsage(metadataAttribute, parameter, false, xAttr.DefaultValue, xAttr.FixedValue);
                    }
                }
            }

            return null;
        }
        private MetadataParameter createMetadataParameter(MetadataAttribute metadataAttribute, string name,string description, XmlSchemaDatatype type)
        {
            if (string.IsNullOrEmpty(name)) return null;
     

            string datatype = type==null?"string":type?.ValueType.Name;
            string typeCodename = type == null ? "string": type?.TypeCode.ToString();
            DataType dataType = GetDataType(datatype, typeCodename);

            MetadataParameter parameter = getExistingMetadataParameter(name); ;

            if(parameter == null)
            {

                parameter = new MetadataParameter()
                {
                    ShortName = name,
                    Name = name,
                    Description = description,
                    DataType = dataType,
                    
                };

            }



            return parameter;
        }

        #endregion metadata structure

        #region mapping file

        private XmlDocument generateXmlMappingFile(XmlMapper mapperInfos, string sourceName, string DestinationName, int direction = 0)
        {
            XmlDocument mappingFile = new XmlDocument();

            // create root
            XmlNode root = XmlUtility.CreateNode("mapping", mappingFile);
            root = XmlUtility.AddAttribute(root, "name", SchemaName, mappingFile);
            mappingFile.AppendChild(root);

            //  create header

            XmlNode header = XmlUtility.CreateNode("header", mappingFile);
            XmlMappingHeader xmlMapperheader = mapperInfos.Header;

            // create destination

            XmlNode destination = XmlUtility.CreateNode("destination", mappingFile);

            if (xmlMapperheader.Destination != null)
            {
                destination = XmlUtility.AddAttribute(destination, "namespaceUri", xmlMapperheader.Destination.NamepsaceURI, mappingFile);
                destination = XmlUtility.AddAttribute(destination, "prefix", xmlMapperheader.Destination.Prefix, mappingFile);
                destination = XmlUtility.AddAttribute(destination, "sequence", xmlMapperheader.Destination.ParentSequence, mappingFile);
                destination = XmlUtility.AddAttribute(destination, "xPath", xmlMapperheader.Destination.XPath, mappingFile);
            }
            header.AppendChild(destination);

            // create attributes
            XmlNode attributes = XmlUtility.CreateNode("attributes", mappingFile);

            if (mapperInfos.Header.Attributes.Count > 0)
            {
                foreach (var attr in mapperInfos.Header.Attributes)
                {
                    XmlNode attribute = XmlUtility.CreateNode("attribute", mappingFile);
                    attribute = XmlUtility.AddAttribute(attribute, "name", attr.Key, mappingFile);
                    attribute = XmlUtility.AddAttribute(attribute, "value", attr.Value, mappingFile);

                    attributes.AppendChild(attribute);
                }
            }

            header.AppendChild(attributes);

            // create packages
            XmlNode packages = XmlUtility.CreateNode("packages", mappingFile);

            if (mapperInfos.Header.Packages.Count > 0)
            {
                foreach (var pack in mapperInfos.Header.Packages)
                {
                    XmlNode package = XmlUtility.CreateNode("package", mappingFile);
                    package = XmlUtility.AddAttribute(package, "abbreviation", pack.Key, mappingFile);
                    package = XmlUtility.AddAttribute(package, "url", pack.Value, mappingFile);

                    packages.AppendChild(package);
                }
            }
            header.AppendChild(packages);

            // create schema

            if (mapperInfos.Header.Schemas.Count > 0)
            {
                foreach (var s in mapperInfos.Header.Schemas)
                {
                    XmlNode schema = XmlUtility.CreateNode("schema", mappingFile);
                    schema = XmlUtility.AddAttribute(schema, "abbreviation", s.Key, mappingFile);
                    schema = XmlUtility.AddAttribute(schema, "url", s.Value, mappingFile);

                    header.AppendChild(schema);
                }
            }

            root.AppendChild(header);

            // create routes

            XmlNode routes = XmlUtility.CreateNode("routes", mappingFile);

            if (mapperInfos.Routes.Count > 0)
            {
                foreach (XmlMappingRoute mapRoute in mapperInfos.Routes)
                {
                    // create route
                    XmlNode route = XmlUtility.CreateNode("route", mappingFile);

                    // create source
                    XmlNode Source = XmlUtility.CreateNode("source", mappingFile);
                    Source = XmlUtility.AddAttribute(Source, "xPath", mapRoute.Source.XPath, mappingFile);

                    route.AppendChild(Source);
                    // create destination
                    XmlNode Destination = XmlUtility.CreateNode("destination", mappingFile);
                    Destination = XmlUtility.AddAttribute(Destination, "xPath", mapRoute.Destination.XPath, mappingFile);
                    Destination = XmlUtility.AddAttribute(Destination, "sequence", mapRoute.Destination.ParentSequence, mappingFile);

                    route.AppendChild(Destination);
                    routes.AppendChild(route);
                }
            }

            root.AppendChild(routes);
            string path = "";
            if (direction == 0)
            {
                mappingFileNameExport = "MappingFile_intern_" + sourceName + "_to_extern_" + DestinationName + ".xml";
                path = Path.Combine(AppConfiguration.GetModuleWorkspacePath("DIM"), mappingFileNameExport);
            }
            else
            {
                mappingFileNameImport = "MappingFile_extern_" + sourceName + "_to_intern_" + DestinationName + ".xml";
                path = Path.Combine(AppConfiguration.GetModuleWorkspacePath("DIM"), mappingFileNameImport);
            }

             
            if(File.Exists(path)) File.Delete(path);
            mappingFile.Save(path);

            return mappingFile;
        }

        private void addMetadataAttributeToMappingFile(MetadataCompoundAttribute compoundAttribute, XmlSchemaElement element, string internalXPath, string externalXPath)
        {
            MetadataAttributeManager metadataAttributeManager = new MetadataAttributeManager();

            try
            {
                MetadataAttribute attribute;

                if (metadataAttributeManager.MetadataAttributeRepo != null &&
                        metadataAttributeManager.MetadataAttributeRepo.Query().Where(m => m.Name.Equals(GetTypeOfName(element.Name))).Count() > 0)
                {
                    attribute = metadataAttributeManager.MetadataAttributeRepo.Query().Where(m => m.Name.Equals(GetTypeOfName(element.Name))).First();
                }
                else
                {
                    attribute = createMetadataAttribute(element, internalXPath, externalXPath);
                }

                #region generate  MappingRoute

                addToExportMappingFile(mappingFileInternalToExternal, internalXPath, externalXPath, element.MaxOccurs, element.Name, attribute.Name);
                addToImportMappingFile(mappingFileExternalToInternal, externalXPath, internalXPath, element.MaxOccurs, element.Name, attribute.Name);

                #endregion generate  MappingRoute

                //Debug.WriteLine("SimpleAttribute :");
                //Debug.WriteLine("--- internal :" + childInternalXPath);
                //Debug.WriteLine("--- external :" + childExternalXPath);
                //Debug.WriteLine("--- sequence :" + element);
            }
            finally
            {
                metadataAttributeManager.Dispose();
            }
        }

        private void addMetadataParameterToMappingFile(XmlSchemaAttribute attr, string internalXPath, string externalXPath)
        {
            addParameterToExportMappingFile(mappingFileInternalToExternal, internalXPath, externalXPath,attr.Name);
            addParameterToImportMappingFile(mappingFileExternalToInternal, externalXPath, internalXPath, attr.Name);

        }

        private XmlMapper addToImportMappingFile(XmlMapper mapper, string sourceXPath, string destinationXPath, decimal max, string name, string nameType)
        {
            string parentExternalName = destinationXPath.Split('/').Last();
            string childSourceXPath = sourceXPath + "/" + name;
            string childDestinationXPath = destinationXPath + "/" + name + "/" + nameType;

            XmlMappingRoute xmr = new XmlMappingRoute();

            xmr.Source = new Source(childSourceXPath);

            string sequence = parentExternalName;
            if (max > 1)
            {
                sequence = nameType;
            }

            xmr.Destination = new Destination(childDestinationXPath, sequence);

            mapper.Routes.Add(xmr);

            return mapper;
        }

        private XmlMapper addToExportMappingFile(XmlMapper mapper, string sourceXPath, string destinationXPath, decimal max, string name, string nameType)
        {
            string parentExternalName = destinationXPath.Split('/').Last();
            string childSourceXPath = sourceXPath + "/" + name + "/" + nameType;
            string childDestinationXPath = destinationXPath + "/" + name;

            XmlMappingRoute xmr = new XmlMappingRoute();

            xmr.Source = new Source(childSourceXPath);

            string sequence = parentExternalName;
            if (max > 1)
            {
                sequence = name;
            }

            xmr.Destination = new Destination(childDestinationXPath, sequence);

            mapper.Routes.Add(xmr);

            return mapper;
        }

        private XmlMapper addParameterToImportMappingFile(XmlMapper mapper, string sourceXPath, string destinationXPath, string name)
        {
            string childSourceXPath = sourceXPath + "/@" + name;
            string childDestinationXPath = destinationXPath + "/@" + name;

            XmlMappingRoute xmr = new XmlMappingRoute();

            xmr.Source = new Source(childSourceXPath);
            xmr.Destination = new Destination(childDestinationXPath, "");

            mapper.Routes.Add(xmr);

            return mapper;
        }

        private XmlMapper addParameterToExportMappingFile(XmlMapper mapper, string sourceXPath, string destinationXPath, string name)
        {
            string childSourceXPath = sourceXPath + "/@" + name;
            string childDestinationXPath = destinationXPath + "/@" + name;

            XmlMappingRoute xmr = new XmlMappingRoute();

            xmr.Source = new Source(childSourceXPath);
            xmr.Destination = new Destination(childDestinationXPath,"");

            mapper.Routes.Add(xmr);

            return mapper;
        }

        #endregion mapping file

        #region helper functions

        private MetadataAttribute getExistingMetadataAttribute(string name)
        {
            MetadataAttributeManager metadataAttributeManager = new MetadataAttributeManager();

            try
            {
                if (createdAttributesDic.ContainsValue(name))
                {
                    long id = createdAttributesDic.Where(k => k.Value.Equals(name)).FirstOrDefault().Key;
                    return metadataAttributeManager.MetadataSimpleAttributeRepo.Get(id);
                }

                return null;
            }
            finally
            {
                metadataAttributeManager.Dispose();
            }
        }

        private MetadataCompoundAttribute getExistingMetadataCompoundAttribute(string name)
        {
            MetadataAttributeManager metadataAttributeManager = new MetadataAttributeManager();

            try
            {
                if (createdCompoundsDic.ContainsValue(name))
                {
                    long id = createdCompoundsDic.Where(k => k.Value.Equals(name)).FirstOrDefault().Key;
                    return metadataAttributeManager.MetadataCompoundAttributeRepo.Get(id);
                }
                return null;
            }
            finally
            {
                metadataAttributeManager.Dispose();
            }
        }

        private MetadataParameter getExistingMetadataParameter(string name)
        {
            using(MetadataAttributeManager metadataAttributeManager = new MetadataAttributeManager())
            {
                if (createdParametersDic.ContainsValue(name))
                {
                    long id = createdParametersDic.Where(k => k.Value.Equals(name)).FirstOrDefault().Key;
                    return metadataAttributeManager.MetadataParameterRepo.Get(id);
                }
                return null;
   
            }
        }

        private MetadataPackage getExistingMetadataPackage(string name)
        {
            MetadataPackageManager metadataPackageManager = new MetadataPackageManager();

            try
            {
                if (createdPackagesDic.ContainsValue(name))
                {
                    long id = createdPackagesDic.Where(k => k.Value.Equals(name)).FirstOrDefault().Key;
                    return metadataPackageManager.MetadataPackageRepo.Get(id);
                }
                return null;
            }
            finally
            {
                metadataPackageManager.Dispose();
            }
        }

        // vielleicht besser mit festen datatypes im system
        private DataType GetDataType(string dataTypeAsString, string typeCodeName)
        {
            // if nothing exist then use string as datatype
            if (string.IsNullOrEmpty(dataTypeAsString) && string.IsNullOrEmpty(typeCodeName)) return GetDataType("string", "");

            // in xsd defined xs:integer has value type decimal, xs:int = int32
            // so we need to overright the decimal to integer 
            if (typeCodeName.ToLower().Equals("integer") && dataTypeAsString.ToLower().Equals("decimal")) dataTypeAsString = "int64";


            DataTypeManager dataTypeManager = new DataTypeManager();
            try
            {
                if (!dataTypeAsString.ToLower().Equals("Object"))
                {
                    TypeCode typeCode = ConvertStringToSystemType(dataTypeAsString);
                    DataType dataType = null;
                    // if datatime - need to check typeCodeName for date, time , datetime

                    string name = typeCode.ToString();

                    if (dataTypeAsString.Equals(TypeCode.DateTime.ToString()))
                    {
                        name = typeCodeName;

                        dataType =
                            dataTypeManager.Repo.Query()
                                .Where(
                                    d => d.SystemType.Equals(typeCode.ToString()) &&
                                    d.Name.ToLower().Equals(name.ToLower())
                                    )
                                .FirstOrDefault();
                    }
                    else
                        dataType =
                            dataTypeManager.Repo.Query()
                                .Where(
                                    d =>
                                        d.SystemType.Equals(typeCode.ToString()) &&
                                        d.Name.ToLower().Equals(name.ToString().ToLower()))
                                .FirstOrDefault();

                    if (dataType == null)
                    {
                        dataType = dataTypeManager.Create(name.ToLower(), typeCode.ToString(),
                            typeCode);
                    }

                    return dataType;
                }
                else
                {
                    return GetDataType("string", "");
                }
            }
            finally
            {
                dataTypeManager.Dispose();
            }
        }

        //convert string to systemtype
        private TypeCode ConvertStringToSystemType(string dataType)
        {
            foreach (TypeCode tc in Enum.GetValues(typeof(TypeCode)))
            {
                if (tc.ToString().ToLower().Equals(dataType.ToLower())) return tc;
            }

            return TypeCode.String;
        }

        private string GetDescription(XmlSchemaAnnotation annotation)
        {
            string description = "";

            if (annotation != null)
            {
                foreach (var item in annotation.Items)
                {
                    if (item is XmlSchemaDocumentation)
                    {
                        XmlSchemaDocumentation documentation = (XmlSchemaDocumentation)item;

                        foreach (XmlNode node in documentation.Markup)
                        {
                            description += node.InnerText;
                        }
                    }
                }

                // if (description.Length > 250)
                // {
                //     description = description.Substring(0, 250);
                // }
            }

            return description;
        }

        private void checkDirectory(string filePath)
        {
            string path = Path.GetDirectoryName(filePath);

            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }

        private string GetTypeOfName(string name)
        {
            return name + "Type";
        }

        #endregion helper functions

        private List<Constraint> convertToConstraints(XmlSchemaObject restriction, MetadataAttribute attr)
        {

            List<Constraint> constraints = new List<Constraint>();

            /// XmlSchemaSimpleTypeRestriction
            if (restriction is XmlSchemaSimpleTypeRestriction)
            {
                XmlSchemaSimpleTypeRestriction simpleTypeRestriction = (XmlSchemaSimpleTypeRestriction)restriction;

                // if id is null, this restirctiion is based on a other simple type and need to set from the SimpleTypes
                if (simpleTypeRestriction.Id == null)
                {
                    var baseType = SimpleTypes.Where(t => t.Name.Equals(simpleTypeRestriction.BaseTypeName.Name)).FirstOrDefault();
                    if(baseType!=null )simpleTypeRestriction = baseType.Content as XmlSchemaSimpleTypeRestriction;
                }
               
                // if content of simpletype is a restriction
                if (restriction != null && simpleTypeRestriction !=null)
                {
                    // if the simpletype is a domin constraint
                    if (XmlSchemaUtility.IsEnumerationType(simpleTypeRestriction))
                    {
                        constraints.Add(GetDomainConstraint(simpleTypeRestriction, attr, GetDescription(simpleTypeRestriction.Annotation)));
                    }
                    else
                    {
                        foreach (XmlSchemaObject facet in simpleTypeRestriction.Facets)
                        {
                            Constraint c = convertFacetToConstraint(facet, attr, constraints);
                            if (c != null)
                                constraints.Add(c);
                        }
                    }
                }
            }

            /// XmlSchemaSimpleContentRestriction
            if (restriction is XmlSchemaSimpleContentRestriction)
            {
                XmlSchemaSimpleContentRestriction simpleContentRestriction = (XmlSchemaSimpleContentRestriction)restriction;

                // if id is null, this restirctiion is based on a other simple type and need to set from the SimpleTypes
                if (simpleContentRestriction.Id == null)
                {
                    var baseType = SimpleTypes.Where(t => t.Name.Equals(simpleContentRestriction.BaseTypeName.Name)).FirstOrDefault();

                }

                // if content of simpletype is a restriction
                if (restriction != null && simpleContentRestriction != null)
                {
                    // if the simpletype is a domin constraint
                    if (XmlSchemaUtility.IsEnumerationType(simpleContentRestriction))
                    {
                        constraints.Add(GetDomainConstraint(simpleContentRestriction, attr, GetDescription(simpleContentRestriction.Annotation)));
                    }
                    else
                    {
                        foreach (XmlSchemaObject facet in simpleContentRestriction.Facets)
                        {
                            Constraint c = convertFacetToConstraint(facet, attr, constraints);
                            if (c != null)
                                constraints.Add(c);
                        }
                    }
                }
            }

            return constraints;
        }

        /// <summary>
        ///  length, minLength,maxLength,pattern,enumeration,whiteSpace,maxInclusive,maxExclusive,minExclusive,minInclusive,totalDigits,fractionDigits
        /// </summary>
        /// <param name="facet"></param>
        /// <returns></returns>
        private Constraint convertFacetToConstraint(XmlSchemaObject facet, MetadataAttribute attr, List<Constraint> constraints)
        {
            DataContainerManager dataContainerManager = new DataContainerManager();
            try
            {
                #region pattern constraints

                //if (facet is XmlSchemaWhiteSpaceFacet)
                //{
                //    XmlSchemaWhiteSpaceFacet defFacet = (XmlSchemaWhiteSpaceFacet)facet;

                //    return GetPatternConstraint(" ", GetDescription(defFacet.Annotation), true, attr);
                //}

                if (facet is XmlSchemaPatternFacet)
                {
                    XmlSchemaPatternFacet defFacet = (XmlSchemaPatternFacet)facet;
                    return GetPatternConstraint(defFacet.Value, GetDescription(defFacet.Annotation), false, attr);
                }

                #endregion pattern constraints

                #region range constraints

                if (facet is XmlSchemaLengthFacet)
                {
                    XmlSchemaLengthFacet defFacet = (XmlSchemaLengthFacet)facet;

                    if (!IsRangeExist(constraints))
                    {
                        return GetRangeConstraint(Convert.ToDouble(defFacet.Value), Convert.ToDouble(defFacet.Value), GetDescription(defFacet.Annotation), false, true, true, attr);
                    }
                    else
                    {
                        double value = Convert.ToDouble(defFacet.Value);
                        RangeConstraint rc = GetRangeConstraint(constraints);
                        dataContainerManager.RemoveConstraint(rc);
                        // if value is bigger then Lowerbound us value
                        if (rc.Lowerbound < value) rc.Lowerbound = value;

                        // if value is smaller then Upperbound  us value
                        if (rc.Upperbound > value) rc.Upperbound = value;

                        rc.LowerboundIncluded = true;
                        rc.UpperboundIncluded = true;

                        constraints.Remove(rc);

                        return GetRangeConstraint(rc.Lowerbound, rc.Upperbound, GetDescription(defFacet.Annotation), false, rc.LowerboundIncluded, rc.UpperboundIncluded, attr);
                    }
                }

                if (facet is XmlSchemaMinLengthFacet)
                {
                    XmlSchemaMinLengthFacet defFacet = (XmlSchemaMinLengthFacet)facet;

                    if (!IsRangeExist(constraints))
                    {
                        return GetRangeConstraint(Convert.ToDouble(defFacet.Value), double.MaxValue, GetDescription(defFacet.Annotation), false, true, true, attr);
                    }
                    else
                    {
                        double value = Convert.ToDouble(defFacet.Value);
                        RangeConstraint rc = GetRangeConstraint(constraints);

                        dataContainerManager.RemoveConstraint(rc);

                        // if value is bigger then Lowerbound us value
                        if (rc.Lowerbound < value) rc.Lowerbound = value;

                        rc.LowerboundIncluded = true;

                        constraints.Remove(rc);

                        return GetRangeConstraint(rc.Lowerbound, rc.Upperbound, GetDescription(defFacet.Annotation), false, rc.LowerboundIncluded, rc.UpperboundIncluded, attr);
                    }
                }

                if (facet is XmlSchemaMaxLengthFacet)
                {
                    XmlSchemaMaxLengthFacet defFacet = (XmlSchemaMaxLengthFacet)facet;

                    if (!IsRangeExist(constraints))
                    {
                        return GetRangeConstraint(double.MinValue, Convert.ToDouble(defFacet.Value), GetDescription(defFacet.Annotation), false, true, true, attr);
                    }
                    else
                    {
                        double value = Convert.ToDouble(defFacet.Value);
                        RangeConstraint rc = GetRangeConstraint(constraints);
                        dataContainerManager.RemoveConstraint(rc);

                        // if value is smaller then Upperbound  us value
                        if (rc.Upperbound > value) rc.Upperbound = value;

                        rc.UpperboundIncluded = true;

                        constraints.Remove(rc);

                        return GetRangeConstraint(rc.Lowerbound, rc.Upperbound, GetDescription(defFacet.Annotation), false, rc.LowerboundIncluded, rc.UpperboundIncluded, attr);
                    }
                }

                if (facet is XmlSchemaMaxInclusiveFacet)
                {
                    XmlSchemaMaxInclusiveFacet defFacet = (XmlSchemaMaxInclusiveFacet)facet;

                    if (!IsRangeExist(constraints))
                    {
                        return GetRangeConstraint(0, Convert.ToDouble(defFacet.Value), GetDescription(defFacet.Annotation), false, true, true, attr);
                    }
                    else
                    {
                        double value = Convert.ToDouble(defFacet.Value);
                        RangeConstraint rc = GetRangeConstraint(constraints);
                        dataContainerManager.RemoveConstraint(rc);

                        // if value is smaller then Upperbound  us value
                        if (rc.Upperbound > value) rc.Upperbound = value;

                        rc.UpperboundIncluded = true;

                        constraints.Remove(rc);

                        return GetRangeConstraint(rc.Lowerbound, rc.Upperbound, GetDescription(defFacet.Annotation), false, rc.LowerboundIncluded, rc.UpperboundIncluded, attr);
                    }
                }

                if (facet is XmlSchemaMinInclusiveFacet)
                {
                    XmlSchemaMinInclusiveFacet defFacet = (XmlSchemaMinInclusiveFacet)facet;
                    if (!IsRangeExist(constraints))
                    {
                        return GetRangeConstraint(Convert.ToDouble(defFacet.Value), int.MaxValue, GetDescription(defFacet.Annotation), false, true, true, attr);
                    }
                    else
                    {
                        double value = Convert.ToDouble(defFacet.Value);
                        RangeConstraint rc = GetRangeConstraint(constraints);
                        dataContainerManager.RemoveConstraint(rc);

                        // if value is bigger then Lowerbound us value
                        if (rc.Lowerbound < value) rc.Lowerbound = value;

                        rc.LowerboundIncluded = true;

                        constraints.Remove(rc);

                        return GetRangeConstraint(rc.Lowerbound, rc.Upperbound, GetDescription(defFacet.Annotation), false, rc.LowerboundIncluded, rc.UpperboundIncluded, attr);
                    }
                }

                if (facet is XmlSchemaMaxExclusiveFacet)
                {
                    XmlSchemaMaxExclusiveFacet defFacet = (XmlSchemaMaxExclusiveFacet)facet;

                    if (!IsRangeExist(constraints))
                    {
                        return GetRangeConstraint(0, Convert.ToDouble(defFacet.Value), GetDescription(defFacet.Annotation), false, true, false, attr);
                    }
                    else
                    {
                        double value = Convert.ToDouble(defFacet.Value);
                        RangeConstraint rc = GetRangeConstraint(constraints);
                        dataContainerManager.RemoveConstraint(rc);

                        // if value is smaller then Upperbound  us value
                        if (rc.Upperbound > value) rc.Upperbound = value;

                        rc.UpperboundIncluded = false;

                        constraints.Remove(rc);

                        return GetRangeConstraint(rc.Lowerbound, rc.Upperbound, GetDescription(defFacet.Annotation), false, rc.LowerboundIncluded, rc.UpperboundIncluded, attr);
                    }
                }

                if (facet is XmlSchemaMinExclusiveFacet)
                {
                    XmlSchemaMinExclusiveFacet defFacet = (XmlSchemaMinExclusiveFacet)facet;
                    if (!IsRangeExist(constraints))
                    {
                        return GetRangeConstraint(Convert.ToDouble(defFacet.Value), int.MaxValue, GetDescription(defFacet.Annotation), false, false, true, attr);
                    }
                    else
                    {
                        double value = Convert.ToDouble(defFacet.Value);
                        RangeConstraint rc = GetRangeConstraint(constraints);
                        dataContainerManager.RemoveConstraint(rc);
                        // if value is bigger then Lowerbound us value
                        if (rc.Lowerbound < value) rc.Lowerbound = value;

                        rc.LowerboundIncluded = false;

                        constraints.Remove(rc);

                        return GetRangeConstraint(rc.Lowerbound, rc.Upperbound, GetDescription(defFacet.Annotation), false, rc.LowerboundIncluded, rc.UpperboundIncluded, attr);
                    }
                }

                #endregion range constraints

                if (facet is XmlSchemaTotalDigitsFacet)
                {
                }

                if (facet is XmlSchemaFractionDigitsFacet)
                {
                }

                /* special case
                if (facet is XmlSchemaEnumerationFacet)
                {
                    return GetDomainConstraint((XmlSchemaEnumerationFacet)facet);
                }
                 * */

                return null;
            }
            finally
            {
                dataContainerManager.Dispose();
            }
        }

        #region constraints

        private DomainConstraint GetDomainConstraint(XmlSchemaObject restriction, MetadataAttribute attr, string restrictionDescription)
        {
            DataContainerManager dataContainerManager = new DataContainerManager();
            try
            {
                XmlSchemaObjectCollection facets = new XmlSchemaObjectCollection();

                if (restriction is XmlSchemaSimpleTypeRestriction)
                    facets = ((XmlSchemaSimpleTypeRestriction)restriction).Facets;

                if (restriction is XmlSchemaSimpleContentRestriction)
                    facets = ((XmlSchemaSimpleContentRestriction)restriction).Facets;

                List<DomainItem> items = new List<DomainItem>();

                foreach (XmlSchemaEnumerationFacet facet in facets)
                {
                    if (facet != null)
                        items.Add(new DomainItem() { Key = facet.Value, Value = facet.Value });
                }

                DomainConstraint domainConstraint = new DomainConstraint(ConstraintProviderSource.Internal, "", "en-US", restrictionDescription, false, null, null, null, items);

                domainConstraint.Materialize();
                dataContainerManager.AddConstraint(domainConstraint, attr);

                return domainConstraint;
            }
            finally
            {
                dataContainerManager.Dispose();
            }
        }

        private RangeConstraint GetRangeConstraint(double min, double max, string description, bool negated, bool lowerBoundIncluded, bool upperBoundIncluded, MetadataAttribute attr)
        {
            DataContainerManager dataContainerManager = new DataContainerManager();
            try
            {
                RangeConstraint constraint = new RangeConstraint(ConstraintProviderSource.Internal, "", "en-US", description, negated, null, null, null, min, lowerBoundIncluded, max, upperBoundIncluded);
                dataContainerManager.AddConstraint(constraint, attr);

                return constraint;
            }
            finally
            {
                dataContainerManager.Dispose();
            }
        }

        private PatternConstraint GetPatternConstraint(string patternString, string description, bool negated, MetadataAttribute attr)
        {
            DataContainerManager dataContainerManager = new DataContainerManager();
            try
            {
                PatternConstraint constraint = new PatternConstraint(ConstraintProviderSource.Internal, "", "en-US", description, negated, null, null, null, patternString, false);
                dataContainerManager.AddConstraint(constraint, attr);

                return constraint;
            }
            finally
            {
                dataContainerManager.Dispose();
            }
        }

        private bool IsRangeExist(List<Constraint> constraints)
        {
            return constraints.Any(c => c is RangeConstraint);
        }

        private RangeConstraint GetRangeConstraint(List<Constraint> constraints)
        {
            return constraints.Where(c => c is RangeConstraint).FirstOrDefault() as RangeConstraint;
        }

        #endregion constraints

        #endregion import To MetadatStructure

        #region delete Schema

        /// <summary>
        /// Delete all depending xsdFiles under the workspace
        /// && all generated mapping files
        /// </summary>
        /// <param name="metadataStructure"></param>
        /// <returns></returns>
        public static bool Delete(MetadataStructure metadataStructure)
        {
            string directoryPath = Path.Combine(AppConfiguration.GetModuleWorkspacePath("DCM"), "Metadata",
                metadataStructure.Name);

            string mappingFileDirectory = AppConfiguration.GetModuleWorkspacePath("DIM");

            XmlDatasetHelper xmlDatasetHelper = new XmlDatasetHelper();

            // delete all mapping files
            // delete export mappings
            List<string> mappingFilPaths =
                xmlDatasetHelper.GetAllTransmissionInformationFromMetadataStructure(metadataStructure.Id,
                    TransmissionType.mappingFileExport, AttributeNames.value).ToList();

            if (mappingFilPaths.Count > 0)
            {
                foreach (var file in mappingFilPaths)
                {
                    FileHelper.Delete(Path.Combine(mappingFileDirectory, file));
                }
            }
            // delete import mappings
            mappingFilPaths =
                    xmlDatasetHelper.GetAllTransmissionInformationFromMetadataStructure(metadataStructure.Id,
                        TransmissionType.mappingFileImport, AttributeNames.value).ToList();

            if (mappingFilPaths.Count > 0)
            {
                foreach (var file in mappingFilPaths)
                {
                    FileHelper.Delete(Path.Combine(mappingFileDirectory, file));
                }
            }

            // deleting all xsds
            if (Directory.Exists(directoryPath))
                Directory.Delete(directoryPath, true);

            if (!Directory.Exists(directoryPath)) return true;

            return false;
        }

        #endregion delete Schema
    }
}