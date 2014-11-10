using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Xml;
using System.Xml.Linq;
using BExIS.Dlm.Entities.Administration;
using BExIS.Dlm.Entities.DataStructure;
using BExIS.Dlm.Entities.MetadataStructure;
using BExIS.Dlm.Services.Administration;
using BExIS.Dlm.Services.DataStructure;
using BExIS.Dlm.Services.MetadataStructure;
using BExIS.Security.Entities.Authentication;
using BExIS.Security.Entities.Objects;
using BExIS.Security.Entities.Subjects;
using BExIS.Security.Services.Authentication;
using BExIS.Security.Services.Authorization;
using BExIS.Security.Services.Objects;
using BExIS.Security.Services.Subjects;
using BExIS.Xml.Services;
using Vaiona.Util.Cfg;

namespace BExIS.Web.Shell.Areas.SAM.Controllers
{
    public class SeedController : Controller
    {
        //
        // GET: /SAM/Seed/

        public ActionResult Index()
        {
            return View();
        }

        #region SEED

        public ActionResult CreateSeedData()
        {

            CreateSeedDataTypes();

            //security
            CreateSecuritySeedData();


            //ResearchPlan
            ResearchPlanManager rpm = new ResearchPlanManager();
            ResearchPlan researchPlan = rpm.Repo.Get(r => r.Title.Equals("Research plan")).FirstOrDefault();
            if (researchPlan == null) rpm.Create("Research plan", "");

            //Datatypes
            CreateSeedDataTypes();

            //Unit
            UnitManager unitManager = new UnitManager();
            Unit unit = unitManager.Repo.Get(p => p.Name.Equals("None")).FirstOrDefault();
            if (unit == null) unitManager.Create("None", "None", "If no unit is used.", "None", MeasurementSystem.Unknown);

            return View("Index");
        }

        private void CreateSecuritySeedData()
        {
#if DEBUG
            #region Security

            // Authenticators
            AuthenticatorManager authenticatorManager = new AuthenticatorManager();

            Authenticator a1 = authenticatorManager.CreateAuthenticator("local", "BExIS.Security.Services.Authentication.BuiltInAuthenticationManager", "BExIS.Security.Services", "", AuthenticatorType.Internal);
            Authenticator a2 = authenticatorManager.CreateAuthenticator("ldap test server", "BExIS.Security.Services.Authentication.LdapAuthenticationManager", "BExIS.Security.Services", "ldapHost:ldap.forumsys.com;ldapPort:389;ldapBaseDn:dc=example,dc=com;ldapSecure:false;ldapAuthUid:uid;ldapProtocolVersion:3", AuthenticatorType.External);

            // Security Questions
            SecurityQuestionManager securityQuestionManager = new SecurityQuestionManager();

            SecurityQuestion sq1 = securityQuestionManager.CreateSecurityQuestion("What is the first name of the person you first kissed?");
            SecurityQuestion sq2 = securityQuestionManager.CreateSecurityQuestion("What was your favorite place to visit as a child?");
            SecurityQuestion sq3 = securityQuestionManager.CreateSecurityQuestion("What is the name of the place your wedding reception was held?");
            SecurityQuestion sq4 = securityQuestionManager.CreateSecurityQuestion("In what city or town did you meet your spouse/partner?");
            SecurityQuestion sq5 = securityQuestionManager.CreateSecurityQuestion("What was the make and model of your first car?");
            SecurityQuestion sq6 = securityQuestionManager.CreateSecurityQuestion("What was the name of your elementary / primary school?");

            // Entities
            EntityManager entityManager = new EntityManager();

            entityManager.CreateEntity("Dataset", "BExIS.Dlm.Entities.Data.Dataset", "BExIS.Dlm.Entities");

            // Subjects
            SubjectManager subjectManager = new SubjectManager();

            Group g1 = subjectManager.CreateGroup("Admin", "Admin");
            User u1 = subjectManager.CreateUser("Administrator", "gWg2xG", "Admin", "admin@bexis.de", sq1.Id, "Nothing", a1.Id);

            subjectManager.AddUserToGroup(u1.Id, g1.Id);

            // Tasks
            TaskManager taskManager = new TaskManager();

            taskManager.CreateTask("Auth", "Account", "*", true);
            taskManager.CreateTask("Site", "Nav", "*", true);
            taskManager.CreateTask("Shell", "Home", "*", true);
            taskManager.CreateTask("System", "Utils", "*", true);

            // Features
            FeatureManager featureManager = new FeatureManager();
            Feature f1 = featureManager.CreateFeature("BExIS", "BExIS");

            Feature f2 = featureManager.CreateFeature("Administration", "Administration", f1.Id);
            Feature f3 = featureManager.CreateFeature("Users Management", "Users Management", f2.Id);
            Feature f4 = featureManager.CreateFeature("Groups Management", "Groups Management", f2.Id);
            Feature f5 = featureManager.CreateFeature("Data Management", "Data Management", f2.Id);
            Feature f6 = featureManager.CreateFeature("Feature Management", "Feature Management", f2.Id);
            Feature f7 = featureManager.CreateFeature("Search", "Search", f2.Id);
            Feature f8 = featureManager.CreateFeature("Seed Data Creation", "Seed Data Creation", f2.Id);

            Feature f9 = featureManager.CreateFeature("Search", "Search", f1.Id);

            Feature f10 = featureManager.CreateFeature("Data Collection", "Data Collection", f1.Id);
            Feature f11 = featureManager.CreateFeature("Dataset Creation", "Dataset Creation", f10.Id);
            Feature f12 = featureManager.CreateFeature("Dataset Submission", "Dataset Submission", f10.Id);

            Feature f13 = featureManager.CreateFeature("Research Plan", "Research Plan", f1.Id);

            Feature f14 = featureManager.CreateFeature("Data Dissemination", "Data Dissemination", f1.Id);
            
            Task t1 = taskManager.CreateTask("Auth", "Users", "*");
            t1.Feature = f3;
            taskManager.UpdateTask(t1);
            Task t2 = taskManager.CreateTask("Auth", "Groups", "*");
            t2.Feature = f4;
            taskManager.UpdateTask(t2);
            Task t3 = taskManager.CreateTask("Auth", "DataPermissions", "*");
            t3.Feature = f5;
            taskManager.UpdateTask(t3);
            Task t4 = taskManager.CreateTask("Auth", "FeaturePermissions", "*");
            t4.Feature = f6;
            taskManager.UpdateTask(t4);
            Task t5 = taskManager.CreateTask("DDM", "Admin", "*");
            t5.Feature = f7;
            taskManager.UpdateTask(t5);
            Task t6 = taskManager.CreateTask("SAM", "Seed", "*");
            t6.Feature = f8;
            taskManager.UpdateTask(t6);


            Task t7 = taskManager.CreateTask("DDM", "Data", "*");
            t7.Feature = f9;
            taskManager.UpdateTask(t7);
            Task t8 = taskManager.CreateTask("DDM", "Home", "*");
            t8.Feature = f9;
            taskManager.UpdateTask(t8);

            Task t9 = taskManager.CreateTask("DCM", "Create", "*");
            t9.Feature = f11;
            taskManager.UpdateTask(t9);
            Task t10 = taskManager.CreateTask("DCM", "CreateSelectDatasetSetup", "*");
            t10.Feature = f11;
            taskManager.UpdateTask(t10);
            Task t11 = taskManager.CreateTask("DCM", "CreateSetMetadataPackage", "*");
            t11.Feature = f11;
            taskManager.UpdateTask(t11);
            Task t12 = taskManager.CreateTask("DCM", "CreateSummary", "*");
            t12.Feature = f11;
            taskManager.UpdateTask(t12);

            Task t15 = taskManager.CreateTask("DCM", "Push", "*");
            t15.Feature = f12;
            taskManager.UpdateTask(t15);
            Task t16 = taskManager.CreateTask("DCM", "Submit", "*");
            t16.Feature = f12;
            taskManager.UpdateTask(t16);
            Task t17 = taskManager.CreateTask("DCM", "SubmitDefinePrimaryKey", "*");
            t17.Feature = f12;
            taskManager.UpdateTask(t17);
            Task t18 = taskManager.CreateTask("DCM", "SubmitGetFileInformation", "*");
            t18.Feature = f12;
            taskManager.UpdateTask(t18);
            Task t19 = taskManager.CreateTask("DCM", "SubmitSelectAFile", "*");
            t19.Feature = f12;
            taskManager.UpdateTask(t19);
            Task t20 = taskManager.CreateTask("DCM", "SubmitSpecifyDataset", "*");
            t20.Feature = f12;
            taskManager.UpdateTask(t20);
            Task t21 = taskManager.CreateTask("DCM", "SubmitSummary", "*");
            t21.Feature = f12;
            taskManager.UpdateTask(t21);
            Task t22 = taskManager.CreateTask("DCM", "SubmitValidation", "*");
            t22.Feature = f12;
            taskManager.UpdateTask(t22);

            Task t23 = taskManager.CreateTask("RPM", "Home", "*");
            t23.Feature = f13;
            taskManager.UpdateTask(t23);

            Task t24 = taskManager.CreateTask("DIM", "Admin", "*");
            t24.Feature = f14;
            taskManager.UpdateTask(t24);

            // Feature Permissions
            PermissionManager permissionManager = new PermissionManager();
            permissionManager.CreateFeaturePermission(g1.Id, f1.Id);

            #endregion
#endif
        }

