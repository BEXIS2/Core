using BExIS.Dlm.Entities.Administration;
using BExIS.Dlm.Entities.DataStructure;
using BExIS.Dlm.Entities.MetadataStructure;
using BExIS.Dlm.Services.Administration;
using BExIS.Dlm.Services.DataStructure;
using BExIS.Dlm.Services.MetadataStructure;
using BExIS.Web.Shell.Areas.RPM.Helpers.SeedData;
using BExIS.Xml.Helpers.Mapping;
using BExIS.Xml.Services;
using System;
using System.Data;
using System.IO;
using System.Linq;
using System.Xml;
using Vaiona.Utils.Cfg;

namespace BExIS.Web.Shell.Areas.RPM.Helpers
{
    public class RPMSeedDataGenerator
    {

        public static void GenerateSeedData()
        {
            //create seed data from csv files
            MappingReader mappingReader = new MappingReader();
            AttributeCreator attributeCreator = new AttributeCreator();
            string filePath = AppConfiguration.GetModuleWorkspacePath("RPM");

            // read data types from csv file
            DataTable mappedDataTypes = mappingReader.readDataTypes(filePath);
            // create read data types in bpp
            attributeCreator.CreateDataTypes(ref mappedDataTypes);

            //// read dimensions from csv file
            DataTable mappedDimensions = mappingReader.readDimensions(filePath);
            // create dimensions in bpp
            attributeCreator.CreateDimensions(ref mappedDimensions);

            //// read units from csv file
            DataTable mappedUnits = mappingReader.readUnits(filePath);
            // create read units in bpp
            attributeCreator.CreateUnits(ref mappedUnits);

            //// read attributes from csv file
            DataTable mappedAttributes = mappingReader.readAttributes(filePath);
            // free memory
            mappedDataTypes.Clear();
            mappedDimensions.Clear();
            // create read attributes in bpp
            attributeCreator.CreateAttributes(ref mappedAttributes);

            createResearchPlan();
            //createSeedDataTypes();
            //createSIUnits();
            //createEmlDatasetAdv();
            //createABCD();


            ImportSchema("Basic ABCD", "ABCD_2.06.XSD", "Dataset", "BExIS.Dlm.Entities.Data.Dataset");
            //ImportSchema("Basic Eml", "eml.xsd","dataset","BExIS.Dlm.Entities.Data.Dataset");
        }

        private static void createResearchPlan()
        {
            //ResearchPlan
            ResearchPlanManager rpm = new ResearchPlanManager();
            ResearchPlan researchPlan = rpm.Repo.Get(r => r.Title.Equals("Research plan")).FirstOrDefault();
            if (researchPlan == null) rpm.Create("Research plan", "");

        }

        #region old seed data
        //private static void createSeedDataTypes()
        //{
        //    DataTypeManager dataTypeManager = new DataTypeManager();
        //    DataType dataType;

        //    try
        //    {
        //        string path = Path.Combine(AppConfiguration.GetModuleWorkspacePath("RPM"), "Seed", "DataTypes.xml");
        //        XDocument xdoc = XDocument.Load(path);

        //        IEnumerable<XElement> datatypesXElements = XmlUtility.GetXElementByNodeName("datatype", xdoc);

        //        if (datatypesXElements.Count() > 0)
        //        {

        //            foreach (XElement xDatatype in datatypesXElements)
        //            {
        //                string name = xDatatype.Attribute("name").Value;
        //                string description = xDatatype.Attribute("description").Value;
        //                string systemtype = xDatatype.Attribute("typeCode").Value;

        //                TypeCode systemType = TypeCode.String;

        //                foreach (TypeCode type in Enum.GetValues(typeof(TypeCode)))
        //                {
        //                    if (type.ToString().Equals(systemtype))
        //                    {
        //                        systemType = type;
        //                    }
        //                }

        //                dataType = dataTypeManager.Repo.Get().Where(d => d.Name.Equals(name)).FirstOrDefault();

        //                if (dataType == null)
        //                {
        //                    dataTypeManager.Create(name, description, systemType);
        //                }

        //            }
        //        }

        //    }
        //    catch (Exception ex)
        //    {

        //    }

        //}

        //private static void createSIUnits()
        //{
        //    DataTypeManager dataTypeManager = new DataTypeManager();

        //    UnitManager unitManager = new UnitManager();
        //    Unit unit = unitManager.Repo.Get(p => p.Name.Equals("None")).FirstOrDefault();


        //    Dimension dimNone = new Dimension();
        //    dimNone.Name = "Dimensionless";

        //    if (unitManager.DimensionRepo.Get(d => d.Name.Equals("None")) == null)
        //    {
        //        if (unit == null)
        //            unitManager.Create("None", "None", "If no unit is used.", dimNone, MeasurementSystem.Unknown);
        //        // null dimension should be replaced
        //    }
        //    try
        //    {
        //        string path = Path.Combine(AppConfiguration.GetModuleWorkspacePath("RPM"), "Seed", "Units.xml");
        //        XDocument xdoc = XDocument.Load(path);

        //        IEnumerable<XElement> unitsXElements = XmlUtility.GetXElementByNodeName("unit", xdoc);

        //        foreach (XElement xUnit in unitsXElements)
        //        {
        //            string name = xUnit.Attribute("name").Value;
        //            string abbrevation = xUnit.Attribute("abbrevation").Value;
        //            string mesurementSystem = xUnit.Attribute("mesurementSystem").Value;
        //            string dimension = xUnit.Attribute("dimension").Value;
        //            string dimensionSpecification = xUnit.Attribute("dimensionSpecification").Value;
        //            string description = xUnit.Attribute("description").Value;
        //            string[] associatedDataTypes = xUnit.Attribute("associatedDataTypes").Value.Split(',');

        //            MeasurementSystem measurementSystemEnum = MeasurementSystem.Unknown;

        //            foreach (MeasurementSystem msCheck in Enum.GetValues(typeof(MeasurementSystem)))
        //            {
        //                if (msCheck.ToString().Equals(mesurementSystem))
        //                {
        //                    measurementSystemEnum = msCheck;
        //                }
        //            }


        //            if (description.Length > 250)
        //            {
        //                description = description.Substring(0, 250) + "...";
        //            }

        //            unit = unitManager.Repo.Get(p => p.Name.Equals(name)).FirstOrDefault();

        //            if (unit == null)
        //            {
        //                Dimension dim = new Dimension();
        //                if (unitManager.DimensionRepo.Get().Where(d => d.Name.ToLower().Equals(dimension.ToLower()) && d.Specification.Equals(dimensionSpecification)).Count() == 0)
        //                {
        //                    dim = unitManager.Create(dimension, "", dimensionSpecification);
        //                }
        //                else
        //                {
        //                    dim = unitManager.DimensionRepo.Get().Where(d => d.Name.ToLower().Equals(dimension.ToLower())).FirstOrDefault();
        //                }

        //                unit = unitManager.Create(name, abbrevation, description, dim, measurementSystemEnum); // null dimension should be replaced bz a proper Dimension object

        //                foreach (string dtName in associatedDataTypes)
        //                {
        //                    DataType dt = dataTypeManager.Repo.Get().Where(d => d.Name.Equals(dtName)).FirstOrDefault();
        //                    if (dt != null)
        //                        unit.AssociatedDataTypes.Add(dt);
        //                }
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Debug.WriteLine(ex.Message);
        //    }

