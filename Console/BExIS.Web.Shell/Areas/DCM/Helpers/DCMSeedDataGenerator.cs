using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml;
using BExIS.Dlm.Entities.DataStructure;
using BExIS.Dlm.Entities.MetadataStructure;
using BExIS.Dlm.Services.DataStructure;
using BExIS.Dlm.Services.MetadataStructure;
using BExIS.Xml.Services;

namespace BExIS.Web.Shell.Areas.DCM.Helpers
{
    public class DCMSeedDataGenerator
    {

        public static void GenerateSeedData()
        {
            CreateEmlDatasetAdv();
        }

        private static void CreateEmlBasic()
        {
            MetadataStructureManager mdsManager = new MetadataStructureManager();
            MetadataPackageManager mdpManager = new MetadataPackageManager();
            MetadataAttributeManager mdaManager = new MetadataAttributeManager();

            DataTypeManager dataTypeManager = new DataTypeManager();
            UnitManager unitManager = new UnitManager();

            MetadataStructure abcd = mdsManager.Repo.Get(p => p.Name == "ABCD").FirstOrDefault();
            if (abcd == null) abcd = mdsManager.Create("ABCD", "This is the ABCD structure", "", "", null);

            MetadataStructure eml = mdsManager.Repo.Get(p => p.Name == "EML").FirstOrDefault();

            if (eml == null) eml = mdsManager.Create("EML", "This is the EML structure", "", "", null);

            XmlDocument xmlDoc = new XmlDocument();

            if (eml.Extra != null)
            {
                xmlDoc = (XmlDocument)eml.Extra;
            }

            // add title Node
            xmlDoc = AddReferenceToMetadatStructure(eml, "title", "Metadata/Description/DescriptionEML/Title/Title", "extra/nodeReferences/nodeRef", xmlDoc);

            // add ConvertReference Mapping file node
            xmlDoc = AddReferenceToMetadatStructure(eml, "mappingFile", "mapping_eml.xml", "extra/convertReferences/convertRef", xmlDoc);

            eml.Extra = xmlDoc;
            mdsManager.Update(eml);

            //package Description for title
            MetadataPackage DescEml = mdpManager.MetadataPackageRepo.Get(p => p.Name == "DescriptionEML").FirstOrDefault();
            if (DescEml == null) DescEml = mdpManager.Create("DescriptionEML", "DescriptionEML", true);

            //package PersonEML ( Creator / Contact)
            MetadataPackage personEml = mdpManager.MetadataPackageRepo.Get(p => p.Name == "PersonEML").FirstOrDefault();
            if (personEml == null) personEml = mdpManager.Create("PersonEML", "PersonEML", true);

            //package PersonEML ( Creator / Contact)
            MetadataPackage projectEml = mdpManager.MetadataPackageRepo.Get(p => p.Name == "ProjectEML").FirstOrDefault();
            if (projectEml == null) projectEml = mdpManager.Create("ProjectEML", "PersonEML", true);

            // add package to structure
            if (eml.MetadataPackageUsages != null && eml.MetadataPackageUsages.Count > 0)
            {
                if (eml.MetadataPackageUsages.Where(p => p.MetadataPackage == DescEml).Count() <= 0)
                    mdsManager.AddMetadataPackageUsage(eml, DescEml, "Description", "", 1, 1);

                if (eml.MetadataPackageUsages.Where(p => p.MetadataPackage == personEml).Count() <= 0)
                    mdsManager.AddMetadataPackageUsage(eml, personEml, "Creator", "", 1, 5);

                if (eml.MetadataPackageUsages.Where(p => p.MetadataPackage == personEml).Count() <= 0)
                    mdsManager.AddMetadataPackageUsage(eml, personEml, "Contact", "", 1, 5);

                if (eml.MetadataPackageUsages.Where(p => p.MetadataPackage == projectEml).Count() <= 0)
                    mdsManager.AddMetadataPackageUsage(eml, projectEml, "Project", "", 1, 1);
            }
            else
            {
                mdsManager.AddMetadataPackageUsage(eml, DescEml, "Description", "", 1, 1);
                mdsManager.AddMetadataPackageUsage(eml, personEml, "Creator", "", 1, 5);
                mdsManager.AddMetadataPackageUsage(eml, personEml, "Contact", "", 1, 5);
                mdsManager.AddMetadataPackageUsage(eml, projectEml, "Project", "", 1, 1);
            }

            #region Description EML

            MetadataAttribute Title = mdaManager.MetadataAttributeRepo.Get(p => p.Name.Equals("Title")).FirstOrDefault();
            if (Title == null)
            {
                DataType dataType = dataTypeManager.Repo.Get(p => p.SystemType.Equals("String")).FirstOrDefault();
                Unit unit = unitManager.Repo.Get(p => p.Name.Equals("None")).FirstOrDefault();

                Title = mdaManager.Create("Title", "Title", "Title", false, false, "David Blaa",
                        MeasurementScale.Categorial, DataContainerType.ValueType, "", dataType, unit, null, null, null, null);
            }

            if (DescEml.MetadataAttributeUsages != null & DescEml.MetadataAttributeUsages.Count > 0)
            {
                // add metadataAttributes to packages
                if (DescEml.MetadataAttributeUsages.Where(p => p.MetadataAttribute == Title).Count() <= 0)
                    mdpManager.AddMetadataAtributeUsage(DescEml, Title, "Title", 1, 1);

            }
            else
            {
                mdpManager.AddMetadataAtributeUsage(DescEml, Title, "Title", 1, 1);
            }

            #endregion

            #region Peronal EML

            MetadataAttribute Name = mdaManager.MetadataAttributeRepo.Get(p => p.Name.Equals("Name")).FirstOrDefault();
            if (Name == null)
            {
                DataType dataType = dataTypeManager.Repo.Get(p => p.SystemType.Equals("String")).FirstOrDefault();
                Unit unit = unitManager.Repo.Get(p => p.Name.Equals("None")).FirstOrDefault();

                Name = mdaManager.Create("Name", "Name", "first and last name", false, false, "David Blaa",
                        MeasurementScale.Categorial, DataContainerType.ValueType, "", dataType, unit, null, null, null, null);
            }

            if (Name == null)
            {
                DataType dataType = dataTypeManager.Repo.Get(p => p.SystemType.Equals("String")).FirstOrDefault();
                Unit unit = unitManager.Repo.Get(p => p.Name.Equals("None")).FirstOrDefault();

                Name = mdaManager.Create("Name", "Name", "first and last name", false, false, "David Blaa",
                        MeasurementScale.Categorial, DataContainerType.ValueType, "", dataType, unit, null, null, null, null);
            }

            if (personEml.MetadataAttributeUsages != null & personEml.MetadataAttributeUsages.Count > 0)
            {
                // add metadataAttributes to packages
                if (personEml.MetadataAttributeUsages.Where(p => p.MetadataAttribute == Name).Count() <= 0)
                    mdpManager.AddMetadataAtributeUsage(personEml, Name, "Given name", 1, 1);

                if (personEml.MetadataAttributeUsages.Where(p => p.MetadataAttribute == Name).Count() <= 0)
                    mdpManager.AddMetadataAtributeUsage(personEml, Name, "Sur name", 1, 1);

                if (personEml.MetadataAttributeUsages.Where(p => p.MetadataAttribute == Name).Count() <= 0)
                    mdpManager.AddMetadataAtributeUsage(personEml, Name, "Organization", 1, 1);

            }
            else
            {
                mdpManager.AddMetadataAtributeUsage(personEml, Name, "Given name", 1, 1);
                mdpManager.AddMetadataAtributeUsage(personEml, Name, "Sur name", 1, 1);
                mdpManager.AddMetadataAtributeUsage(personEml, Name, "Organization", 1, 1);
            }

            #endregion

            #region Project Eml

            MetadataAttribute DescriptionAttr = mdaManager.MetadataAttributeRepo.Get(p => p.Name.Equals("Description")).FirstOrDefault();
            if (DescriptionAttr == null)
            {
                DataType dataType = dataTypeManager.Repo.Get(p => p.SystemType.Equals("String")).FirstOrDefault();
                Unit unit = unitManager.Repo.Get(p => p.Name.Equals("None")).FirstOrDefault();

                DescriptionAttr = mdaManager.Create("Description", "Description", "Description", false, false, "David Blaa",
                        MeasurementScale.Categorial, DataContainerType.ValueType, "", dataType, unit, null, null, null, null);
            }

            MetadataAttribute Role = mdaManager.MetadataAttributeRepo.Get(p => p.Name.Equals("Role")).FirstOrDefault();
            if (Role == null)
            {
                DataType dataType = dataTypeManager.Repo.Get(p => p.SystemType.Equals("String")).FirstOrDefault();
                Unit unit = unitManager.Repo.Get(p => p.Name.Equals("None")).FirstOrDefault();

                Role = mdaManager.Create("Role", "Role", "Role", false, false, "David Blaa",
                        MeasurementScale.Categorial, DataContainerType.ValueType, "", dataType, unit, null, null, null, null);
            }

            if (projectEml.MetadataAttributeUsages != null & projectEml.MetadataAttributeUsages.Count > 0)
            {
                if (Title == null)
                {
                    DataType dataType = dataTypeManager.Repo.Get(p => p.SystemType.Equals("String")).FirstOrDefault();
                    Unit unit = unitManager.Repo.Get(p => p.Name.Equals("None")).FirstOrDefault();

                    Title = mdaManager.Create("Title", "Title", "Title", false, false, "David Blaa",
                            MeasurementScale.Categorial, DataContainerType.ValueType, "", dataType, unit, null, null, null, null);
                }

                // add metadataAttributes to packages
                if (projectEml.MetadataAttributeUsages.Where(p => p.MetadataAttribute == Title).Count() <= 0)
                    mdpManager.AddMetadataAtributeUsage(projectEml, Title, "Title", 0, 1);

                if (projectEml.MetadataAttributeUsages.Where(p => p.MetadataAttribute == Name).Count() <= 0)
                    mdpManager.AddMetadataAtributeUsage(projectEml, Name, "Personnel given name", 0, 1);

                if (projectEml.MetadataAttributeUsages.Where(p => p.MetadataAttribute == Name).Count() <= 0)
                    mdpManager.AddMetadataAtributeUsage(projectEml, Name, "Personnel sur name", 0, 1);

                if (projectEml.MetadataAttributeUsages.Where(p => p.MetadataAttribute == Role).Count() <= 0)
                    mdpManager.AddMetadataAtributeUsage(projectEml, Role, "Role", 0, 1);

                if (projectEml.MetadataAttributeUsages.Where(p => p.MetadataAttribute == DescriptionAttr).Count() <= 0)
                    mdpManager.AddMetadataAtributeUsage(projectEml, DescriptionAttr, "Project description", 0, 1);

            }
            else
            {
                mdpManager.AddMetadataAtributeUsage(projectEml, Title, "Title", 0, 1);
                mdpManager.AddMetadataAtributeUsage(projectEml, Name, "Personnel given name", 0, 1);
                mdpManager.AddMetadataAtributeUsage(projectEml, Name, "Personnel sur name", 0, 1);
                mdpManager.AddMetadataAtributeUsage(projectEml, Role, "Role", 0, 1);
                mdpManager.AddMetadataAtributeUsage(projectEml, DescriptionAttr, "Project description", 0, 1);
            }

            #endregion
        }