        private void CreateSeedDataTypes()
        {
            DataTypeManager dataTypeManager = new DataTypeManager();
            DataType dataType;

            try
            {
                string path = Path.Combine(AppConfiguration.GetModuleWorkspacePath("RPM"), "Seed", "DataTypes.xml");
                XDocument xdoc = XDocument.Load(path);

                IEnumerable<XElement> datatypesXElements = XmlUtility.GetXElementByNodeName("datatype", xdoc);

                if (datatypesXElements.Count() > 0)
                {

                    foreach (XElement xDatatype in datatypesXElements)
                    {
                        string name = xDatatype.Attribute("name").Value;
                        string description = xDatatype.Attribute("description").Value;
                        string systemtype = xDatatype.Attribute("typeCode").Value;

                        TypeCode systemType = TypeCode.String;

                        foreach (TypeCode type in Enum.GetValues(typeof(TypeCode)))
                        {
                            if (type.ToString().Equals(systemtype))
                            {
                                systemType = type;
                            }
                        }

                        dataType = dataTypeManager.Repo.Get().Where(d => d.Name.Equals(name)).FirstOrDefault();

                        if (dataType == null)
                        {
                            dataTypeManager.Create(name, description, systemType);
                        }

                    }
                }

            }
            catch (Exception ex)
            { 
            
            }

            ////Datatypes
            
            //DataType stringDataType = dataTypeManager.Repo.Get(p => p.Name.Equals("String")).FirstOrDefault();
            //if (stringDataType == null) dataTypeManager.Create("String", "A Unicode String", TypeCode.String);

            //DataType numberDataType = dataTypeManager.Repo.Get(p => p.Name.Equals("Number")).FirstOrDefault();
            //if (numberDataType == null) dataTypeManager.Create("Number", "An Integer Number", TypeCode.Int32);

            //DataType decimalDataType = dataTypeManager.Repo.Get(p => p.Name.Equals("Decimal")).FirstOrDefault();
            //if (decimalDataType == null) dataTypeManager.Create("Decimal", "A Real Number", TypeCode.Double);

            //DataType dateDataType = dataTypeManager.Repo.Get(p => p.Name.Equals("Date")).FirstOrDefault();
            //if (dateDataType == null) dataTypeManager.Create("Date", "A Date +Time", TypeCode.DateTime);

            //DataType textDataType = dataTypeManager.Repo.Get(p => p.Name.Equals("Text")).FirstOrDefault();
            //if (dateDataType == null) dataTypeManager.Create("Text", "for long text", TypeCode.String);
        }

        public ActionResult CreateSIUnits()
        {
            createSIUnits();

            return View("Index");
        }