        //}
        #endregion

        #region METADATA

        private static void ImportSchema(string name, string filename, string root, string entity)
        {
            long metadataStructureid = 0;
            string schemaName = name;

            string filepath = Path.Combine(AppConfiguration.GetModuleWorkspacePath("DCM"), "Metadata", name,
                filename);

            XmlSchemaManager xmlSchemaManager = new XmlSchemaManager();

            //load
            try
            {
                xmlSchemaManager.Load(filepath, "application");
            }
            catch (Exception ex)
            {
                throw ex;
            }

            //generate
            try
            {
                metadataStructureid = xmlSchemaManager.GenerateMetadataStructure("Dataset", schemaName);
            }
            catch (Exception ex)
            {
                xmlSchemaManager.Delete(schemaName);
            }

            try
            {
                //set parameters
                string titleXPath = "Metadata/Metadata/MetadataType/Description/DescriptionType/Representation/MetadataDescriptionRepr/Title/TitleType";
                string descriptionXpath = "Metadata/Metadata/MetadataType/Description/DescriptionType/Representation/MetadataDescriptionRepr/Details/DetailsType";
                string mappingFileImport = xmlSchemaManager.mappingFileNameImport;
                string mappingFileExport = xmlSchemaManager.mappingFileNameExport;

                StoreParametersToMetadataStruture(
                    metadataStructureid,
                    titleXPath,
                    descriptionXpath,
                    entity,
                    mappingFileImport,
                    mappingFileExport);
            }
            catch (Exception ex)
            {

                throw ex;
            }


        }