        private static void CreateEmlDatasetAdv()
        {
            MetadataStructureManager mdsManager = new MetadataStructureManager();
            MetadataPackageManager mdpManager = new MetadataPackageManager();
            MetadataAttributeManager mdaManager = new MetadataAttributeManager();

            DataTypeManager dataTypeManager = new DataTypeManager();
            UnitManager unitManager = new UnitManager();

            MetadataStructure eml = mdsManager.Repo.Get(p => p.Name == "EML Dataset").FirstOrDefault();

            if (eml == null) eml = mdsManager.Create("EML Dataset", "This is the EML structure", "", "", null);

            XmlDocument xmlDoc = new XmlDocument();

            if (eml.Extra != null)
            {
                xmlDoc = (XmlDocument)eml.Extra;
            }

            // add title Node
            xmlDoc = AddReferenceToMetadatStructure(eml, "title", "Metadata/Description/DescriptionEML/Title/Title", "extra/nodeReferences/nodeRef", xmlDoc);

            // add ConvertReference Mapping file node
            xmlDoc = AddReferenceToMetadatStructure(eml, "mappingFile", "mapping_eml.xml", "extra/convertReferences/convertRef", xmlDoc);

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
                    mdsManager.AddMetadataPackageUsage(eml, DescEml, "Description", "", 1, 1);

                if (eml.MetadataPackageUsages.Where(p => p.MetadataPackage == personEml).Count() <= 0)
                    mdsManager.AddMetadataPackageUsage(eml, personEml, "Creator", "", 1, 5);

                if (eml.MetadataPackageUsages.Where(p => p.MetadataPackage == party).Count() <= 0)
                    mdsManager.AddMetadataPackageUsage(eml, party, "Associated Party ", "", 1, 10);

                if (eml.MetadataPackageUsages.Where(p => p.MetadataPackage == personEml).Count() <= 0)
                    mdsManager.AddMetadataPackageUsage(eml, personEml, "Contact", "", 1, 5);

                if (eml.MetadataPackageUsages.Where(p => p.MetadataPackage == projectEml).Count() <= 0)
                    mdsManager.AddMetadataPackageUsage(eml, projectEml, "Project", "", 1, 1);

                //if (eml.MetadataPackageUsages.Where(p => p.MetadataPackage == coverage).Count() <= 0)
                //    mdsManager.AddMetadataPackageUsage(eml, coverage, "Coverage", 1, 1);
            }
            else
            {
                mdsManager.AddMetadataPackageUsage(eml, DescEml, "Description", "", 1, 1);
                mdsManager.AddMetadataPackageUsage(eml, personEml, "Creator", "", 1, 5);
                mdsManager.AddMetadataPackageUsage(eml, party, "Associated Parties ", "", 1, 10);
                mdsManager.AddMetadataPackageUsage(eml, personEml, "Contact", "", 1, 5);
                mdsManager.AddMetadataPackageUsage(eml, projectEml, "Project", "", 1, 1);
                //mdsManager.AddMetadataPackageUsage(eml, coverage, "Coverage", 1, 1);
            }