        private void createSIUnits()
        {
            DataTypeManager dataTypeManager = new DataTypeManager();

            UnitManager unitManager = new UnitManager();
            Unit unit = unitManager.Repo.Get(p => p.Name.Equals("None")).FirstOrDefault();
            if (unit == null) unitManager.Create("None", "None", "If no unit is used.", "None", MeasurementSystem.Unknown);

            try
            {
                string path = Path.Combine(AppConfiguration.GetModuleWorkspacePath("RPM"), "Seed", "Units.xml");
                XDocument xdoc = XDocument.Load(path);

                IEnumerable<XElement> unitsXElements = XmlUtility.GetXElementByNodeName("unit", xdoc);

                foreach (XElement xUnit in unitsXElements)
                {
                    string name = xUnit.Attribute("name").Value;
                    string abbrevation = xUnit.Attribute("abbrevation").Value;
                    string mesurementSystem = xUnit.Attribute("mesurementSystem").Value;
                    string dimension = xUnit.Attribute("dimension").Value;
                    string description = xUnit.Attribute("description").Value;
                    string[] associatedDataTypes = xUnit.Attribute("associatedDataTypes").Value.Split(',');

                    MeasurementSystem measurementSystemEnum = MeasurementSystem.Unknown;

                    foreach (MeasurementSystem msCheck in Enum.GetValues(typeof(MeasurementSystem)))
                    {
                        if (msCheck.ToString().Equals(mesurementSystem))
                        {
                            measurementSystemEnum = msCheck;
                        }
                    }


                    if (description.Length > 250)
                    {
                        description = description.Substring(0, 250) + "...";
                    }

                    unit = unitManager.Repo.Get(p => p.Name.Equals(name)).FirstOrDefault();

                    if (unit == null)
                    {
                        unit = unitManager.Create(name, abbrevation, description, dimension, measurementSystemEnum);

                        foreach(string dtName in associatedDataTypes)
                        {
                            DataType dt = dataTypeManager.Repo.Get().Where(d=>d.Name.Equals(dtName)).FirstOrDefault();
                            if(dt!=null)
                            unit.AssociatedDataTypes.Add(dt);
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                Debug.WriteLine(ex.Message); 
            }

        }

        #endregion

        #region metadata

        public ActionResult CreateABCDMetadataStructure()
        {
            //MetadataStructureManager mdsManager = new MetadataStructureManager();
            //MetadataPackageManager mdpManager = new MetadataPackageManager();
            //MetadataAttributeManager mdaManager = new MetadataAttributeManager();

            //DataTypeManager dataTypeManager = new DataTypeManager();
            //UnitManager unitManager = new UnitManager();

            //#region ABCD

            //MetadataStructure abcd = mdsManager.Repo.Get(p => p.Name == "ABCD").FirstOrDefault();
            //if (abcd == null) abcd = mdsManager.Create("ABCD", "This is the ABCD structure", "", "", null);

            //XmlDocument xmlDoc = new XmlDocument();

            //if (abcd.Extra != null)
            //{
            //    xmlDoc = (XmlDocument)abcd.Extra;
            //}

            //// add title Node
            //xmlDoc = AddReferenceToMetadatStructure(abcd, "title", "Metadata/Description/Description/Title/Title", "extra/nodeReferences/nodeRef", xmlDoc);

            //// add ConvertReference Mapping file node
            //xmlDoc = AddReferenceToMetadatStructure(abcd, "mappingFile", "mapping_abcd.xml", "extra/convertReferences/convertRef", xmlDoc);

            //abcd.Extra = xmlDoc;
            //mdsManager.Update(abcd);

            ////package Person ( Tecnical contact /ContentContact)
            //MetadataPackage person = mdpManager.MetadataPackageRepo.Get(p => p.Name == "Person").FirstOrDefault();
            //if (person == null) person = mdpManager.Create("Person", "Person", true);

            ////package Description
            //MetadataPackage Description = mdpManager.MetadataPackageRepo.Get(p => p.Name == "Description").FirstOrDefault();
            //if (Description == null) Description = mdpManager.Create("Description", "Description about a dataset", true);

            ////package Owner
            //MetadataPackage Owner = mdpManager.MetadataPackageRepo.Get(p => p.Name == "Owner").FirstOrDefault();
            //if (Owner == null) Owner = mdpManager.Create("Owner", "Owner/s of the dataset", true);

            //// Package Scope
            //MetadataPackage Scope = mdpManager.MetadataPackageRepo.Get(p => p.Name == "Scope").FirstOrDefault();
            //if (Scope == null) Scope = mdpManager.Create("Scope", "Scope of the dataset", true);


            //// add package to structure
            //if (abcd.MetadataPackageUsages != null && abcd.MetadataPackageUsages.Count > 0)
            //{
            //    if (abcd.MetadataPackageUsages.Where(p => p.MetadataPackage == person).Count() <= 0)
            //        mdsManager.AddMetadataPackageUsage(abcd, person, "Content Contact", "A container element for several administrative contacts for the dataset.", 0, 3);

            //    if (abcd.MetadataPackageUsages.Where(p => p.MetadataPackage == Description).Count() <= 0)
            //        mdsManager.AddMetadataPackageUsage(abcd, Description, "Description", "Description of the data source queried.", 0, 1);

            //    if (abcd.MetadataPackageUsages.Where(p => p.MetadataPackage == Owner).Count() <= 0)
            //        mdsManager.AddMetadataPackageUsage(abcd, Owner, "Owner", "A container element for several owners of the data source.", 1, 5);

            //    if (abcd.MetadataPackageUsages.Where(p => p.MetadataPackage == Scope).Count() <= 0)
            //        mdsManager.AddMetadataPackageUsage(abcd, Scope, "Scope", "Keyword list describing the scope of the data source.", 0, 1);
            //}
            //else
            //{

            //    mdsManager.AddMetadataPackageUsage(abcd, person, "Technical Contact", "A technical contact normally representing the agent acting as the publisher of the dataset in the network.", 1, 1);
            //    mdsManager.AddMetadataPackageUsage(abcd, person, "Content Contact", "A container element for several administrative contacts for the dataset.", 1, 10);
            //    mdsManager.AddMetadataPackageUsage(abcd, Description, "Description", "Description of the data source queried.", 1, 1);
            //    mdsManager.AddMetadataPackageUsage(abcd, Owner, "Owner", "A container element for several owners of the data source.", 1, 10);
            //    mdsManager.AddMetadataPackageUsage(abcd, Scope, "Scope", "Keyword list describing the scope of the data source.", 0, 1);

            //}


            //#region person

            //MetadataAttribute Name = mdaManager.MetadataAttributeRepo.Get(p => p.Name.Equals("Name")).FirstOrDefault();
            //if (Name == null)
            //{
            //    DataType dataType = dataTypeManager.Repo.Get(p => p.SystemType.Equals("String")).FirstOrDefault();
            //    Unit unit = unitManager.Repo.Get(p => p.Name.Equals("None")).FirstOrDefault();

            //    Name = mdaManager.Create("Name", "Name", "first and last name", false, false, "David Blaa",
            //            MeasurementScale.Categorial, DataContainerType.ValueType, "", dataType, unit, null, null, null, null);
            //}

            //MetadataAttribute Email = mdaManager.MetadataAttributeRepo.Get(p => p.Name.Equals("Email")).FirstOrDefault();
            //if (Email == null)
            //{
            //    DataType dataType = dataTypeManager.Repo.Get(p => p.SystemType.Equals("String")).FirstOrDefault();
            //    Unit unit = unitManager.Repo.Get(p => p.Name.Equals("None")).FirstOrDefault();

            //    Email = mdaManager.Create("Email", "Email", "Email address", false, false, "David Blaa",
            //            MeasurementScale.Categorial, DataContainerType.ValueType, "", dataType, unit, null, null, null, null);
            //}

            //MetadataAttribute Address = mdaManager.MetadataAttributeRepo.Get(p => p.Name.Equals("Address")).FirstOrDefault();
            //if (Address == null)
            //{
            //    DataType dataType = dataTypeManager.Repo.Get(p => p.SystemType.Equals("String")).FirstOrDefault();
            //    Unit unit = unitManager.Repo.Get(p => p.Name.Equals("None")).FirstOrDefault();

            //    Address = mdaManager.Create("Address", "Address", "Address", false, false, "David Blaa",
            //            MeasurementScale.Categorial, DataContainerType.ValueType, "", dataType, unit, null, null, null, null);
            //}

            //MetadataAttribute Phone = mdaManager.MetadataAttributeRepo.Get(p => p.Name.Equals("Phone")).FirstOrDefault();
            //if (Phone == null)
            //{
            //    DataType dataType = dataTypeManager.Repo.Get(p => p.SystemType.Equals("String")).FirstOrDefault();
            //    Unit unit = unitManager.Repo.Get(p => p.Name.Equals("None")).FirstOrDefault();

            //    Phone = mdaManager.Create("Phone", "Phone", "Phone", false, false, "David Blaa",
            //            MeasurementScale.Categorial, DataContainerType.ValueType, "", dataType, unit, null, null, null, null);
            //}


            //if (person.MetadataAttributeUsages != null & person.MetadataAttributeUsages.Count > 0)
            //{
            //    // add metadataAttributes to packages
            //    if (person.MetadataAttributeUsages.Where(p => p.MetadataAttribute == Name).Count() <= 0)
            //        mdpManager.AddMetadataAtributeUsage(person, Name, "Name", "Administrative contact person, Technical contact person, person team, or role for the dataset.", 1, 1);

            //    if (person.MetadataAttributeUsages.Where(p => p.MetadataAttribute == Email).Count() <= 0)
            //        mdpManager.AddMetadataAtributeUsage(person, Email, "Email", "Email address for the administrative, or technical contact for the dataset.", 0, 1);

            //    if (person.MetadataAttributeUsages.Where(p => p.MetadataAttribute == Address).Count() <= 0)
            //        mdpManager.AddMetadataAtributeUsage(person, Address, "Address", "An address for the administrative, or technical contact for the dataset.", 0, 1);

            //    if (person.MetadataAttributeUsages.Where(p => p.MetadataAttribute == Phone).Count() <= 0)
            //        mdpManager.AddMetadataAtributeUsage(person, Phone, "Phone", "Voice phone number for the administrative, or technical contact for the dataset.", 0, 1);
            //}
            //else
            //{
            //    mdpManager.AddMetadataAtributeUsage(person, Name, "Name", "Administrative contact person, Technical contact person, person team, or role for the dataset.", 1, 1);
            //    mdpManager.AddMetadataAtributeUsage(person, Email, "Email", "Email address for the administrative, or technical contact for the dataset.", 0, 1);
            //    mdpManager.AddMetadataAtributeUsage(person, Address, "Address", "An address for the administrative, or technical contact for the dataset.", 0, 1);
            //    mdpManager.AddMetadataAtributeUsage(person, Phone, "Phone", "Voice phone number for the administrative, or technical contact for the dataset.", 0, 1);
            //}

            //#endregion

            //#region metadata

            //MetadataAttribute Title = mdaManager.MetadataAttributeRepo.Get(p => p.Name.Equals("Title")).FirstOrDefault();
            //if (Title == null)
            //{
            //    DataType dataType = dataTypeManager.Repo.Get(p => p.SystemType.Equals("String")).FirstOrDefault();
            //    Unit unit = unitManager.Repo.Get(p => p.Name.Equals("None")).FirstOrDefault();

            //    Title = mdaManager.Create("Title", "Title", "Title", false, false, "David Blaa",
            //            MeasurementScale.Categorial, DataContainerType.ValueType, "", dataType, unit, null, null, null, null);
            //}

            //MetadataAttribute RevisionData = mdaManager.MetadataAttributeRepo.Get(p => p.Name.Equals("RevisionData")).FirstOrDefault();
            //if (RevisionData == null)
            //{
            //    DataType dataType = dataTypeManager.Repo.Get(p => p.SystemType.Equals("DateTime")).FirstOrDefault();
            //    Unit unit = unitManager.Repo.Get(p => p.Name.Equals("None")).FirstOrDefault();

            //    RevisionData = mdaManager.Create("RevisionData", "RevisionData", "RevisionData", false, false, "David Blaa",
            //            MeasurementScale.Categorial, DataContainerType.ValueType, "", dataType, unit, null, null, null, null);
            //}

            //MetadataAttribute Details = mdaManager.MetadataAttributeRepo.Get(p => p.Name.Equals("Details")).FirstOrDefault();
            //if (Details == null)
            //{
            //    DataType dataType = dataTypeManager.Repo.Get(p => p.SystemType.Equals("String") && p.Name.Equals("Text")).FirstOrDefault();
            //    Unit unit = unitManager.Repo.Get(p => p.Name.Equals("None")).FirstOrDefault();

            //    Details = mdaManager.Create("Details", "Details", "Details", false, false, "David Blaa",
            //            MeasurementScale.Categorial, DataContainerType.ValueType, "", dataType, unit, null, null, null, null);
            //}

            //MetadataAttribute Coverage = mdaManager.MetadataAttributeRepo.Get(p => p.Name.Equals("Coverage")).FirstOrDefault();
            //if (Coverage == null)
            //{
            //    DataType dataType = dataTypeManager.Repo.Get(p => p.SystemType.Equals("String")).FirstOrDefault();
            //    Unit unit = unitManager.Repo.Get(p => p.Name.Equals("None")).FirstOrDefault();

            //    Coverage = mdaManager.Create("Coverage", "Coverage", "Coverage", false, false, "David Blaa",
            //            MeasurementScale.Categorial, DataContainerType.ValueType, "", dataType, unit, null, null, null, null);
            //}

            //MetadataAttribute URI = mdaManager.MetadataAttributeRepo.Get(p => p.Name.Equals("URI")).FirstOrDefault();
            //if (URI == null)
            //{
            //    DataType dataType = dataTypeManager.Repo.Get(p => p.SystemType.Equals("String")).FirstOrDefault();
            //    Unit unit = unitManager.Repo.Get(p => p.Name.Equals("None")).FirstOrDefault();

            //    URI = mdaManager.Create("URI", "URI", "URI", false, false, "David Blaa",
            //            MeasurementScale.Categorial, DataContainerType.ValueType, "", dataType, unit, null, null, null, null);
            //}

            //if (Description.MetadataAttributeUsages != null & Description.MetadataAttributeUsages.Count > 0)
            //{
            //    // add metadataAttributes to packages
            //    if (Description.MetadataAttributeUsages.Where(p => p.MetadataAttribute == Title).Count() <= 0)
            //        mdpManager.AddMetadataAtributeUsage(Description, Title, "Title", "A short concise title for the dataset.", 1, 1);

            //    if (Description.MetadataAttributeUsages.Where(p => p.MetadataAttribute == RevisionData).Count() <= 0)
            //        mdpManager.AddMetadataAtributeUsage(Description, RevisionData, "DateModified", "The last modification date for the data source.", 0, 1);

            //    if (Description.MetadataAttributeUsages.Where(p => p.MetadataAttribute == Details).Count() <= 0)
            //        mdpManager.AddMetadataAtributeUsage(Description, Details, "Details", "Free-form text containing a longer description of the data source.", 0, 1);

            //    if (Description.MetadataAttributeUsages.Where(p => p.MetadataAttribute == Coverage).Count() <= 0)
            //        mdpManager.AddMetadataAtributeUsage(Description, Coverage, "Coverage", "Free-form text terminology or descriptions available in the data source (geographic, taxonomic, etc.).", 0, 1);

            //    if (Description.MetadataAttributeUsages.Where(p => p.MetadataAttribute == URI).Count() <= 0)
            //        mdpManager.AddMetadataAtributeUsage(Description, URI, "URI", "URL that points to an online source related to the data source, which may or may not serve as an updated version of the description data.", 0, 1);
            //}
            //else
            //{
            //    mdpManager.AddMetadataAtributeUsage(Description, Title, "Title", "A short concise title for the dataset.", 1, 1);
            //    mdpManager.AddMetadataAtributeUsage(Description, RevisionData, "DateModified", "The last modification date for the data source.", 0, 1);
            //    mdpManager.AddMetadataAtributeUsage(Description, Details, "Details", "Free-form text containing a longer description of the data source.", 0, 1);
            //    mdpManager.AddMetadataAtributeUsage(Description, Coverage, "Coverage", "Free-form text terminology or descriptions available in the data source (geographic, taxonomic, etc.).", 0, 1);
            //    mdpManager.AddMetadataAtributeUsage(Description, URI, "URI", "URL that points to an online source related to the data source, which may or may not serve as an updated version of the description data.", 0, 1);
            //}



            //#endregion

            //#region Owner package

            //MetadataAttribute Role = mdaManager.MetadataAttributeRepo.Get(p => p.Name.Equals("Role")).FirstOrDefault();
            //if (Role == null)
            //{
            //    DataType dataType = dataTypeManager.Repo.Get(p => p.SystemType.Equals("String")).FirstOrDefault();
            //    Unit unit = unitManager.Repo.Get(p => p.Name.Equals("None")).FirstOrDefault();

            //    Role = mdaManager.Create("Role", "Role", "Role", false, false, "David Blaa",
            //            MeasurementScale.Categorial, DataContainerType.ValueType, "", dataType, unit, null, null, null, null);
            //}

            //if (Owner.MetadataAttributeUsages != null & Owner.MetadataAttributeUsages.Count > 0)
            //{
            //    // add metadataAttributes to packages
            //    if (Owner.MetadataAttributeUsages.Where(p => p.MetadataAttribute == Name).Count() <= 0)
            //        mdpManager.AddMetadataAtributeUsage(Owner, Name, "Full Name", "String of the preferred form of personal name for display representing the data collection's legal owner.", 1, 1);

            //    if (Owner.MetadataAttributeUsages.Where(p => p.MetadataAttribute == Name).Count() <= 0)
            //        mdpManager.AddMetadataAtributeUsage(Owner, Name, "Sorting Name", "The full name of the data collection owner in a form appropriate for sorting alphabetically.", 0, 1);

            //    if (Owner.MetadataAttributeUsages.Where(p => p.MetadataAttribute == Name).Count() <= 0)
            //        mdpManager.AddMetadataAtributeUsage(Owner, Name, "Organisation Name", "Container element for several language-specific representations of the full organisation or corporate name for the data source owner.", 0, 1);

            //    if (Owner.MetadataAttributeUsages.Where(p => p.MetadataAttribute == Role).Count() <= 0)
            //        mdpManager.AddMetadataAtributeUsage(Owner, Role, "Role", "Title for the role of the person or organisation owner of the data collection.", 0, 1);

            //    if (Owner.MetadataAttributeUsages.Where(p => p.MetadataAttribute == Address).Count() <= 0)
            //        mdpManager.AddMetadataAtributeUsage(Owner, Address, "Address", "A string representing the address of the data collection owner.", 0, 1);

            //    if (Owner.MetadataAttributeUsages.Where(p => p.MetadataAttribute == Email).Count() <= 0)
            //        mdpManager.AddMetadataAtributeUsage(Owner, Email, "Email", "A valid contact e-mail address for the owner of the data collection.", 0, 1);

            //    if (Owner.MetadataAttributeUsages.Where(p => p.MetadataAttribute == Phone).Count() <= 0)
            //        mdpManager.AddMetadataAtributeUsage(Owner, Phone, "Phone", "Telephone number for the legal owner of the data collection.", 0, 1);

            //    if (Owner.MetadataAttributeUsages.Where(p => p.MetadataAttribute == URI).Count() <= 0)
            //        mdpManager.AddMetadataAtributeUsage(Owner, URI, "URI", "Publicly available URL for the person or organisation representing the legal owner of the data collection.", 0, 1);

            //}
            //else
            //{
            //    mdpManager.AddMetadataAtributeUsage(Owner, Name, "Full Name", "String of the preferred form of personal name for display representing the data collection's legal owner.", 1, 1);
            //    mdpManager.AddMetadataAtributeUsage(Owner, Name, "Sorting Name", "The full name of the data collection owner in a form appropriate for sorting alphabetically.", 0, 1);
            //    mdpManager.AddMetadataAtributeUsage(Owner, Name, "Organisation Name", "Container element for several language-specific representations of the full organisation or corporate name for the data source owner.", 0, 1);
            //    mdpManager.AddMetadataAtributeUsage(Owner, Address, "Address", "A string representing the address of the data collection owner.", 0, 1);
            //    mdpManager.AddMetadataAtributeUsage(Owner, Email, "Email", "A valid contact e-mail address for the owner of the data collection.", 0, 1);
            //    mdpManager.AddMetadataAtributeUsage(Owner, Role, "Role", "Title for the role of the person or organisation owner of the data collection.", 0, 1);
            //    mdpManager.AddMetadataAtributeUsage(Owner, Phone, "Phone", "Telephone number for the legal owner of the data collection.", 0, 1);
            //    mdpManager.AddMetadataAtributeUsage(Owner, URI, "URI", "Publicly available URL for the person or organisation representing the legal owner of the data collection.", 0, 1);

            //}

            //#endregion

            //#region Scope package

            //MetadataAttribute TaxonomicTerm = mdaManager.MetadataAttributeRepo.Get(p => p.Name.Equals("TaxonomicTerm")).FirstOrDefault();
            //if (TaxonomicTerm == null)
            //{
            //    DataType dataType = dataTypeManager.Repo.Get(p => p.SystemType.Equals("String")).FirstOrDefault();
            //    Unit unit = unitManager.Repo.Get(p => p.Name.Equals("None")).FirstOrDefault();

            //    TaxonomicTerm = mdaManager.Create("TaxonomicTerm", "TaxonomicTerm", "TaxonomicTerm", false, false, "David Blaa",
            //            MeasurementScale.Categorial, DataContainerType.ValueType, "", dataType, unit, null, null, null, null);
            //}

            //MetadataAttribute GeoEcologicalTerm = mdaManager.MetadataAttributeRepo.Get(p => p.Name.Equals("GeoEcologicalTerm")).FirstOrDefault();
            //if (GeoEcologicalTerm == null)
            //{
            //    DataType dataType = dataTypeManager.Repo.Get(p => p.SystemType.Equals("String")).FirstOrDefault();
            //    Unit unit = unitManager.Repo.Get(p => p.Name.Equals("None")).FirstOrDefault();

            //    GeoEcologicalTerm = mdaManager.Create("GeoEcologicalTerm", "GeoEcologicalTerm", "GeoEcologicalTerm", false, false, "David Blaa",
            //            MeasurementScale.Categorial, DataContainerType.ValueType, "", dataType, unit, null, null, null, null);
            //}

            //if (Scope.MetadataAttributeUsages != null & Scope.MetadataAttributeUsages.Count > 0)
            //{
            //    // add metadataAttributes to packages
            //    if (Scope.MetadataAttributeUsages.Where(p => p.MetadataAttribute == TaxonomicTerm).Count() <= 0)
            //        mdpManager.AddMetadataAtributeUsage(Scope, TaxonomicTerm, "TaxonomicTerm", "A container for taxonomic terms describing the data source.", 0, 10);

            //    if (Scope.MetadataAttributeUsages.Where(p => p.MetadataAttribute == GeoEcologicalTerm).Count() <= 0)
            //        mdpManager.AddMetadataAtributeUsage(Scope, GeoEcologicalTerm, "GeoEcologicalTerm", "A container for geoecological terms describing the data source.", 0, 10);

            //}
            //else
            //{
            //    mdpManager.AddMetadataAtributeUsage(Scope, TaxonomicTerm, "TaxonomicTerm", "A container for taxonomic terms describing the data source.", 0, 10);
            //    mdpManager.AddMetadataAtributeUsage(Scope, GeoEcologicalTerm, "GeoEcologicalTerm", "A container for geoecological terms describing the data source.", 0, 10);
            //}


            //#endregion

            //#region Unit (ABCD Part)

            //MetadataPackage Unit = mdpManager.MetadataPackageRepo.Get(p => p.Name == "Unit").FirstOrDefault();
            //if (Unit == null) Unit = mdpManager.Create("Unit", "A container for all data referring to a unit (specimen or observation record).", true);

            //if (abcd.MetadataPackageUsages != null && abcd.MetadataPackageUsages.Count > 0)
            //{
            //    if (abcd.MetadataPackageUsages.Where(p => p.MetadataPackage == Unit).Count() <= 0)
            //    {
            //        mdsManager.AddMetadataPackageUsage(abcd, Unit, "Unit", "A container for one or more unit data records from the gathering project.", 1, 5);
            //    }

            //}
            //else
            //{

            //    mdsManager.AddMetadataPackageUsage(abcd, Unit, "Unit", "A container for one or more unit data records from the gathering project.", 1, 5);
            //}

            //// metadata attributes for Unit
            //MetadataAttribute Id = mdaManager.MetadataAttributeRepo.Get(p => p.Name.Equals("Id")).FirstOrDefault();
            //if (Id == null)
            //{
            //    DataType dataType = dataTypeManager.Repo.Get(p => p.SystemType.Equals("String")).FirstOrDefault();
            //    Unit unit = unitManager.Repo.Get(p => p.Name.Equals("None")).FirstOrDefault();

            //    Id = mdaManager.Create("Id", "Id", "Name or code of the data source", false, false, "David Blaa",
            //            MeasurementScale.Categorial, DataContainerType.ValueType, "", dataType, unit, null, null, null, null);
            //}

            //// MetadataAttribute Usage
            //if (Unit.MetadataAttributeUsages != null & Unit.MetadataAttributeUsages.Count > 0)
            //{
            //    // add metadataAttributes to packages
            //    if (Unit.MetadataAttributeUsages.Where(p => p.MetadataAttribute == Id).Count() <= 0)
            //    {

            //        MetadataAttributeUsage mau = mdpManager.AddMetadataAtributeUsage(Unit, Id, "SourceInstitutionID","The unique identifier (code or name) of the institution holding the original data source.", 1, 1);
            //    }

            //    if (Unit.MetadataAttributeUsages.Where(p => p.MetadataAttribute == Id).Count() <= 0)
            //        mdpManager.AddMetadataAtributeUsage(Unit, Id, "SourceID", "The name or code of the data source.", 1, 1);

            //    if (Unit.MetadataAttributeUsages.Where(p => p.MetadataAttribute == Id).Count() <= 0)
            //        mdpManager.AddMetadataAtributeUsage(Unit, Id, "UnitID", "A unique identifier for the unit record within the data source.", 1, 1);

            //}
            //else
            //{
            //    mdpManager.AddMetadataAtributeUsage(Unit, Id, "SourceInstitutionID", "The unique identifier (code or name) of the institution holding the original data source.", 1, 1);
            //    mdpManager.AddMetadataAtributeUsage(Unit, Id, "SourceID", "The name or code of the data source.", 1, 1);
            //    mdpManager.AddMetadataAtributeUsage(Unit, Id, "UnitID", "A unique identifier for the unit record within the data source.", 1, 1);
            //}


            //#endregion

            //#endregion

            return View("Index");
        }

        public ActionResult CreateEMLMetadataStructure()
        {

            #region eml

            //CreateEmlDatasetAdv();

            #endregion

            return View("Index");
        }


        #endregion

        //private void CreateEmlBasic()
        //{
        //    MetadataStructureManager mdsManager = new MetadataStructureManager();
        //    MetadataPackageManager mdpManager = new MetadataPackageManager();
        //    MetadataAttributeManager mdaManager = new MetadataAttributeManager();

        //    DataTypeManager dataTypeManager = new DataTypeManager();
        //    UnitManager unitManager = new UnitManager();

        //    MetadataStructure abcd = mdsManager.Repo.Get(p => p.Name == "ABCD").FirstOrDefault();
        //    if (abcd == null) abcd = mdsManager.Create("ABCD", "This is the ABCD structure", "", "", null);

        //    MetadataStructure eml = mdsManager.Repo.Get(p => p.Name == "EML").FirstOrDefault();

        //    if (eml == null) eml = mdsManager.Create("EML", "This is the EML structure", "", "", null);

        //    XmlDocument xmlDoc = new XmlDocument();

        //    if (eml.Extra != null)
        //    {
        //        xmlDoc=(XmlDocument)eml.Extra;
        //    }

        //    // add title Node
        //    xmlDoc = AddReferenceToMetadatStructure(eml, "title", "Metadata/Description/DescriptionEML/Title/Title", "extra/nodeReferences/nodeRef", xmlDoc);

        //    // add ConvertReference Mapping file node
        //    xmlDoc = AddReferenceToMetadatStructure(eml, "mappingFile", "mapping_eml.xml", "extra/convertReferences/convertRef", xmlDoc);

        //    eml.Extra = xmlDoc;
        //    mdsManager.Update(eml);

        //    //package Description for title
        //    MetadataPackage DescEml = mdpManager.MetadataPackageRepo.Get(p => p.Name == "DescriptionEML").FirstOrDefault();
        //    if (DescEml == null) DescEml = mdpManager.Create("DescriptionEML", "DescriptionEML", true);

        //    //package PersonEML ( Creator / Contact)
        //    MetadataPackage personEml = mdpManager.MetadataPackageRepo.Get(p => p.Name == "PersonEML").FirstOrDefault();
        //    if (personEml == null) personEml = mdpManager.Create("PersonEML", "PersonEML", true);

        //    //package PersonEML ( Creator / Contact)
        //    MetadataPackage projectEml = mdpManager.MetadataPackageRepo.Get(p => p.Name == "ProjectEML").FirstOrDefault();
        //    if (projectEml == null) projectEml = mdpManager.Create("ProjectEML", "PersonEML", true);

        //    // add package to structure
        //    if (eml.MetadataPackageUsages != null && eml.MetadataPackageUsages.Count > 0)
        //    {
        //        if (eml.MetadataPackageUsages.Where(p => p.MetadataPackage == DescEml).Count() <= 0)
        //            mdsManager.AddMetadataPackageUsage(eml, DescEml, "Description", "A text description of the maintenance of this data resource.", 1, 1);

        //        if (eml.MetadataPackageUsages.Where(p => p.MetadataPackage == personEml).Count() <= 0)
        //            mdsManager.AddMetadataPackageUsage(eml, personEml, "Creator", "The 'creator' element provides the full name of the person, organization, or position who created the resource.", 1, 5);

        //        if (eml.MetadataPackageUsages.Where(p => p.MetadataPackage == personEml).Count() <= 0)
        //            mdsManager.AddMetadataPackageUsage(eml, personEml, "Contact", "The contact contains contact information for this dataset. This is the person or institution to contact with questions about the use, interpretation of a data set.", 1, 5);

        //        if (eml.MetadataPackageUsages.Where(p => p.MetadataPackage == projectEml).Count() <= 0)
        //            mdsManager.AddMetadataPackageUsage(eml, projectEml, "Project", "The project contains information on the project in which this dataset was collected.", 1, 1);
        //    }
        //    else
        //    {
        //        mdsManager.AddMetadataPackageUsage(eml, DescEml, "Description", "A text description of the maintenance of this data resource.", 1, 1);
        //        mdsManager.AddMetadataPackageUsage(eml, personEml, "Creator", "The 'creator' element provides the full name of the person, organization, or position who created the resource.", 1, 5);
        //        mdsManager.AddMetadataPackageUsage(eml, personEml, "Contact", "The contact contains contact information for this dataset. This is the person or institution to contact with questions about the use, interpretation of a data set.", 1, 5);
        //        mdsManager.AddMetadataPackageUsage(eml, projectEml, "Project", "The project contains information on the project in which this dataset was collected.", 1, 1);
        //    }

        //    #region Description EML

        //    MetadataAttribute Title = mdaManager.MetadataAttributeRepo.Get(p => p.Name.Equals("Title")).FirstOrDefault();
        //    if (Title == null)
        //    {
        //        DataType dataType = dataTypeManager.Repo.Get(p => p.SystemType.Equals("String")).FirstOrDefault();
        //        Unit unit = unitManager.Repo.Get(p => p.Name.Equals("None")).FirstOrDefault();

        //        Title = mdaManager.Create("Title", "Title", "Title", false, false, "David Blaa",
        //                MeasurementScale.Categorial, DataContainerType.ValueType, "", dataType, unit, null, null, null, null);
        //    }

        //    if (DescEml.MetadataAttributeUsages != null & DescEml.MetadataAttributeUsages.Count > 0)
        //    {
        //        // add metadataAttributes to packages
        //        if (DescEml.MetadataAttributeUsages.Where(p => p.MetadataAttribute == Title).Count() <= 0)
        //            mdpManager.AddMetadataAtributeUsage(DescEml, Title, "Title", "A descriptive title for the research project.", 1, 1);

        //    }
        //    else
        //    {
        //        mdpManager.AddMetadataAtributeUsage(DescEml, Title, "Title", "A descriptive title for the research project.", 1, 1);
        //    }

        //    #endregion

        //    #region Peronal EML

        //    MetadataAttribute Name = mdaManager.MetadataAttributeRepo.Get(p => p.Name.Equals("Name")).FirstOrDefault();
        //    if (Name == null)
        //    {
        //        DataType dataType = dataTypeManager.Repo.Get(p => p.SystemType.Equals("String")).FirstOrDefault();
        //        Unit unit = unitManager.Repo.Get(p => p.Name.Equals("None")).FirstOrDefault();

        //        Name = mdaManager.Create("Name", "Name", "first and last name", false, false, "David Blaa",
        //                MeasurementScale.Categorial, DataContainerType.ValueType, "", dataType, unit, null, null, null, null);
        //    }

        //    if (Name == null)
        //    {
        //        DataType dataType = dataTypeManager.Repo.Get(p => p.SystemType.Equals("String")).FirstOrDefault();
        //        Unit unit = unitManager.Repo.Get(p => p.Name.Equals("None")).FirstOrDefault();

        //        Name = mdaManager.Create("Name", "Name", "first and last name", false, false, "David Blaa",
        //                MeasurementScale.Categorial, DataContainerType.ValueType, "", dataType, unit, null, null, null, null);
        //    }

        //    if (personEml.MetadataAttributeUsages != null & personEml.MetadataAttributeUsages.Count > 0)
        //    {
        //        // add metadataAttributes to packages
        //        if (personEml.MetadataAttributeUsages.Where(p => p.MetadataAttribute == Name).Count() <= 0)
        //            mdpManager.AddMetadataAtributeUsage(personEml, Name, "Given name", "The given name is used for first name of the individual associated with the resource, or for any other names that are not intended to be alphabetized.", 1, 1);

        //        if (personEml.MetadataAttributeUsages.Where(p => p.MetadataAttribute == Name).Count() <= 0)
        //            mdpManager.AddMetadataAtributeUsage(personEml, Name, "Sur name", "The surname is used for the last name of the individual associated with the resource. This is typically the family name of an individual.", 1, 1);

        //        if (personEml.MetadataAttributeUsages.Where(p => p.MetadataAttribute == Name).Count() <= 0)
        //            mdpManager.AddMetadataAtributeUsage(personEml, Name, "Organization", 1, 1);

        //    }
        //    else
        //    {
        //        mdpManager.AddMetadataAtributeUsage(personEml, Name, "Given name", "The given name can be used for first name of the individual associated with the resource, or for any other names that are not intended to be alphabetized, (as appropriate).", 1, 1);
        //        mdpManager.AddMetadataAtributeUsage(personEml, Name, "Sur name", "The surname is used for the last name of the individual associated with the resource. This is typically the family name of an individual.", 1, 1);
        //        mdpManager.AddMetadataAtributeUsage(personEml, Name, "Organization", 1, 1);
        //    }

        //    #endregion

        //    #region Project Eml

        //    MetadataAttribute DescriptionAttr = mdaManager.MetadataAttributeRepo.Get(p => p.Name.Equals("Description")).FirstOrDefault();
        //    if (DescriptionAttr == null)
        //    {
        //        DataType dataType = dataTypeManager.Repo.Get(p => p.SystemType.Equals("String")).FirstOrDefault();
        //        Unit unit = unitManager.Repo.Get(p => p.Name.Equals("None")).FirstOrDefault();

        //        DescriptionAttr = mdaManager.Create("Description", "Description", "Description", false, false, "David Blaa",
        //                MeasurementScale.Categorial, DataContainerType.ValueType, "", dataType, unit, null, null, null, null);
        //    }

        //    MetadataAttribute Role = mdaManager.MetadataAttributeRepo.Get(p => p.Name.Equals("Role")).FirstOrDefault();
        //    if (Role == null)
        //    {
        //        DataType dataType = dataTypeManager.Repo.Get(p => p.SystemType.Equals("String")).FirstOrDefault();
        //        Unit unit = unitManager.Repo.Get(p => p.Name.Equals("None")).FirstOrDefault();

        //        Role = mdaManager.Create("Role", "Role", "Role", false, false, "David Blaa",
        //                MeasurementScale.Categorial, DataContainerType.ValueType, "", dataType, unit, null, null, null, null);
        //    }

        //    if (projectEml.MetadataAttributeUsages != null & projectEml.MetadataAttributeUsages.Count > 0)
        //    {
        //        if (Title == null)
        //        {
        //            DataType dataType = dataTypeManager.Repo.Get(p => p.SystemType.Equals("String")).FirstOrDefault();
        //            Unit unit = unitManager.Repo.Get(p => p.Name.Equals("None")).FirstOrDefault();

        //            Title = mdaManager.Create("Title", "Title", "Title", false, false, "David Blaa",
        //                    MeasurementScale.Categorial, DataContainerType.ValueType, "", dataType, unit, null, null, null, null);
        //        }

        //        // add metadataAttributes to packages
        //        if (projectEml.MetadataAttributeUsages.Where(p => p.MetadataAttribute == Title).Count() <= 0)
        //            mdpManager.AddMetadataAtributeUsage(projectEml, Title, "Title", "A descriptive title for the research project.", 0, 1);

        //        if (projectEml.MetadataAttributeUsages.Where(p => p.MetadataAttribute == Name).Count() <= 0)
        //            mdpManager.AddMetadataAtributeUsage(projectEml, Name, "Personnel given name", "The given name is used for the first name of the individual associated with the resource.", 0, 1);

        //        if (projectEml.MetadataAttributeUsages.Where(p => p.MetadataAttribute == Name).Count() <= 0)
        //            mdpManager.AddMetadataAtributeUsage(projectEml, Name, "Personnel sur name", "The sur name is used for the last name of the individual associated with the resource.", 0, 1);

        //        if (projectEml.MetadataAttributeUsages.Where(p => p.MetadataAttribute == Role).Count() <= 0)
        //            mdpManager.AddMetadataAtributeUsage(projectEml, Role, "Role", "The role contains information about role a person plays in a research project.", 0, 1);

        //        if (projectEml.MetadataAttributeUsages.Where(p => p.MetadataAttribute == DescriptionAttr).Count() <= 0)
        //            mdpManager.AddMetadataAtributeUsage(projectEml, DescriptionAttr, "Project description", "The project description de contains general textual descriptions of research design.", 0, 1);

        //    }
        //    else
        //    {
        //        mdpManager.AddMetadataAtributeUsage(projectEml, Title, "Title", "A descriptive title for the research project.", 0, 1);
        //        mdpManager.AddMetadataAtributeUsage(projectEml, Name, "Personnel given name", "The given name is used for the first name of the individual associated with the resource.", 0, 1);
        //        mdpManager.AddMetadataAtributeUsage(projectEml, Name, "Personnel sur name", "The sur name is used for the last name of the individual associated with the resource.", 0, 1);
        //        mdpManager.AddMetadataAtributeUsage(projectEml, Role, "Role", "The role contains information about role a person plays in a research project.", 0, 1);
        //        mdpManager.AddMetadataAtributeUsage(projectEml, DescriptionAttr, "Project description", "The project description de contains general textual descriptions of research design.", 0, 1);
        //    }

        //    #endregion
        //}

        //private void CreateEmlDatasetAdv()
        //{
        //    MetadataStructureManager mdsManager = new MetadataStructureManager();
        //    MetadataPackageManager mdpManager = new MetadataPackageManager();
        //    MetadataAttributeManager mdaManager = new MetadataAttributeManager();

        //    DataTypeManager dataTypeManager = new DataTypeManager();
        //    UnitManager unitManager = new UnitManager();

        //    MetadataStructure eml = mdsManager.Repo.Get(p => p.Name == "EML Dataset").FirstOrDefault();

        //    if (eml == null) eml = mdsManager.Create("EML Dataset", "This is the EML structure", "", "", null);

        //    XmlDocument xmlDoc = new XmlDocument();

        //    if (eml.Extra != null)
        //    {
        //        xmlDoc = (XmlDocument)eml.Extra;
        //    }

        //    // add title Node
        //    xmlDoc = AddReferenceToMetadatStructure(eml, "title", "Metadata/Description/DescriptionEML/Title/Title", "extra/nodeReferences/nodeRef", xmlDoc);

        //    // add ConvertReference Mapping file node
        //    xmlDoc = AddReferenceToMetadatStructure(eml, "mappingFile", "mapping_eml.xml", "extra/convertReferences/convertRef", xmlDoc);

        //    eml.Extra = xmlDoc;
        //    mdsManager.Update(eml);

        //    #region create packages

        //    //package Description for title
        //    MetadataPackage DescEml = mdpManager.MetadataPackageRepo.Get(p => p.Name == "DescriptionEML").FirstOrDefault();
        //    if (DescEml == null) DescEml = mdpManager.Create("DescriptionEML", "DescriptionEML", true);

        //    //package PersonEML ( Creator / Contact)
        //    MetadataPackage personEml = mdpManager.MetadataPackageRepo.Get(p => p.Name == "PersonEML").FirstOrDefault();
        //    if (personEml == null) personEml = mdpManager.Create("PersonEML", "PersonEML", true);

        //    //package PersonEML ( Creator / Contact)
        //    MetadataPackage party = mdpManager.MetadataPackageRepo.Get(p => p.Name == "Party").FirstOrDefault();
        //    if (party == null) party = mdpManager.Create("Party", "Person or Organization", true);

        //    //package PersonEML ( Creator / Contact)
        //    MetadataPackage projectEml = mdpManager.MetadataPackageRepo.Get(p => p.Name == "ProjectEML").FirstOrDefault();
        //    if (projectEml == null) projectEml = mdpManager.Create("ProjectEML", "ProjectEML", true);

        //    //package PersonEML ( Creator / Contact)
        //    MetadataPackage coverage = mdpManager.MetadataPackageRepo.Get(p => p.Name == "Coverage").FirstOrDefault();
        //    if (coverage == null) coverage = mdpManager.Create("Coverage", "Coverage", true);

        //    #endregion

        //    #region add packages


        //    // add package to structure
        //    if (eml.MetadataPackageUsages != null && eml.MetadataPackageUsages.Count > 0)
        //    {
        //        if (eml.MetadataPackageUsages.Where(p => p.MetadataPackage == DescEml).Count() <= 0)
        //            mdsManager.AddMetadataPackageUsage(eml, DescEml, "Description", "A text description of the maintenance of this data resource.", 1, 1);

        //        if (eml.MetadataPackageUsages.Where(p => p.MetadataPackage == personEml).Count() <= 0)
        //            mdsManager.AddMetadataPackageUsage(eml, personEml, "Creator", "The 'creator' element provides the full name of the person, organization, or position who created the resource.", 1, 5);

        //        if (eml.MetadataPackageUsages.Where(p => p.MetadataPackage == party).Count() <= 0)
        //            mdsManager.AddMetadataPackageUsage(eml, party, "Associated Party ", "The responsible party is used to describe a person, organization, or position within an organization.", 1, 10);

        //        if (eml.MetadataPackageUsages.Where(p => p.MetadataPackage == personEml).Count() <= 0)
        //            mdsManager.AddMetadataPackageUsage(eml, personEml, "Contact", "The contact contains contact information for this dataset. This is the person or institution to contact with questions about the use, interpretation of a data set.", 1, 5);

        //        if (eml.MetadataPackageUsages.Where(p => p.MetadataPackage == projectEml).Count() <= 0)
        //            mdsManager.AddMetadataPackageUsage(eml, projectEml, "Project", "The project contains information on the project in which this dataset was collected.", 1, 1);

        //        //if (eml.MetadataPackageUsages.Where(p => p.MetadataPackage == coverage).Count() <= 0)
        //        //    mdsManager.AddMetadataPackageUsage(eml, coverage, "Coverage", 1, 1);
        //    }
        //    else
        //    {
        //        mdsManager.AddMetadataPackageUsage(eml, DescEml, "Description", "A text description of the maintenance of this data resource.", 1, 1);
        //        mdsManager.AddMetadataPackageUsage(eml, personEml, "Creator", "The 'creator' element provides the full name of the person, organization, or position who created the resource.", 1, 5);
        //        mdsManager.AddMetadataPackageUsage(eml, party, "Associated Parties ", "The responsible party is used to describe a person, organization, or position within an organization.", 1, 10);
        //        mdsManager.AddMetadataPackageUsage(eml, personEml, "Contact", "The contact contains contact information for this dataset. This is the person or institution to contact with questions about the use, interpretation of a data set.", 1, 5);
        //        mdsManager.AddMetadataPackageUsage(eml, projectEml, "Project", "The project contains information on the project in which this dataset was collected.", 1, 1);
        //        //mdsManager.AddMetadataPackageUsage(eml, coverage, "Coverage", 1, 1);
        //    }

        //    #endregion

        //    #region Description EML

        //    #region create attr

        //    MetadataAttribute Name = mdaManager.MetadataAttributeRepo.Get(p => p.Name.Equals("Name")).FirstOrDefault();
        //    if (Name == null)
        //    {
        //        DataType dataType = dataTypeManager.Repo.Get(p => p.SystemType.Equals("String")).FirstOrDefault();
        //        Unit unit = unitManager.Repo.Get(p => p.Name.Equals("None")).FirstOrDefault();

        //        Name = mdaManager.Create("Name", "Name", "first and last name", false, false, "David Blaa",
        //                MeasurementScale.Categorial, DataContainerType.ValueType, "", dataType, unit, null, null, null, null);
        //    }

        //    MetadataAttribute Title = mdaManager.MetadataAttributeRepo.Get(p => p.Name.Equals("Title")).FirstOrDefault();
        //    if (Title == null)
        //    {
        //        DataType dataType = dataTypeManager.Repo.Get(p => p.SystemType.Equals("String")).FirstOrDefault();
        //        Unit unit = unitManager.Repo.Get(p => p.Name.Equals("None")).FirstOrDefault();

        //        Title = mdaManager.Create("Title", "Title", "Title", false, false, "David Blaa",
        //                MeasurementScale.Categorial, DataContainerType.ValueType, "", dataType, unit, null, null, null, null);
        //    }

        //    MetadataAttribute Date = mdaManager.MetadataAttributeRepo.Get(p => p.Name.Equals("Date")).FirstOrDefault();
        //    if (Date == null)
        //    {
        //        DataType dataType = dataTypeManager.Repo.Get(p => p.SystemType.Equals("DateTime")).FirstOrDefault();
        //        Unit unit = unitManager.Repo.Get(p => p.Name.Equals("None")).FirstOrDefault();

        //        Date = mdaManager.Create("Date", "Date", "Date", false, false, "David Blaa",
        //                MeasurementScale.Categorial, DataContainerType.ValueType, "", dataType, unit, null, null, null, null);
        //    }

        //    MetadataAttribute Info = mdaManager.MetadataAttributeRepo.Get(p => p.Name.Equals("Information")).FirstOrDefault();
        //    if (Info == null)
        //    {
        //        DataType dataType = dataTypeManager.Repo.Get(p => p.SystemType.Equals("String") && p.Name.Equals("Text")).FirstOrDefault();
        //        Unit unit = unitManager.Repo.Get(p => p.Name.Equals("None")).FirstOrDefault();

        //        Info = mdaManager.Create("Information", "Information", "Information", false, false, "David Blaa",
        //                MeasurementScale.Categorial, DataContainerType.ValueType, "", dataType, unit, null, null, null, null);
        //    }


        //    #endregion

        //    #region add attr

        //    if (DescEml.MetadataAttributeUsages != null & DescEml.MetadataAttributeUsages.Count > 0)
        //    {
        //        // add metadataAttributes to packages
        //        if (DescEml.MetadataAttributeUsages.Where(p => p.MetadataAttribute == Name).Count() <= 0)
        //        {
        //            mdpManager.AddMetadataAtributeUsage(DescEml, Name, "Short Name", "The 'shortName' provides a concise name that describes the resource that is being documented.", 0, 1);
        //        }

        //        if (DescEml.MetadataAttributeUsages.Where(p => p.MetadataAttribute == Title).Count() <= 0)
        //            mdpManager.AddMetadataAtributeUsage(DescEml, Title, "Title", "The 'title' provides a description of the resource that is being documented that is long enough to differentiate it from other similar resources.", 1, 1);

        //        if (DescEml.MetadataAttributeUsages.Where(p => p.MetadataAttribute == Date).Count() <= 0)
        //            mdpManager.AddMetadataAtributeUsage(DescEml, Date, "Publish Date", "The 'Publish Date' represents the date that the resource was published.", 0, 1);

        //        if (DescEml.MetadataAttributeUsages.Where(p => p.MetadataAttribute == Info).Count() <= 0)
        //            mdpManager.AddMetadataAtributeUsage(DescEml, Info, "Additional Information", "This field provides any information that is not characterized by the other resource metadata fields.", 0, 1);


        //    }
        //    else
        //    {
        //        mdpManager.AddMetadataAtributeUsage(DescEml, Name, "Short Name", "The 'shortName' provides a concise name that describes the resource that is being documented.", 0, 1);
        //        mdpManager.AddMetadataAtributeUsage(DescEml, Title, "Title", "The 'title' provides a description of the resource that is being documented that is long enough to differentiate it from other similar resources.", 1, 1);
        //        mdpManager.AddMetadataAtributeUsage(DescEml, Date, "Publish Date", "The 'Publish Date' represents the date that the resource was published.", 0, 1);
        //        mdpManager.AddMetadataAtributeUsage(DescEml, Info, "Additional Information", "This field provides any information that is not characterized by the other resource metadata fields.", 0, 1);
        //    }


        //    #endregion

        //    #endregion

        //    #region Peronal EML

        //    #region create attr

        //    Name = mdaManager.MetadataAttributeRepo.Get(p => p.Name.Equals("Name")).FirstOrDefault();
        //    if (Name == null)
        //    {
        //        DataType dataType = dataTypeManager.Repo.Get(p => p.SystemType.Equals("String")).FirstOrDefault();
        //        Unit unit = unitManager.Repo.Get(p => p.Name.Equals("None")).FirstOrDefault();

        //        Name = mdaManager.Create("Name", "Name", "first and last name", false, false, "David Blaa",
        //                MeasurementScale.Categorial, DataContainerType.ValueType, "", dataType, unit, null, null, null, null);
        //    }

        //    #endregion

        //    #region add attr

        //    if (Name == null)
        //    {
        //        DataType dataType = dataTypeManager.Repo.Get(p => p.SystemType.Equals("String")).FirstOrDefault();
        //        Unit unit = unitManager.Repo.Get(p => p.Name.Equals("None")).FirstOrDefault();

        //        Name = mdaManager.Create("Name", "Name", "first and last name", false, false, "David Blaa",
        //                MeasurementScale.Categorial, DataContainerType.ValueType, "", dataType, unit, null, null, null, null);
        //    }

        //    if (personEml.MetadataAttributeUsages != null & personEml.MetadataAttributeUsages.Count > 0)
        //    {
        //        // add metadataAttributes to packages
        //        if (personEml.MetadataAttributeUsages.Where(p => p.MetadataAttribute == Name).Count() <= 0)
        //            mdpManager.AddMetadataAtributeUsage(personEml, Name, "Given name", "The given name is used for the first name of the individual associated with the resource.", 1, 1);

        //        if (personEml.MetadataAttributeUsages.Where(p => p.MetadataAttribute == Name).Count() <= 0)
        //            mdpManager.AddMetadataAtributeUsage(personEml, Name, "Sur name", "The Sur name is used for the last name of the individual associated with the resource.", 1, 1);

        //        if (personEml.MetadataAttributeUsages.Where(p => p.MetadataAttribute == Name).Count() <= 0)
        //            mdpManager.AddMetadataAtributeUsage(personEml, Name, "Organization", "This field is intended to describe which institution or overall organization is associated with the resource being described.", 1, 1);

        //    }
        //    else
        //    {
        //        mdpManager.AddMetadataAtributeUsage(personEml, Name, "Given name", "The given name is used for the first name of the individual associated with the resource.", 1, 1);
        //        mdpManager.AddMetadataAtributeUsage(personEml, Name, "Sur name", "The Sur name is used for the last name of the individual associated with the resource.", 1, 1);
        //        mdpManager.AddMetadataAtributeUsage(personEml, Name, "Organization", "This field is intended to describe which institution or overall organization is associated with the resource being described.", 1, 1);
        //    }

        //    #endregion

        //    #endregion

        //    #region Peronal with role EML

        //    #region create attr

        //    Name = mdaManager.MetadataAttributeRepo.Get(p => p.Name.Equals("Name")).FirstOrDefault();
        //    if (Name == null)
        //    {
        //        DataType dataType = dataTypeManager.Repo.Get(p => p.SystemType.Equals("String")).FirstOrDefault();
        //        Unit unit = unitManager.Repo.Get(p => p.Name.Equals("None")).FirstOrDefault();

        //        Name = mdaManager.Create("Name", "Name", "first and last name", false, false, "David Blaa",
        //                MeasurementScale.Categorial, DataContainerType.ValueType, "", dataType, unit, null, null, null, null);
        //    }

        //    MetadataAttribute RoleType = mdaManager.MetadataAttributeRepo.Get(p => p.Name.Equals("RoleType")).FirstOrDefault();
        //    if (RoleType == null)
        //    {
        //        DataType dataType = dataTypeManager.Repo.Get(p => p.SystemType.Equals("String")).FirstOrDefault();
        //        Unit unit = unitManager.Repo.Get(p => p.Name.Equals("None")).FirstOrDefault();

        //        RoleType = mdaManager.Create("Role", "RoleType", "Use this field to describe the role the party played with respect to the resource. Some potential roles include technician, reviewer, principal investigator, and many others.", false, false, "David Blaa",
        //                MeasurementScale.Categorial, DataContainerType.ValueType, "", dataType, unit, null, null, null, null);
        //    }


        //    #endregion

        //    #region add attr

        //    if (party.MetadataAttributeUsages != null & party.MetadataAttributeUsages.Count > 0)
        //    {
        //        // add metadataAttributes to packages
        //        if (party.MetadataAttributeUsages.Where(p => p.MetadataAttribute == Name).Count() <= 0)
        //            mdpManager.AddMetadataAtributeUsage(party, Name, "Given name", "The given name is used for the first name of the individual associated with the resource.", 1, 1);

        //        if (party.MetadataAttributeUsages.Where(p => p.MetadataAttribute == Name).Count() <= 0)
        //            mdpManager.AddMetadataAtributeUsage(party, Name, "Surname", "The Surname is used for the last name of the individual associated with the resource.", 1, 1);

        //        if (party.MetadataAttributeUsages.Where(p => p.MetadataAttribute == Name).Count() <= 0)
        //            mdpManager.AddMetadataAtributeUsage(party, Name, "Organization", "This field is intended to describe which institution or overall organization is associated with the resource being described.", 1, 1);

        //        if (party.MetadataAttributeUsages.Where(p => p.MetadataAttribute == Name).Count() <= 0)
        //            mdpManager.AddMetadataAtributeUsage(party, RoleType, "Role", 1, 1);

        //    }
        //    else
        //    {
        //        mdpManager.AddMetadataAtributeUsage(party, Name, "Given name", "The given name is used for the first name of the individual associated with the resource.", 1, 1);
        //        mdpManager.AddMetadataAtributeUsage(party, Name, "Sur name", "The Sur name is used for the last name of the individual associated with the resource.", 1, 1);
        //        mdpManager.AddMetadataAtributeUsage(party, Name, "Organization", "This field is intended to describe which institution or overall organization is associated with the resource being described.", 1, 1);
        //        mdpManager.AddMetadataAtributeUsage(party, RoleType, "Role", 1, 1);
        //    }

        //    #endregion

        //    #endregion

        //    #region Project Eml

        //    #region create attr

        //    MetadataAttribute DescriptionAttr = mdaManager.MetadataAttributeRepo.Get(p => p.Name.Equals("Description")).FirstOrDefault();
        //    if (DescriptionAttr == null)
        //    {
        //        DataType dataType = dataTypeManager.Repo.Get(p => p.SystemType.Equals("String") && p.Name.Equals("Text")).FirstOrDefault();
        //        Unit unit = unitManager.Repo.Get(p => p.Name.Equals("None")).FirstOrDefault();

        //        DescriptionAttr = mdaManager.Create("Description", "Description", "Description", false, false, "David Blaa",
        //                MeasurementScale.Categorial, DataContainerType.ValueType, "", dataType, unit, null, null, null, null);
        //    }

        //    #endregion

        //    #region add attr
        //    if (projectEml.MetadataAttributeUsages != null & projectEml.MetadataAttributeUsages.Count > 0)
        //    {
        //        if (Title == null)
        //        {
        //            DataType dataType = dataTypeManager.Repo.Get(p => p.SystemType.Equals("String")).FirstOrDefault();
        //            Unit unit = unitManager.Repo.Get(p => p.Name.Equals("None")).FirstOrDefault();

        //            Title = mdaManager.Create("Title", "Title", "Title", false, false, "David Blaa",
        //                    MeasurementScale.Categorial, DataContainerType.ValueType, "", dataType, unit, null, null, null, null);
        //        }

        //        RoleType = mdaManager.MetadataAttributeRepo.Get(p => p.Name.Equals("RoleType")).FirstOrDefault();
        //        if (RoleType == null)
        //        {
        //            DataType dataType = dataTypeManager.Repo.Get(p => p.SystemType.Equals("String")).FirstOrDefault();
        //            Unit unit = unitManager.Repo.Get(p => p.Name.Equals("None")).FirstOrDefault();

        //            RoleType = mdaManager.Create("Role", "RoleType", "Use this field to describe the role the party played with respect to the resource. Some potential roles include technician, reviewer, principal investigator, and many others.", false, false, "David Blaa",
        //                MeasurementScale.Categorial, DataContainerType.ValueType, "", dataType, unit, null, null, null, null);
        //        }

        //        // add metadataAttributes to packages
        //        if (projectEml.MetadataAttributeUsages.Where(p => p.MetadataAttribute == Title).Count() <= 0)
        //            mdpManager.AddMetadataAtributeUsage(projectEml, Title, "Title", 0, 1);

        //        if (projectEml.MetadataAttributeUsages.Where(p => p.MetadataAttribute == Name).Count() <= 0)
        //            mdpManager.AddMetadataAtributeUsage(projectEml, Name, "Personnel given name", "The given name is used for the first name of the individual associated with the resource.", 0, 1);

        //        if (projectEml.MetadataAttributeUsages.Where(p => p.MetadataAttribute == Name).Count() <= 0)
        //            mdpManager.AddMetadataAtributeUsage(projectEml, Name, "Personnel surname", "The Sur name is used for the last name of the individual associated with the resource.", 0, 1);

        //        if (projectEml.MetadataAttributeUsages.Where(p => p.MetadataAttribute == RoleType).Count() <= 0)
        //            mdpManager.AddMetadataAtributeUsage(projectEml, RoleType, "Role", 0, 1);

        //        if (projectEml.MetadataAttributeUsages.Where(p => p.MetadataAttribute == DescriptionAttr).Count() <= 0)
        //            mdpManager.AddMetadataAtributeUsage(projectEml, DescriptionAttr, "Project description", 0, 1);

        //    }
        //    else
        //    {
        //        mdpManager.AddMetadataAtributeUsage(projectEml, Title, "Title", 0, 1);
        //        mdpManager.AddMetadataAtributeUsage(projectEml, Name, "Personnel given name", "The given name is used for the first name of the individual associated with the resource.", 0, 1);
        //        mdpManager.AddMetadataAtributeUsage(projectEml, Name, "Personnel surname", "The Sur name is used for the last name of the individual associated with the resource.", 0, 1);
        //        mdpManager.AddMetadataAtributeUsage(projectEml, RoleType, "Role", 0, 1);
        //        mdpManager.AddMetadataAtributeUsage(projectEml, DescriptionAttr, "Project description", 0, 1);
        //    }

        //#endregion

        //    #endregion
        //}

        #region helper

        private XmlDocument AddReferenceToMetadatStructure(MetadataStructure metadataStructure, string nodeName, string nodePath,string destinationPath, XmlDocument xmlDoc)
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
        private XmlNode createMissingNodes(string destinationParentXPath, XmlNode parentNode, XmlDocument doc)
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