        private static void createEmlDatasetAdv()
        {

            MetadataStructureManager mdsManager = new MetadataStructureManager();
            MetadataPackageManager mdpManager = new MetadataPackageManager();
            MetadataAttributeManager mdaManager = new MetadataAttributeManager();

            DataTypeManager dataTypeManager = new DataTypeManager();
            UnitManager unitManager = new UnitManager();

            MetadataStructure eml = mdsManager.Repo.Get(p => p.Name == "eml-dataset basic").FirstOrDefault();

            if (eml == null) eml = mdsManager.Create("eml-dataset basic", "The eml-dataset module contains general information that describes dataset resources. It provides an overview about the dataset attributes such as title, abstract, keywords, contacts, and distribution of the data themselves.", "", "", null);

            XmlDocument xmlDoc = new XmlDocument();

            if (eml.Extra != null)
            {
                xmlDoc = (XmlDocument)eml.Extra;
            }

            // add title Node
            xmlDoc = AddReferenceToMetadatStructure(eml, "title", "Metadata/Description/DescriptionEML/Title/Title", "xpath", "extra/nodeReferences/nodeRef", xmlDoc);
            // add description
            xmlDoc = AddReferenceToMetadatStructure(eml, "description", "Metadata/Description/DescriptionEML/AdditionalInformation/Information", "xpath", "extra/nodeReferences/nodeRef", xmlDoc);

            // add ConvertReference Mapping file node
            xmlDoc = AddReferenceToMetadatStructure(eml, "eml", "mapping_eml.xml", TransmissionType.mappingFileExport.ToString(), "extra/convertReferences/convertRef", xmlDoc);

            eml.Extra = xmlDoc;
            mdsManager.Update(eml);

            #region create packages

            //package Description for title
            MetadataPackage DescEml = mdpManager.MetadataPackageRepo.Get(p => p.Name == "DescriptionEML").FirstOrDefault();
            if (DescEml == null) DescEml = mdpManager.Create("DescriptionEML", "DescriptionEML", true);

            //package PersonEML ( Creator / Contact)
            MetadataPackage personEml = mdpManager.MetadataPackageRepo.Get(p => p.Name == "PersonEML").FirstOrDefault();
            if (personEml == null) personEml = mdpManager.Create("PersonEML", "PersonEML", true);

            //package PersonEML ( Creator / Contact)
            MetadataPackage party = mdpManager.MetadataPackageRepo.Get(p => p.Name == "Party").FirstOrDefault();
            if (party == null) party = mdpManager.Create("Party", "Person or Organization", true);

            //package PersonEML ( Creator / Contact)
            MetadataPackage projectEml = mdpManager.MetadataPackageRepo.Get(p => p.Name == "ProjectEML").FirstOrDefault();
            if (projectEml == null) projectEml = mdpManager.Create("ProjectEML", "ProjectEML", true);

            //package PersonEML ( Creator / Contact)
            MetadataPackage coverage = mdpManager.MetadataPackageRepo.Get(p => p.Name == "Coverage").FirstOrDefault();
            if (coverage == null) coverage = mdpManager.Create("Coverage", "Coverage", true);

            #endregion

            #region add packages


            // add package to structure
            if (eml.MetadataPackageUsages != null && eml.MetadataPackageUsages.Count > 0)
            {
                if (eml.MetadataPackageUsages.Where(p => p.MetadataPackage == DescEml).Count() <= 0)
                    mdsManager.AddMetadataPackageUsage(eml, DescEml, "Description", "A text description of the maintenance of this data resource.", 1, 1);

                if (eml.MetadataPackageUsages.Where(p => p.MetadataPackage == personEml).Count() <= 0)
                    mdsManager.AddMetadataPackageUsage(eml, personEml, "Creator", "The 'creator' element provides the full name of the person, organization, or position who created the resource.", 1, 5);

                if (eml.MetadataPackageUsages.Where(p => p.MetadataPackage == party).Count() <= 0)
                    mdsManager.AddMetadataPackageUsage(eml, party, "Associated Party ", "The responsible party is used to describe a person, organization, or position within an organization.", 1, 10);

                if (eml.MetadataPackageUsages.Where(p => p.MetadataPackage == personEml).Count() <= 0)
                    mdsManager.AddMetadataPackageUsage(eml, personEml, "Contact", "The contact contains contact information for this dataset. This is the person or institution to contact with questions about the use, interpretation of a data set.", 1, 5);

                if (eml.MetadataPackageUsages.Where(p => p.MetadataPackage == projectEml).Count() <= 0)
                    mdsManager.AddMetadataPackageUsage(eml, projectEml, "Project", "The project contains information on the project in which this dataset was collected.", 1, 1);

                //if (eml.MetadataPackageUsages.Where(p => p.MetadataPackage == coverage).Count() <= 0)
                //    mdsManager.AddMetadataPackageUsage(eml, coverage, "Coverage", 1, 1);
            }
            else
            {
                mdsManager.AddMetadataPackageUsage(eml, DescEml, "Description", "A text description of the maintenance of this data resource.", 1, 1);
                mdsManager.AddMetadataPackageUsage(eml, personEml, "Creator", "The 'creator' element provides the full name of the person, organization, or position who created the resource.", 1, 5);
                mdsManager.AddMetadataPackageUsage(eml, party, "Associated Parties ", "The responsible party is used to describe a person, organization, or position within an organization.", 1, 10);
                mdsManager.AddMetadataPackageUsage(eml, personEml, "Contact", "The contact contains contact information for this dataset. This is the person or institution to contact with questions about the use, interpretation of a data set.", 1, 5);
                mdsManager.AddMetadataPackageUsage(eml, projectEml, "Project", "The project contains information on the project in which this dataset was collected.", 1, 1);
                //mdsManager.AddMetadataPackageUsage(eml, coverage, "Coverage", 1, 1);
            }

            #endregion

            #region Description EML

            #region create attr

            MetadataAttribute Name = mdaManager.MetadataAttributeRepo.Get(p => p.Name.Equals("Name")).FirstOrDefault();
            if (Name == null)
            {
                DataType dataType = dataTypeManager.Repo.Get(p => p.SystemType.Equals("String") && p.Name.ToLower().Equals("string")).FirstOrDefault();
                Unit unit = unitManager.Repo.Get(p => p.Name.Equals("None")).FirstOrDefault();

                Name = mdaManager.Create("Name", "Name", "first and last name", false, false, "David Blaa",
                        MeasurementScale.Categorial, DataContainerType.ValueType, "", dataType, unit, null, null, null, null);
            }

            MetadataAttribute Title = mdaManager.MetadataAttributeRepo.Get(p => p.Name.Equals("Title")).FirstOrDefault();
            if (Title == null)
            {
                DataType dataType = dataTypeManager.Repo.Get(p => p.SystemType.Equals("String") && p.Name.ToLower().Equals("string")).FirstOrDefault();
                Unit unit = unitManager.Repo.Get(p => p.Name.Equals("None")).FirstOrDefault();

                Title = mdaManager.Create("Title", "Title", "Title", false, false, "David Blaa",
                        MeasurementScale.Categorial, DataContainerType.ValueType, "", dataType, unit, null, null, null, null);
            }

            MetadataAttribute Date = mdaManager.MetadataAttributeRepo.Get(p => p.Name.Equals("Date")).FirstOrDefault();
            if (Date == null)
            {
                DataType dataType = dataTypeManager.Repo.Get(p => p.SystemType.Equals("DateTime")).FirstOrDefault();
                Unit unit = unitManager.Repo.Get(p => p.Name.Equals("None")).FirstOrDefault();

                Date = mdaManager.Create("Date", "Date", "Date", false, false, "David Blaa",
                        MeasurementScale.Categorial, DataContainerType.ValueType, "", dataType, unit, null, null, null, null);
            }

            MetadataAttribute Info = mdaManager.MetadataAttributeRepo.Get(p => p.Name.Equals("Information")).FirstOrDefault();
            if (Info == null)
            {
                DataType dataType = dataTypeManager.Repo.Get(p => p.SystemType.Equals("String") && p.Name.ToLower().Equals("text")).FirstOrDefault();
                Unit unit = unitManager.Repo.Get(p => p.Name.Equals("None")).FirstOrDefault();

                Info = mdaManager.Create("Information", "Information", "Information", false, false, "David Blaa",
                        MeasurementScale.Categorial, DataContainerType.ValueType, "", dataType, unit, null, null, null, null);
            }


            #endregion

            #region add attr

            if (DescEml.MetadataAttributeUsages != null & DescEml.MetadataAttributeUsages.Count > 0)
            {
                // add metadataAttributes to packages
                if (DescEml.MetadataAttributeUsages.Where(p => p.MetadataAttribute == Name).Count() <= 0)
                {
                    mdpManager.AddMetadataAtributeUsage(DescEml, Name, "Short Name", "The 'shortName' provides a concise name that describes the resource that is being documented.", 0, 1);
                }

                if (DescEml.MetadataAttributeUsages.Where(p => p.MetadataAttribute == Title).Count() <= 0)
                    mdpManager.AddMetadataAtributeUsage(DescEml, Title, "Title", "The 'title' provides a description of the resource that is being documented that is long enough to differentiate it from other similar resources.", 1, 1);

                if (DescEml.MetadataAttributeUsages.Where(p => p.MetadataAttribute == Date).Count() <= 0)
                    mdpManager.AddMetadataAtributeUsage(DescEml, Date, "Publish Date", "The 'Publish Date' represents the date that the resource was published.", 0, 1);

                if (DescEml.MetadataAttributeUsages.Where(p => p.MetadataAttribute == Info).Count() <= 0)
                    mdpManager.AddMetadataAtributeUsage(DescEml, Info, "Additional Information", "This field provides any information that is not characterized by the other resource metadata fields.", 0, 1);


            }
            else
            {
                mdpManager.AddMetadataAtributeUsage(DescEml, Name, "Short Name", "The 'shortName' provides a concise name that describes the resource that is being documented.", 0, 1);
                mdpManager.AddMetadataAtributeUsage(DescEml, Title, "Title", "The 'title' provides a description of the resource that is being documented that is long enough to differentiate it from other similar resources.", 1, 1);
                mdpManager.AddMetadataAtributeUsage(DescEml, Date, "Publish Date", "The 'Publish Date' represents the date that the resource was published.", 0, 1);
                mdpManager.AddMetadataAtributeUsage(DescEml, Info, "Additional Information", "This field provides any information that is not characterized by the other resource metadata fields.", 0, 1);
            }


            #endregion

            #endregion

            #region Peronal EML

            #region create attr

            Name = mdaManager.MetadataAttributeRepo.Get(p => p.Name.Equals("Name")).FirstOrDefault();
            if (Name == null)
            {
                DataType dataType = dataTypeManager.Repo.Get(p => p.SystemType.Equals("String") && p.Name.ToLower().Equals("string")).FirstOrDefault();
                Unit unit = unitManager.Repo.Get(p => p.Name.Equals("None")).FirstOrDefault();

                Name = mdaManager.Create("Name", "Name", "first and last name", false, false, "David Blaa",
                    MeasurementScale.Categorial, DataContainerType.ValueType, "", dataType, unit, null, null, null, null);
            }

            #endregion

            #region add attr

            if (Name == null)
            {
                DataType dataType = dataTypeManager.Repo.Get(p => p.SystemType.Equals("String") && p.Name.ToLower().Equals("string")).FirstOrDefault();
                Unit unit = unitManager.Repo.Get(p => p.Name.Equals("None")).FirstOrDefault();

                Name = mdaManager.Create("Name", "Name", "first and last name", false, false, "David Blaa",
                        MeasurementScale.Categorial, DataContainerType.ValueType, "", dataType, unit, null, null, null, null);
            }

            if (personEml.MetadataAttributeUsages != null & personEml.MetadataAttributeUsages.Count > 0)
            {
                // add metadataAttributes to packages
                if (personEml.MetadataAttributeUsages.Where(p => p.MetadataAttribute == Name).Count() <= 0)
                    mdpManager.AddMetadataAtributeUsage(personEml, Name, "Given name", "The given name is used for the first name of the individual associated with the resource.", 1, 1);

                if (personEml.MetadataAttributeUsages.Where(p => p.MetadataAttribute == Name).Count() <= 0)
                    mdpManager.AddMetadataAtributeUsage(personEml, Name, "Sur name", "The Sur name is used for the last name of the individual associated with the resource.", 1, 1);

                if (personEml.MetadataAttributeUsages.Where(p => p.MetadataAttribute == Name).Count() <= 0)
                    mdpManager.AddMetadataAtributeUsage(personEml, Name, "Organization", "This field is intended to describe which institution or overall organization is associated with the resource being described.", 1, 1);

            }
            else
            {
                mdpManager.AddMetadataAtributeUsage(personEml, Name, "Given name", "The given name is used for the first name of the individual associated with the resource.", 1, 1);
                mdpManager.AddMetadataAtributeUsage(personEml, Name, "Sur name", "The Sur name is used for the last name of the individual associated with the resource.", 1, 1);
                mdpManager.AddMetadataAtributeUsage(personEml, Name, "Organization", "This field is intended to describe which institution or overall organization is associated with the resource being described.", 1, 1);
            }

            #endregion

            #endregion

            #region Peronal with role EML

            #region create attr

            Name = mdaManager.MetadataAttributeRepo.Get(p => p.Name.Equals("Name")).FirstOrDefault();
            if (Name == null)
            {
                DataType dataType = dataTypeManager.Repo.Get(p => p.SystemType.Equals("String") && p.Name.ToLower().Equals("string")).FirstOrDefault();
                Unit unit = unitManager.Repo.Get(p => p.Name.Equals("None")).FirstOrDefault();

                Name = mdaManager.Create("Name", "Name", "first and last name", false, false, "David Blaa",
                        MeasurementScale.Categorial, DataContainerType.ValueType, "", dataType, unit, null, null, null, null);
            }

            MetadataAttribute RoleType = mdaManager.MetadataAttributeRepo.Get(p => p.Name.Equals("RoleType")).FirstOrDefault();
            if (RoleType == null)
            {
                DataType dataType = dataTypeManager.Repo.Get(p => p.SystemType.Equals("String") && p.Name.ToLower().Equals("string")).FirstOrDefault();
                Unit unit = unitManager.Repo.Get(p => p.Name.Equals("None")).FirstOrDefault();

                RoleType = mdaManager.Create("Role", "RoleType", "Use this field to describe the role the party played with respect to the resource. Some potential roles include technician, reviewer, principal investigator, and many others.", false, false, "David Blaa",
                        MeasurementScale.Categorial, DataContainerType.ValueType, "", dataType, unit, null, null, null, null);
            }


            #endregion

            #region add attr

            if (party.MetadataAttributeUsages != null & party.MetadataAttributeUsages.Count > 0)
            {
                // add metadataAttributes to packages
                if (party.MetadataAttributeUsages.Where(p => p.MetadataAttribute == Name).Count() <= 0)
                    mdpManager.AddMetadataAtributeUsage(party, Name, "Given name", "The given name is used for the first name of the individual associated with the resource.", 1, 1);

                if (party.MetadataAttributeUsages.Where(p => p.MetadataAttribute == Name).Count() <= 0)
                    mdpManager.AddMetadataAtributeUsage(party, Name, "Surname", "The Surname is used for the last name of the individual associated with the resource.", 1, 1);

                if (party.MetadataAttributeUsages.Where(p => p.MetadataAttribute == Name).Count() <= 0)
                    mdpManager.AddMetadataAtributeUsage(party, Name, "Organization", "This field is intended to describe which institution or overall organization is associated with the resource being described.", 1, 1);

                if (party.MetadataAttributeUsages.Where(p => p.MetadataAttribute == Name).Count() <= 0)
                    mdpManager.AddMetadataAtributeUsage(party, RoleType, "Role", "Use to describe the role the party played with respect to the resource.", 1, 1);

            }
            else
            {
                mdpManager.AddMetadataAtributeUsage(party, Name, "Given name", "The given name is used for the first name of the individual associated with the resource.", 1, 1);
                mdpManager.AddMetadataAtributeUsage(party, Name, "Sur name", "The Sur name is used for the last name of the individual associated with the resource.", 1, 1);
                mdpManager.AddMetadataAtributeUsage(party, Name, "Organization", "This field is intended to describe which institution or overall organization is associated with the resource being described.", 1, 1);
                mdpManager.AddMetadataAtributeUsage(party, RoleType, "Role", "Use to describe the role the party played with respect to the resource.", 1, 1);
            }

            #endregion

            #endregion

            #region Project Eml

            #region create attr

            MetadataAttribute DescriptionAttr = mdaManager.MetadataAttributeRepo.Get(p => p.Name.Equals("Description")).FirstOrDefault();
            if (DescriptionAttr == null)
            {
                DataType dataType = dataTypeManager.Repo.Get(p => p.SystemType.Equals("String") && p.Name.ToLower().Equals("text")).FirstOrDefault();
                Unit unit = unitManager.Repo.Get(p => p.Name.Equals("None")).FirstOrDefault();

                DescriptionAttr = mdaManager.Create("Description", "Description", "Description", false, false, "David Blaa",
                        MeasurementScale.Categorial, DataContainerType.ValueType, "", dataType, unit, null, null, null, null);
            }

            #endregion

            #region add attr
            if (projectEml.MetadataAttributeUsages != null & projectEml.MetadataAttributeUsages.Count > 0)
            {
                if (Title == null)
                {
                    DataType dataType = dataTypeManager.Repo.Get(p => p.SystemType.Equals("String") && p.Name.ToLower().Equals("string")).FirstOrDefault();
                    Unit unit = unitManager.Repo.Get(p => p.Name.Equals("None")).FirstOrDefault();

                    Title = mdaManager.Create("Title", "Title", "Title", false, false, "David Blaa",
                            MeasurementScale.Categorial, DataContainerType.ValueType, "", dataType, unit, null, null, null, null);
                }

                RoleType = mdaManager.MetadataAttributeRepo.Get(p => p.Name.Equals("RoleType")).FirstOrDefault();
                if (RoleType == null)
                {
                    DataType dataType = dataTypeManager.Repo.Get(p => p.SystemType.Equals("String") && p.Name.ToLower().Equals("string")).FirstOrDefault();
                    Unit unit = unitManager.Repo.Get(p => p.Name.Equals("None")).FirstOrDefault();

                    RoleType = mdaManager.Create("Role", "RoleType", "Use this field to describe the role the party played with respect to the resource. Some potential roles include technician, reviewer, principal investigator, and many others.", false, false, "David Blaa",
                        MeasurementScale.Categorial, DataContainerType.ValueType, "", dataType, unit, null, null, null, null);
                }

                // add metadataAttributes to packages
                if (projectEml.MetadataAttributeUsages.Where(p => p.MetadataAttribute == Title).Count() <= 0)
                    mdpManager.AddMetadataAtributeUsage(projectEml, Title, "Title", "A descriptive title for the research project.", 0, 1);

                if (projectEml.MetadataAttributeUsages.Where(p => p.MetadataAttribute == Name).Count() <= 0)
                    mdpManager.AddMetadataAtributeUsage(projectEml, Name, "Personnel given name", "The given name is used for the first name of the individual associated with the resource.", 0, 1);

                if (projectEml.MetadataAttributeUsages.Where(p => p.MetadataAttribute == Name).Count() <= 0)
                    mdpManager.AddMetadataAtributeUsage(projectEml, Name, "Personnel surname", "The Sur name is used for the last name of the individual associated with the resource.", 0, 1);

                if (projectEml.MetadataAttributeUsages.Where(p => p.MetadataAttribute == RoleType).Count() <= 0)
                    mdpManager.AddMetadataAtributeUsage(projectEml, RoleType, "Role", "", 0, 1);

                if (projectEml.MetadataAttributeUsages.Where(p => p.MetadataAttribute == DescriptionAttr).Count() <= 0)
                    mdpManager.AddMetadataAtributeUsage(projectEml, DescriptionAttr, "Project description", "Contains general textual descriptions of research design.", 0, 1);

            }
            else
            {
                mdpManager.AddMetadataAtributeUsage(projectEml, Title, "Title", "A descriptive title for the research project.", 0, 1);
                mdpManager.AddMetadataAtributeUsage(projectEml, Name, "Personnel given name", "The given name is used for the first name of the individual associated with the resource.", 0, 1);
                mdpManager.AddMetadataAtributeUsage(projectEml, Name, "Personnel surname", "The Sur name is used for the last name of the individual associated with the resource.", 0, 1);
                mdpManager.AddMetadataAtributeUsage(projectEml, RoleType, "Role", "", 0, 1);
                mdpManager.AddMetadataAtributeUsage(projectEml, DescriptionAttr, "Project description", "Contains general textual descriptions of research design.", 0, 1);
            }

            #endregion

            #endregion


        }