            #endregion

            #region Description EML

            #region create attr

            MetadataAttribute Name = mdaManager.MetadataAttributeRepo.Get(p => p.Name.Equals("Name")).FirstOrDefault();
            if (Name == null)
            {
                DataType dataType = dataTypeManager.Repo.Get(p => p.SystemType.Equals("String")).FirstOrDefault();
                Unit unit = unitManager.Repo.Get(p => p.Name.Equals("None")).FirstOrDefault();

                Name = mdaManager.Create("Name", "Name", "first and last name", false, false, "David Blaa",
                        MeasurementScale.Categorial, DataContainerType.ValueType, "", dataType, unit, null, null, null, null);
            }

            MetadataAttribute Title = mdaManager.MetadataAttributeRepo.Get(p => p.Name.Equals("Title")).FirstOrDefault();
            if (Title == null)
            {
                DataType dataType = dataTypeManager.Repo.Get(p => p.SystemType.Equals("String")).FirstOrDefault();
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
                DataType dataType = dataTypeManager.Repo.Get(p => p.SystemType.Equals("String") && p.Name.Equals("Text")).FirstOrDefault();
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
                    mdpManager.AddMetadataAtributeUsage(DescEml, Name, "Short Name", 0, 1);
                }

                if (DescEml.MetadataAttributeUsages.Where(p => p.MetadataAttribute == Title).Count() <= 0)
                    mdpManager.AddMetadataAtributeUsage(DescEml, Title, "Title", 1, 1);