        private static void createABCD()
        {
            MetadataStructureManager mdsManager = new MetadataStructureManager();
            MetadataPackageManager mdpManager = new MetadataPackageManager();
            MetadataAttributeManager mdaManager = new MetadataAttributeManager();

            DataTypeManager dataTypeManager = new DataTypeManager();
            UnitManager unitManager = new UnitManager();

            #region ABCD

            MetadataStructure abcd = mdsManager.Repo.Get(p => p.Name == "ABCD Basic").FirstOrDefault();
            if (abcd == null) abcd = mdsManager.Create("ABCD Basic", "Access to Biological Collections Data - Schema is a common data specification for biological collection units, including living and preserved specimens, along with field observations that did not produce voucher specimens.", "", "", null);

            XmlDocument xmlDoc = new XmlDocument();

            if (abcd.Extra != null)
            {
                xmlDoc = (XmlDocument)abcd.Extra;
            }

            // add title Node
            xmlDoc = AddReferenceToMetadatStructure(abcd, "title", "Metadata/Description/Description/Title/Title", "xpath", "extra/nodeReferences/nodeRef", xmlDoc);
            // add Description
            xmlDoc = AddReferenceToMetadatStructure(abcd, "description", "Metadata/Description/Description/Details/Details", "xpath", "extra/nodeReferences/nodeRef", xmlDoc);


            // add ConvertReference Mapping file node
            xmlDoc = AddReferenceToMetadatStructure(abcd, "abcd", "mapping_abcd.xml", TransmissionType.mappingFileExport.ToString(), "extra/convertReferences/convertRef", xmlDoc);

            abcd.Extra = xmlDoc;
            mdsManager.Update(abcd);

            //package Person ( Tecnical contact /ContentContact)
            MetadataPackage person = mdpManager.MetadataPackageRepo.Get(p => p.Name == "Person").FirstOrDefault();
            if (person == null) person = mdpManager.Create("Person", "Person", true);

            //package Description
            MetadataPackage Description = mdpManager.MetadataPackageRepo.Get(p => p.Name == "Description").FirstOrDefault();
            if (Description == null) Description = mdpManager.Create("Description", "Description about a dataset", true);

            //package Owner
            MetadataPackage Owner = mdpManager.MetadataPackageRepo.Get(p => p.Name == "Owner").FirstOrDefault();
            if (Owner == null) Owner = mdpManager.Create("Owner", "Owner/s of the dataset", true);

            // Package Scope
            MetadataPackage Scope = mdpManager.MetadataPackageRepo.Get(p => p.Name == "Scope").FirstOrDefault();
            if (Scope == null) Scope = mdpManager.Create("Scope", "Scope of the dataset", true);


            // add package to structure
            if (abcd.MetadataPackageUsages != null && abcd.MetadataPackageUsages.Count > 0)
            {
                if (abcd.MetadataPackageUsages.Where(p => p.MetadataPackage == person).Count() <= 0)
                    mdsManager.AddMetadataPackageUsage(abcd, person, "Content Contact", "A container element for several administrative contacts for the dataset.", 0, 3);

                if (abcd.MetadataPackageUsages.Where(p => p.MetadataPackage == Description).Count() <= 0)
                    mdsManager.AddMetadataPackageUsage(abcd, Description, "Description", "Description of the data source queried.", 0, 1);

                if (abcd.MetadataPackageUsages.Where(p => p.MetadataPackage == Owner).Count() <= 0)
                    mdsManager.AddMetadataPackageUsage(abcd, Owner, "Owner", "A container element for several owners of the data source.", 1, 5);

                if (abcd.MetadataPackageUsages.Where(p => p.MetadataPackage == Scope).Count() <= 0)
                    mdsManager.AddMetadataPackageUsage(abcd, Scope, "Scope", "Keyword list describing the scope of the data source.", 0, 1);
            }
            else
            {

                mdsManager.AddMetadataPackageUsage(abcd, person, "Technical Contact", "A technical contact normally representing the agent acting as the publisher of the dataset in the network.", 1, 1);
                mdsManager.AddMetadataPackageUsage(abcd, person, "Content Contact", "A container element for several administrative contacts for the dataset.", 1, 10);
                mdsManager.AddMetadataPackageUsage(abcd, Description, "Description", "Description of the data source queried.", 1, 1);
                mdsManager.AddMetadataPackageUsage(abcd, Owner, "Owner", "A container element for several owners of the data source.", 1, 10);
                mdsManager.AddMetadataPackageUsage(abcd, Scope, "Scope", "Keyword list describing the scope of the data source.", 0, 1);

            }


            #region person

            MetadataAttribute Name = mdaManager.MetadataAttributeRepo.Get(p => p.Name.Equals("Name")).FirstOrDefault();
            if (Name == null)
            {
                DataType dataType = dataTypeManager.Repo.Get(p => p.SystemType.Equals("String") && p.Name.ToLower().Equals("string")).FirstOrDefault();
                Unit unit = unitManager.Repo.Get(p => p.Name.Equals("None")).FirstOrDefault();

                Name = mdaManager.Create("Name", "Name", "first and last name", false, false, "David Blaa",
                        MeasurementScale.Categorial, DataContainerType.ValueType, "", dataType, unit, null, null, null, null);
            }

            MetadataAttribute Email = mdaManager.MetadataAttributeRepo.Get(p => p.Name.Equals("Email")).FirstOrDefault();
            if (Email == null)
            {
                DataType dataType = dataTypeManager.Repo.Get(p => p.SystemType.Equals("String") && p.Name.ToLower().Equals("string")).FirstOrDefault();
                Unit unit = unitManager.Repo.Get(p => p.Name.Equals("None")).FirstOrDefault();

                Email = mdaManager.Create("Email", "Email", "Email address", false, false, "David Blaa",
                        MeasurementScale.Categorial, DataContainerType.ValueType, "", dataType, unit, null, null, null, null);
            }

            MetadataAttribute Address = mdaManager.MetadataAttributeRepo.Get(p => p.Name.Equals("Address")).FirstOrDefault();
            if (Address == null)
            {
                DataType dataType = dataTypeManager.Repo.Get(p => p.SystemType.Equals("String") && p.Name.ToLower().Equals("string")).FirstOrDefault();
                Unit unit = unitManager.Repo.Get(p => p.Name.Equals("None")).FirstOrDefault();

                Address = mdaManager.Create("Address", "Address", "Address", false, false, "David Blaa",
                        MeasurementScale.Categorial, DataContainerType.ValueType, "", dataType, unit, null, null, null, null);
            }

            MetadataAttribute Phone = mdaManager.MetadataAttributeRepo.Get(p => p.Name.Equals("Phone")).FirstOrDefault();
            if (Phone == null)
            {
                DataType dataType = dataTypeManager.Repo.Get(p => p.SystemType.Equals("String") && p.Name.ToLower().Equals("string")).FirstOrDefault();
                Unit unit = unitManager.Repo.Get(p => p.Name.Equals("None")).FirstOrDefault();

                Phone = mdaManager.Create("Phone", "Phone", "Phone", false, false, "David Blaa",
                        MeasurementScale.Categorial, DataContainerType.ValueType, "", dataType, unit, null, null, null, null);
            }


            if (person.MetadataAttributeUsages != null & person.MetadataAttributeUsages.Count > 0)
            {
                // add metadataAttributes to packages
                if (person.MetadataAttributeUsages.Where(p => p.MetadataAttribute == Name).Count() <= 0)
                    mdpManager.AddMetadataAtributeUsage(person, Name, "Name", "Administrative contact person, Technical contact person, person team, or role for the dataset.", 1, 1);

                if (person.MetadataAttributeUsages.Where(p => p.MetadataAttribute == Email).Count() <= 0)
                    mdpManager.AddMetadataAtributeUsage(person, Email, "Email", "Email address for the administrative, or technical contact for the dataset.", 0, 1);

                if (person.MetadataAttributeUsages.Where(p => p.MetadataAttribute == Address).Count() <= 0)
                    mdpManager.AddMetadataAtributeUsage(person, Address, "Address", "An address for the administrative, or technical contact for the dataset.", 0, 1);

                if (person.MetadataAttributeUsages.Where(p => p.MetadataAttribute == Phone).Count() <= 0)
                    mdpManager.AddMetadataAtributeUsage(person, Phone, "Phone", "Voice phone number for the administrative, or technical contact for the dataset.", 0, 1);
            }
            else
            {
                mdpManager.AddMetadataAtributeUsage(person, Name, "Name", "Administrative contact person, Technical contact person, person team, or role for the dataset.", 1, 1);
                mdpManager.AddMetadataAtributeUsage(person, Email, "Email", "Email address for the administrative, or technical contact for the dataset.", 0, 1);
                mdpManager.AddMetadataAtributeUsage(person, Address, "Address", "An address for the administrative, or technical contact for the dataset.", 0, 1);
                mdpManager.AddMetadataAtributeUsage(person, Phone, "Phone", "Voice phone number for the administrative, or technical contact for the dataset.", 0, 1);
            }

            #endregion

            #region metadata

            MetadataAttribute Title = mdaManager.MetadataAttributeRepo.Get(p => p.Name.Equals("Title")).FirstOrDefault();
            if (Title == null)
            {
                DataType dataType = dataTypeManager.Repo.Get(p => p.SystemType.Equals("String") && p.Name.ToLower().Equals("string")).FirstOrDefault();
                Unit unit = unitManager.Repo.Get(p => p.Name.Equals("None")).FirstOrDefault();

                Title = mdaManager.Create("Title", "Title", "Title", false, false, "David Blaa",
                        MeasurementScale.Categorial, DataContainerType.ValueType, "", dataType, unit, null, null, null, null);
            }

            MetadataAttribute RevisionData = mdaManager.MetadataAttributeRepo.Get(p => p.Name.Equals("RevisionData")).FirstOrDefault();
            if (RevisionData == null)
            {
                DataType dataType = dataTypeManager.Repo.Get(p => p.SystemType.Equals("DateTime")).FirstOrDefault();
                Unit unit = unitManager.Repo.Get(p => p.Name.Equals("None")).FirstOrDefault();

                RevisionData = mdaManager.Create("RevisionData", "RevisionData", "RevisionData", false, false, "David Blaa",
                        MeasurementScale.Categorial, DataContainerType.ValueType, "", dataType, unit, null, null, null, null);
            }

            MetadataAttribute Details = mdaManager.MetadataAttributeRepo.Get(p => p.Name.Equals("Details")).FirstOrDefault();
            if (Details == null)
            {
                DataType dataType = dataTypeManager.Repo.Get(p => p.SystemType.Equals("String") && p.Name.ToLower().Equals("text")).FirstOrDefault();
                Unit unit = unitManager.Repo.Get(p => p.Name.Equals("None")).FirstOrDefault();

                Details = mdaManager.Create("Details", "Details", "Details", false, false, "David Blaa",
                        MeasurementScale.Categorial, DataContainerType.ValueType, "", dataType, unit, null, null, null, null);
            }

            MetadataAttribute Coverage = mdaManager.MetadataAttributeRepo.Get(p => p.Name.Equals("Coverage")).FirstOrDefault();
            if (Coverage == null)
            {
                DataType dataType = dataTypeManager.Repo.Get(p => p.SystemType.Equals("String") && p.Name.ToLower().Equals("string")).FirstOrDefault();
                Unit unit = unitManager.Repo.Get(p => p.Name.Equals("None")).FirstOrDefault();

                Coverage = mdaManager.Create("Coverage", "Coverage", "Coverage", false, false, "David Blaa",
                        MeasurementScale.Categorial, DataContainerType.ValueType, "", dataType, unit, null, null, null, null);
            }

            MetadataAttribute URI = mdaManager.MetadataAttributeRepo.Get(p => p.Name.Equals("URI")).FirstOrDefault();
            if (URI == null)
            {
                DataType dataType = dataTypeManager.Repo.Get(p => p.SystemType.Equals("String") && p.Name.ToLower().Equals("string")).FirstOrDefault();
                Unit unit = unitManager.Repo.Get(p => p.Name.Equals("None")).FirstOrDefault();

                URI = mdaManager.Create("URI", "URI", "URI", false, false, "David Blaa",
                        MeasurementScale.Categorial, DataContainerType.ValueType, "", dataType, unit, null, null, null, null);
            }

            if (Description.MetadataAttributeUsages != null & Description.MetadataAttributeUsages.Count > 0)
            {
                // add metadataAttributes to packages
                if (Description.MetadataAttributeUsages.Where(p => p.MetadataAttribute == Title).Count() <= 0)
                    mdpManager.AddMetadataAtributeUsage(Description, Title, "Title", "A short concise title for the dataset.", 1, 1);

                if (Description.MetadataAttributeUsages.Where(p => p.MetadataAttribute == RevisionData).Count() <= 0)
                    mdpManager.AddMetadataAtributeUsage(Description, RevisionData, "DateModified", "The last modification date for the data source.", 0, 1);

                if (Description.MetadataAttributeUsages.Where(p => p.MetadataAttribute == Details).Count() <= 0)
                    mdpManager.AddMetadataAtributeUsage(Description, Details, "Details", "Free-form text containing a longer description of the data source.", 0, 1);

                if (Description.MetadataAttributeUsages.Where(p => p.MetadataAttribute == Coverage).Count() <= 0)
                    mdpManager.AddMetadataAtributeUsage(Description, Coverage, "Coverage", "Free-form text terminology or descriptions available in the data source (geographic, taxonomic, etc.).", 0, 1);

                if (Description.MetadataAttributeUsages.Where(p => p.MetadataAttribute == URI).Count() <= 0)
                    mdpManager.AddMetadataAtributeUsage(Description, URI, "URI", "URL that points to an online source related to the data source, which may or may not serve as an updated version of the description data.", 0, 1);
            }
            else
            {
                mdpManager.AddMetadataAtributeUsage(Description, Title, "Title", "A short concise title for the dataset.", 1, 1);
                mdpManager.AddMetadataAtributeUsage(Description, RevisionData, "DateModified", "The last modification date for the data source.", 0, 1);
                mdpManager.AddMetadataAtributeUsage(Description, Details, "Details", "Free-form text containing a longer description of the data source.", 0, 1);
                mdpManager.AddMetadataAtributeUsage(Description, Coverage, "Coverage", "Free-form text terminology or descriptions available in the data source (geographic, taxonomic, etc.).", 0, 1);
                mdpManager.AddMetadataAtributeUsage(Description, URI, "URI", "URL that points to an online source related to the data source, which may or may not serve as an updated version of the description data.", 0, 1);
            }



            #endregion

            #region Owner package

            MetadataAttribute Role = mdaManager.MetadataAttributeRepo.Get(p => p.Name.Equals("Role")).FirstOrDefault();
            if (Role == null)
            {
                DataType dataType = dataTypeManager.Repo.Get(p => p.SystemType.Equals("String") && p.Name.ToLower().Equals("string") && p.Name.ToLower().Equals("string")).FirstOrDefault();
                Unit unit = unitManager.Repo.Get(p => p.Name.Equals("None")).FirstOrDefault();

                Role = mdaManager.Create("Role", "Role", "Role", false, false, "David Blaa",
                        MeasurementScale.Categorial, DataContainerType.ValueType, "", dataType, unit, null, null, null, null);
            }

            if (Owner.MetadataAttributeUsages != null & Owner.MetadataAttributeUsages.Count > 0)
            {
                // add metadataAttributes to packages
                if (Owner.MetadataAttributeUsages.Where(p => p.MetadataAttribute == Name).Count() <= 0)
                    mdpManager.AddMetadataAtributeUsage(Owner, Name, "Full Name", "String of the preferred form of personal name for display representing the data collection's legal owner.", 1, 1);

                if (Owner.MetadataAttributeUsages.Where(p => p.MetadataAttribute == Name).Count() <= 0)
                    mdpManager.AddMetadataAtributeUsage(Owner, Name, "Sorting Name", "The full name of the data collection owner in a form appropriate for sorting alphabetically.", 0, 1);

                if (Owner.MetadataAttributeUsages.Where(p => p.MetadataAttribute == Name).Count() <= 0)
                    mdpManager.AddMetadataAtributeUsage(Owner, Name, "Organisation Name", "Container element for several language-specific representations of the full organisation or corporate name for the data source owner.", 0, 1);

                if (Owner.MetadataAttributeUsages.Where(p => p.MetadataAttribute == Role).Count() <= 0)
                    mdpManager.AddMetadataAtributeUsage(Owner, Role, "Role", "Title for the role of the person or organisation owner of the data collection.", 0, 1);

                if (Owner.MetadataAttributeUsages.Where(p => p.MetadataAttribute == Address).Count() <= 0)
                    mdpManager.AddMetadataAtributeUsage(Owner, Address, "Address", "A string representing the address of the data collection owner.", 0, 1);

                if (Owner.MetadataAttributeUsages.Where(p => p.MetadataAttribute == Email).Count() <= 0)
                    mdpManager.AddMetadataAtributeUsage(Owner, Email, "Email", "A valid contact e-mail address for the owner of the data collection.", 0, 1);

                if (Owner.MetadataAttributeUsages.Where(p => p.MetadataAttribute == Phone).Count() <= 0)
                    mdpManager.AddMetadataAtributeUsage(Owner, Phone, "Phone", "Telephone number for the legal owner of the data collection.", 0, 1);

                if (Owner.MetadataAttributeUsages.Where(p => p.MetadataAttribute == URI).Count() <= 0)
                    mdpManager.AddMetadataAtributeUsage(Owner, URI, "URI", "Publicly available URL for the person or organisation representing the legal owner of the data collection.", 0, 1);

            }
            else
            {
                mdpManager.AddMetadataAtributeUsage(Owner, Name, "Full Name", "String of the preferred form of personal name for display representing the data collection's legal owner.", 1, 1);
                mdpManager.AddMetadataAtributeUsage(Owner, Name, "Sorting Name", "The full name of the data collection owner in a form appropriate for sorting alphabetically.", 0, 1);
                mdpManager.AddMetadataAtributeUsage(Owner, Name, "Organisation Name", "Container element for several language-specific representations of the full organisation or corporate name for the data source owner.", 0, 1);
                mdpManager.AddMetadataAtributeUsage(Owner, Address, "Address", "A string representing the address of the data collection owner.", 0, 1);
                mdpManager.AddMetadataAtributeUsage(Owner, Email, "Email", "A valid contact e-mail address for the owner of the data collection.", 0, 1);
                mdpManager.AddMetadataAtributeUsage(Owner, Role, "Role", "Title for the role of the person or organisation owner of the data collection.", 0, 1);
                mdpManager.AddMetadataAtributeUsage(Owner, Phone, "Phone", "Telephone number for the legal owner of the data collection.", 0, 1);
                mdpManager.AddMetadataAtributeUsage(Owner, URI, "URI", "Publicly available URL for the person or organisation representing the legal owner of the data collection.", 0, 1);

            }

            #endregion

            #region Scope package

            MetadataAttribute TaxonomicTerm = mdaManager.MetadataAttributeRepo.Get(p => p.Name.Equals("TaxonomicTerm")).FirstOrDefault();
            if (TaxonomicTerm == null)
            {
                DataType dataType = dataTypeManager.Repo.Get(p => p.SystemType.Equals("String") && p.Name.ToLower().Equals("string")).FirstOrDefault();
                Unit unit = unitManager.Repo.Get(p => p.Name.Equals("None")).FirstOrDefault();

                TaxonomicTerm = mdaManager.Create("TaxonomicTerm", "TaxonomicTerm", "TaxonomicTerm", false, false, "David Blaa",
                        MeasurementScale.Categorial, DataContainerType.ValueType, "", dataType, unit, null, null, null, null);
            }

            MetadataAttribute GeoEcologicalTerm = mdaManager.MetadataAttributeRepo.Get(p => p.Name.Equals("GeoEcologicalTerm")).FirstOrDefault();
            if (GeoEcologicalTerm == null)
            {
                DataType dataType = dataTypeManager.Repo.Get(p => p.SystemType.Equals("String") && p.Name.ToLower().Equals("string")).FirstOrDefault();
                Unit unit = unitManager.Repo.Get(p => p.Name.Equals("None")).FirstOrDefault();

                GeoEcologicalTerm = mdaManager.Create("GeoEcologicalTerm", "GeoEcologicalTerm", "GeoEcologicalTerm", false, false, "David Blaa",
                        MeasurementScale.Categorial, DataContainerType.ValueType, "", dataType, unit, null, null, null, null);
            }

            if (Scope.MetadataAttributeUsages != null & Scope.MetadataAttributeUsages.Count > 0)
            {
                // add metadataAttributes to packages
                if (Scope.MetadataAttributeUsages.Where(p => p.MetadataAttribute == TaxonomicTerm).Count() <= 0)
                    mdpManager.AddMetadataAtributeUsage(Scope, TaxonomicTerm, "TaxonomicTerm", "A container for taxonomic terms describing the data source.", 0, 10);

                if (Scope.MetadataAttributeUsages.Where(p => p.MetadataAttribute == GeoEcologicalTerm).Count() <= 0)
                    mdpManager.AddMetadataAtributeUsage(Scope, GeoEcologicalTerm, "GeoEcologicalTerm", "A container for geoecological terms describing the data source.", 0, 10);

            }
            else
            {
                mdpManager.AddMetadataAtributeUsage(Scope, TaxonomicTerm, "TaxonomicTerm", "A container for taxonomic terms describing the data source.", 0, 10);
                mdpManager.AddMetadataAtributeUsage(Scope, GeoEcologicalTerm, "GeoEcologicalTerm", "A container for geoecological terms describing the data source.", 0, 10);
            }


            #endregion

            #region Unit (ABCD Part)

            MetadataPackage Unit = mdpManager.MetadataPackageRepo.Get(p => p.Name == "Unit").FirstOrDefault();
            if (Unit == null) Unit = mdpManager.Create("Unit", "A container for all data referring to a unit (specimen or observation record).", true);

            if (abcd.MetadataPackageUsages != null && abcd.MetadataPackageUsages.Count > 0)
            {
                if (abcd.MetadataPackageUsages.Where(p => p.MetadataPackage == Unit).Count() <= 0)
                {
                    mdsManager.AddMetadataPackageUsage(abcd, Unit, "Unit", "A container for one or more unit data records from the gathering project.", 1, 5);
                }

            }
            else
            {

                mdsManager.AddMetadataPackageUsage(abcd, Unit, "Unit", "A container for one or more unit data records from the gathering project.", 1, 5);
            }

            // metadata attributes for Unit
            MetadataAttribute Id = mdaManager.MetadataAttributeRepo.Get(p => p.Name.Equals("Id")).FirstOrDefault();
            if (Id == null)
            {
                DataType dataType = dataTypeManager.Repo.Get(p => p.SystemType.Equals("String") && p.Name.ToLower().Equals("string")).FirstOrDefault();
                Unit unit = unitManager.Repo.Get(p => p.Name.Equals("None")).FirstOrDefault();

                Id = mdaManager.Create("Id", "Id", "Name or code of the data source", false, false, "David Blaa",
                        MeasurementScale.Categorial, DataContainerType.ValueType, "", dataType, unit, null, null, null, null);
            }

            // MetadataAttribute Usage
            if (Unit.MetadataAttributeUsages != null & Unit.MetadataAttributeUsages.Count > 0)
            {
                // add metadataAttributes to packages
                if (Unit.MetadataAttributeUsages.Where(p => p.MetadataAttribute == Id).Count() <= 0)
                {

                    MetadataAttributeUsage mau = mdpManager.AddMetadataAtributeUsage(Unit, Id, "SourceInstitutionID", "The unique identifier (code or name) of the institution holding the original data source.", 1, 1);
                }

                if (Unit.MetadataAttributeUsages.Where(p => p.MetadataAttribute == Id).Count() <= 0)
                    mdpManager.AddMetadataAtributeUsage(Unit, Id, "SourceID", "The name or code of the data source.", 1, 1);

                if (Unit.MetadataAttributeUsages.Where(p => p.MetadataAttribute == Id).Count() <= 0)
                    mdpManager.AddMetadataAtributeUsage(Unit, Id, "UnitID", "A unique identifier for the unit record within the data source.", 1, 1);

            }
            else
            {
                mdpManager.AddMetadataAtributeUsage(Unit, Id, "SourceInstitutionID", "The unique identifier (code or name) of the institution holding the original data source.", 1, 1);
                mdpManager.AddMetadataAtributeUsage(Unit, Id, "SourceID", "The name or code of the data source.", 1, 1);
                mdpManager.AddMetadataAtributeUsage(Unit, Id, "UnitID", "A unique identifier for the unit record within the data source.", 1, 1);
            }


            #endregion

            #endregion
        }

        #region helper

        private static XmlDocument AddReferenceToMetadatStructure(MetadataStructure metadataStructure, string nodeName, string nodePath, string nodeType, string destinationPath, XmlDocument xmlDoc)
        {

            xmlDoc = XmlDatasetHelper.AddReferenceToXml(xmlDoc, nodeName, nodePath, nodeType, destinationPath);

            return xmlDoc;

        }

        #region extra xdoc
        /// <summary>
        /// </summary>
        /// <param name="id"></param>
        /// <param name="titlePath"></param>
        /// <param name="descriptionPath"></param>
        /// <param name="mappingFilePath"></param>
        /// <param name="direction"></param>
        private static void StoreParametersToMetadataStruture(long id, string titlePath, string descriptionPath, string entity, string mappingFilePathImport, string mappingFilePathExport)
        {
            MetadataStructureManager mdsManager = new MetadataStructureManager();
            MetadataStructure metadataStructure = mdsManager.Repo.Get(id);

            XmlDocument xmlDoc = new XmlDocument();

            if (metadataStructure.Extra != null)
            {
                xmlDoc = (XmlDocument)metadataStructure.Extra;
            }

            // add title Node
            xmlDoc = AddReferenceToMetadatStructure("title", titlePath, AttributeType.xpath.ToString(), "extra/nodeReferences/nodeRef", xmlDoc);
            // add Description
            xmlDoc = AddReferenceToMetadatStructure("description", descriptionPath, AttributeType.xpath.ToString(), "extra/nodeReferences/nodeRef", xmlDoc);

            xmlDoc = AddReferenceToMetadatStructure("entity", entity, AttributeType.entity.ToString(), "extra/entity", xmlDoc);

            // add mappingFilePath
            xmlDoc = AddReferenceToMetadatStructure(metadataStructure.Name, mappingFilePathImport, "mappingFileImport", "extra/convertReferences/convertRef", xmlDoc);
            xmlDoc = AddReferenceToMetadatStructure(metadataStructure.Name, mappingFilePathExport, "mappingFileExport", "extra/convertReferences/convertRef", xmlDoc);

            //set active
            xmlDoc = AddReferenceToMetadatStructure(NameAttributeValues.active.ToString(), true.ToString(), AttributeType.parameter.ToString(), "extra/parameters/parameter", xmlDoc);

            metadataStructure.Extra = xmlDoc;
            mdsManager.Update(metadataStructure);

        }

        private static XmlDocument AddReferenceToMetadatStructure(string nodeName, string nodePath, string nodeType, string destinationPath, XmlDocument xmlDoc)
        {

            XmlDocument doc = XmlDatasetHelper.AddReferenceToXml(xmlDoc, nodeName, nodePath, nodeType, destinationPath);

            return doc;

        }

        #endregion

        #endregion

        #endregion


    }
}