                if (DescEml.MetadataAttributeUsages.Where(p => p.MetadataAttribute == Date).Count() <= 0)
                    mdpManager.AddMetadataAtributeUsage(DescEml, Date, "Publish Date", 0, 1);

                if (DescEml.MetadataAttributeUsages.Where(p => p.MetadataAttribute == Info).Count() <= 0)
                    mdpManager.AddMetadataAtributeUsage(DescEml, Info, "Additional Information", 0, 1);


            }
            else
            {
                mdpManager.AddMetadataAtributeUsage(DescEml, Name, "Short Name", 0, 1);
                mdpManager.AddMetadataAtributeUsage(DescEml, Title, "Title", 1, 1);
                mdpManager.AddMetadataAtributeUsage(DescEml, Date, "Publish Date", 0, 1);
                mdpManager.AddMetadataAtributeUsage(DescEml, Info, "Additional Information", 0, 1);
            }


            #endregion

            #endregion

            #region Peronal EML

            #region create attr

            Name = mdaManager.MetadataAttributeRepo.Get(p => p.Name.Equals("Name")).FirstOrDefault();
            if (Name == null)
            {
                DataType dataType = dataTypeManager.Repo.Get(p => p.SystemType.Equals("String")).FirstOrDefault();
                Unit unit = unitManager.Repo.Get(p => p.Name.Equals("None")).FirstOrDefault();

                Name = mdaManager.Create("Name", "Name", "first and last name", false, false, "David Blaa",
                        MeasurementScale.Categorial, DataContainerType.ValueType, "", dataType, unit, null, null, null, null);
            }

            #endregion

            #region add attr

            if (Name == null)
            {
                DataType dataType = dataTypeManager.Repo.Get(p => p.SystemType.Equals("String")).FirstOrDefault();
                Unit unit = unitManager.Repo.Get(p => p.Name.Equals("None")).FirstOrDefault();

                Name = mdaManager.Create("Name", "Name", "first and last name", false, false, "David Blaa",
                        MeasurementScale.Categorial, DataContainerType.ValueType, "", dataType, unit, null, null, null, null);
            }

            if (personEml.MetadataAttributeUsages != null & personEml.MetadataAttributeUsages.Count > 0)
            {
                // add metadataAttributes to packages
                if (personEml.MetadataAttributeUsages.Where(p => p.MetadataAttribute == Name).Count() <= 0)
                    mdpManager.AddMetadataAtributeUsage(personEml, Name, "Given name", 1, 1);

                if (personEml.MetadataAttributeUsages.Where(p => p.MetadataAttribute == Name).Count() <= 0)
                    mdpManager.AddMetadataAtributeUsage(personEml, Name, "Surname", 1, 1);

                if (personEml.MetadataAttributeUsages.Where(p => p.MetadataAttribute == Name).Count() <= 0)
                    mdpManager.AddMetadataAtributeUsage(personEml, Name, "Organization", 1, 1);

            }
            else
            {
                mdpManager.AddMetadataAtributeUsage(personEml, Name, "Given name", 1, 1);
                mdpManager.AddMetadataAtributeUsage(personEml, Name, "Sur name", 1, 1);
                mdpManager.AddMetadataAtributeUsage(personEml, Name, "Organization", 1, 1);
            }

            #endregion

            #endregion

            #region Peronal with role EML

            #region create attr

            Name = mdaManager.MetadataAttributeRepo.Get(p => p.Name.Equals("Name")).FirstOrDefault();
            if (Name == null)
            {
                DataType dataType = dataTypeManager.Repo.Get(p => p.SystemType.Equals("String")).FirstOrDefault();
                Unit unit = unitManager.Repo.Get(p => p.Name.Equals("None")).FirstOrDefault();

                Name = mdaManager.Create("Name", "Name", "first and last name", false, false, "David Blaa",
                        MeasurementScale.Categorial, DataContainerType.ValueType, "", dataType, unit, null, null, null, null);
            }

            MetadataAttribute RoleType = mdaManager.MetadataAttributeRepo.Get(p => p.Name.Equals("RoleType")).FirstOrDefault();
            if (RoleType == null)
            {
                DataType dataType = dataTypeManager.Repo.Get(p => p.SystemType.Equals("String")).FirstOrDefault();
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
                    mdpManager.AddMetadataAtributeUsage(party, Name, "Given name", 1, 1);

                if (party.MetadataAttributeUsages.Where(p => p.MetadataAttribute == Name).Count() <= 0)
                    mdpManager.AddMetadataAtributeUsage(party, Name, "Surname", 1, 1);

                if (party.MetadataAttributeUsages.Where(p => p.MetadataAttribute == Name).Count() <= 0)
                    mdpManager.AddMetadataAtributeUsage(party, Name, "Organization", 1, 1);

                if (party.MetadataAttributeUsages.Where(p => p.MetadataAttribute == Name).Count() <= 0)
                    mdpManager.AddMetadataAtributeUsage(party, RoleType, "Role", 1, 1);

            }
            else
            {
                mdpManager.AddMetadataAtributeUsage(party, Name, "Given name", 1, 1);
                mdpManager.AddMetadataAtributeUsage(party, Name, "Sur name", 1, 1);
                mdpManager.AddMetadataAtributeUsage(party, Name, "Organization", 1, 1);
                mdpManager.AddMetadataAtributeUsage(party, RoleType, "Role", 1, 1);
            }

            #endregion

            #endregion

            #region Project Eml

            #region create attr

            MetadataAttribute DescriptionAttr = mdaManager.MetadataAttributeRepo.Get(p => p.Name.Equals("Description")).FirstOrDefault();
            if (DescriptionAttr == null)
            {
                DataType dataType = dataTypeManager.Repo.Get(p => p.SystemType.Equals("String") && p.Name.Equals("Text")).FirstOrDefault();
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
                    DataType dataType = dataTypeManager.Repo.Get(p => p.SystemType.Equals("String")).FirstOrDefault();
                    Unit unit = unitManager.Repo.Get(p => p.Name.Equals("None")).FirstOrDefault();

                    Title = mdaManager.Create("Title", "Title", "Title", false, false, "David Blaa",
                            MeasurementScale.Categorial, DataContainerType.ValueType, "", dataType, unit, null, null, null, null);
                }

                RoleType = mdaManager.MetadataAttributeRepo.Get(p => p.Name.Equals("RoleType")).FirstOrDefault();
                if (RoleType == null)
                {
                    DataType dataType = dataTypeManager.Repo.Get(p => p.SystemType.Equals("String")).FirstOrDefault();
                    Unit unit = unitManager.Repo.Get(p => p.Name.Equals("None")).FirstOrDefault();

                    RoleType = mdaManager.Create("Role", "RoleType", "Use this field to describe the role the party played with respect to the resource. Some potential roles include technician, reviewer, principal investigator, and many others.", false, false, "David Blaa",
                        MeasurementScale.Categorial, DataContainerType.ValueType, "", dataType, unit, null, null, null, null);
                }

                // add metadataAttributes to packages
                if (projectEml.MetadataAttributeUsages.Where(p => p.MetadataAttribute == Title).Count() <= 0)
                    mdpManager.AddMetadataAtributeUsage(projectEml, Title, "Title", 0, 1);

                if (projectEml.MetadataAttributeUsages.Where(p => p.MetadataAttribute == Name).Count() <= 0)
                    mdpManager.AddMetadataAtributeUsage(projectEml, Name, "Personnel given name", 0, 1);

                if (projectEml.MetadataAttributeUsages.Where(p => p.MetadataAttribute == Name).Count() <= 0)
                    mdpManager.AddMetadataAtributeUsage(projectEml, Name, "Personnel surname", 0, 1);

                if (projectEml.MetadataAttributeUsages.Where(p => p.MetadataAttribute == RoleType).Count() <= 0)
                    mdpManager.AddMetadataAtributeUsage(projectEml, RoleType, "Role", 0, 1);

                if (projectEml.MetadataAttributeUsages.Where(p => p.MetadataAttribute == DescriptionAttr).Count() <= 0)
                    mdpManager.AddMetadataAtributeUsage(projectEml, DescriptionAttr, "Project description", 0, 1);

            }
            else
            {
                mdpManager.AddMetadataAtributeUsage(projectEml, Title, "Title", 0, 1);
                mdpManager.AddMetadataAtributeUsage(projectEml, Name, "Personnel given name", 0, 1);
                mdpManager.AddMetadataAtributeUsage(projectEml, Name, "Personnel surname", 0, 1);
                mdpManager.AddMetadataAtributeUsage(projectEml, RoleType, "Role", 0, 1);
                mdpManager.AddMetadataAtributeUsage(projectEml, DescriptionAttr, "Project description", 0, 1);
            }

            #endregion

            #endregion
        }

        #region helper

        private static XmlDocument AddReferenceToMetadatStructure(MetadataStructure metadataStructure, string nodeName, string nodePath, string destinationPath, XmlDocument xmlDoc)
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

            XmlNode x = createMissingNodes(destinationPath, doc.DocumentElement, doc);

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

        /// <summary>
        /// Add missing node to the desitnation document
        /// </summary>
        /// <param name="destinationParentXPath"></param>
        /// <param name="currentParentXPath"></param>
        /// <param name="parentNode"></param>
        /// <param name="doc"></param>
        /// <returns></returns>
        private static XmlNode createMissingNodes(string destinationParentXPath, XmlNode parentNode, XmlDocument doc)
        {
            string dif = destinationParentXPath;

            List<string> temp = dif.Split('/').ToList();
            temp.RemoveAt(0);

            XmlNode parentTemp = parentNode;

            foreach (string s in temp)
            {
                if (XmlUtility.GetXmlNodeByName(parentTemp, s) == null)
                {
                    XmlNode t = XmlUtility.CreateNode(s, doc);

                    parentTemp.AppendChild(t);
                    parentTemp = t;
                }
                else
                {
                    XmlNode t = XmlUtility.GetXmlNodeByName(parentTemp, s);
                    parentTemp = t;
                }
            }

            return parentTemp;
        }

        #endregion
    }
}