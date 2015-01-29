using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Xml;
using BExIS.Dlm.Entities.DataStructure;
using BExIS.Dlm.Entities.MetadataStructure;
using BExIS.Dlm.Services.DataStructure;
using BExIS.Dlm.Services.MetadataStructure;
using BExIS.Xml.Helpers;


namespace BExIS.Web.Shell.Areas.DCM.Controllers
{
    public class DcmAdminController : Controller
    {
        //
        // GET: /DCM/DcmAdmin/

        public ActionResult Index()
        {
            return View();
        }

        #region metadata

        public ActionResult XsdRead()
        {
            XsdSchemaReader xsdSchemaReader = new XsdSchemaReader();

            xsdSchemaReader.Read();

            return View("Index");
        }

        //public ActionResult CreateABCDMetadataStructure()
        //{
        //    MetadataStructureManager mdsManager = new MetadataStructureManager();
        //    MetadataPackageManager mdpManager = new MetadataPackageManager();
        //    MetadataAttributeManager mdaManager = new MetadataAttributeManager();

        //    DataTypeManager dataTypeManager = new DataTypeManager();
        //    UnitManager unitManager = new UnitManager();

        //    #region ABCD

        //    MetadataStructure abcd = mdsManager.Repo.Get(p => p.Name == "ABCD").FirstOrDefault();
        //    if (abcd == null) abcd = mdsManager.Create("ABCD", "This is the ABCD structure", "", "", null);

        //    XmlDocument xmlDoc = new XmlDocument();

        //    if (abcd.Extra != null)
        //    {
        //        xmlDoc.AppendChild(abcd.Extra);
        //    }

        //    abcd.Extra = AddNodeReferncesToMetadatStructure(abcd, "title", "Metadata/Description/Description/Title/Title",xmlDoc);
        //    mdsManager.Update(abcd);

        //    //package Person ( Tecnical contact /ContentContact)
        //    MetadataPackage person = mdpManager.MetadataPackageRepo.Get(p => p.Name == "Person").FirstOrDefault();
        //    if (person == null) person = mdpManager.Create("Person", "Person", true);

        //    //package Description
        //    MetadataPackage Description = mdpManager.MetadataPackageRepo.Get(p => p.Name == "Description").FirstOrDefault();
        //    if (Description == null) Description = mdpManager.Create("Description", "Description about a dataset", true);

        //    //package Owner
        //    MetadataPackage Owner = mdpManager.MetadataPackageRepo.Get(p => p.Name == "Owner").FirstOrDefault();
        //    if (Owner == null) Owner = mdpManager.Create("Owner", "Owner/s of the dataset", true);

        //    // Package Scope
        //    MetadataPackage Scope = mdpManager.MetadataPackageRepo.Get(p => p.Name == "Scope").FirstOrDefault();
        //    if (Scope == null) Scope = mdpManager.Create("Scope", "Scope of the dataset", true);


        //    // add package to structure
        //    if (abcd.MetadataPackageUsages != null && abcd.MetadataPackageUsages.Count > 0)
        //    {
        //        if (abcd.MetadataPackageUsages.Where(p => p.MetadataPackage == person).Count() <= 0)
        //            mdsManager.AddMetadataPackageUsage(abcd, person, "Content Contact", "", 0, 3);

        //        if (abcd.MetadataPackageUsages.Where(p => p.MetadataPackage == Description).Count() <= 0)
        //            mdsManager.AddMetadataPackageUsage(abcd, Description, "Description", "", 0, 1);

        //        if (abcd.MetadataPackageUsages.Where(p => p.MetadataPackage == Owner).Count() <= 0)
        //            mdsManager.AddMetadataPackageUsage(abcd, Owner, "Owner", "", 1, 5);

        //        if (abcd.MetadataPackageUsages.Where(p => p.MetadataPackage == Scope).Count() <= 0)
        //            mdsManager.AddMetadataPackageUsage(abcd, Scope, "Scope", "", 0, 1);
        //    }
        //    else
        //    {

        //        mdsManager.AddMetadataPackageUsage(abcd, person, "Technical Contact", "", 1, 1);
        //        mdsManager.AddMetadataPackageUsage(abcd, person, "Content Contact", "", 1, 10);
        //        mdsManager.AddMetadataPackageUsage(abcd, Description, "Description", "", 1, 1);
        //        mdsManager.AddMetadataPackageUsage(abcd, Owner, "Owner", "", 1, 10);
        //        mdsManager.AddMetadataPackageUsage(abcd, Scope, "Scope", "", 0, 1);

        //    }


        //    #region person

        //    MetadataAttribute Name = mdaManager.MetadataAttributeRepo.Get(p => p.Name.Equals("Name")).FirstOrDefault();
        //    if (Name == null)
        //    {
        //        DataType dataType = dataTypeManager.Repo.Get(p => p.SystemType.Equals("String")).FirstOrDefault();
        //        Unit unit = unitManager.Repo.Get(p => p.Name.Equals("None")).FirstOrDefault();

        //        Name = mdaManager.Create("Name", "Name", "first and last name", false, false, "David Blaa",
        //                MeasurementScale.Categorial, DataContainerType.ValueType, "", dataType, unit, null, null, null, null);
        //    }


        //    MetadataAttribute Email = mdaManager.MetadataAttributeRepo.Get(p => p.Name.Equals("Email")).FirstOrDefault();
        //    if (Email == null)
        //    {
        //        DataType dataType = dataTypeManager.Repo.Get(p => p.SystemType.Equals("String")).FirstOrDefault();
        //        Unit unit = unitManager.Repo.Get(p => p.Name.Equals("None")).FirstOrDefault();

        //        Email = mdaManager.Create("Email", "Email", "Email address", false, false, "David Blaa",
        //                MeasurementScale.Categorial, DataContainerType.ValueType, "", dataType, unit, null, null, null, null);
        //    }

        //    MetadataAttribute Address = mdaManager.MetadataAttributeRepo.Get(p => p.Name.Equals("Address")).FirstOrDefault();
        //    if (Address == null)
        //    {
        //        DataType dataType = dataTypeManager.Repo.Get(p => p.SystemType.Equals("String")).FirstOrDefault();
        //        Unit unit = unitManager.Repo.Get(p => p.Name.Equals("None")).FirstOrDefault();

        //        Address = mdaManager.Create("Address", "Address", "Address", false, false, "David Blaa",
        //                MeasurementScale.Categorial, DataContainerType.ValueType, "", dataType, unit, null, null, null, null);
        //    }

        //    MetadataAttribute Phone = mdaManager.MetadataAttributeRepo.Get(p => p.Name.Equals("Phone")).FirstOrDefault();
        //    if (Phone == null)
        //    {
        //        DataType dataType = dataTypeManager.Repo.Get(p => p.SystemType.Equals("String")).FirstOrDefault();
        //        Unit unit = unitManager.Repo.Get(p => p.Name.Equals("None")).FirstOrDefault();

        //        Phone = mdaManager.Create("Phone", "Phone", "Phone", false, false, "David Blaa",
        //                MeasurementScale.Categorial, DataContainerType.ValueType, "", dataType, unit, null, null, null, null);
        //    }


        //    if (person.MetadataAttributeUsages != null & person.MetadataAttributeUsages.Count > 0)
        //    {
        //        // add metadataAttributes to packages
        //        if (person.MetadataAttributeUsages.Where(p => p.MetadataAttribute == Name).Count() <= 0)
        //            mdpManager.AddMetadataAtributeUsage(person, Name, "Name", 1, 1);

        //        if (person.MetadataAttributeUsages.Where(p => p.MetadataAttribute == Email).Count() <= 0)
        //            mdpManager.AddMetadataAtributeUsage(person, Email, "Email", 0, 1);

        //        if (person.MetadataAttributeUsages.Where(p => p.MetadataAttribute == Address).Count() <= 0)
        //            mdpManager.AddMetadataAtributeUsage(person, Address, "Address", 0, 1);

        //        if (person.MetadataAttributeUsages.Where(p => p.MetadataAttribute == Phone).Count() <= 0)
        //            mdpManager.AddMetadataAtributeUsage(person, Phone, "Phone", 0, 1);
        //    }
        //    else
        //    {
        //        mdpManager.AddMetadataAtributeUsage(person, Name, "Name", 1, 1);
        //        mdpManager.AddMetadataAtributeUsage(person, Email, "Email", 0, 1);
        //        mdpManager.AddMetadataAtributeUsage(person, Address, "Address", 0, 1);
        //        mdpManager.AddMetadataAtributeUsage(person, Phone, "Phone", 0, 1);
        //    }

        //    #endregion

        //    #region metadata

        //    MetadataAttribute Title = mdaManager.MetadataAttributeRepo.Get(p => p.Name.Equals("Title")).FirstOrDefault();
        //    if (Title == null)
        //    {
        //        DataType dataType = dataTypeManager.Repo.Get(p => p.SystemType.Equals("String")).FirstOrDefault();
        //        Unit unit = unitManager.Repo.Get(p => p.Name.Equals("None")).FirstOrDefault();

        //        Title = mdaManager.Create("Title", "Title", "Title", false, false, "David Blaa",
        //                MeasurementScale.Categorial, DataContainerType.ValueType, "", dataType, unit, null, null, null, null);
        //    }

        //    MetadataAttribute RevisionData = mdaManager.MetadataAttributeRepo.Get(p => p.Name.Equals("RevisionData")).FirstOrDefault();
        //    if (RevisionData == null)
        //    {
        //        DataType dataType = dataTypeManager.Repo.Get(p => p.SystemType.Equals("DateTime")).FirstOrDefault();
        //        Unit unit = unitManager.Repo.Get(p => p.Name.Equals("None")).FirstOrDefault();

        //        RevisionData = mdaManager.Create("RevisionData", "RevisionData", "RevisionData", false, false, "David Blaa",
        //                MeasurementScale.Categorial, DataContainerType.ValueType, "", dataType, unit, null, null, null, null);
        //    }

        //    MetadataAttribute Details = mdaManager.MetadataAttributeRepo.Get(p => p.Name.Equals("Details")).FirstOrDefault();
        //    if (Details == null)
        //    {
        //        DataType dataType = dataTypeManager.Repo.Get(p => p.SystemType.Equals("String") && p.Name.Equals("Text")).FirstOrDefault();
        //        Unit unit = unitManager.Repo.Get(p => p.Name.Equals("None")).FirstOrDefault();

        //        Details = mdaManager.Create("Details", "Details", "Details", false, false, "David Blaa",
        //                MeasurementScale.Categorial, DataContainerType.ValueType, "", dataType, unit, null, null, null, null);
        //    }

        //    MetadataAttribute Coverage = mdaManager.MetadataAttributeRepo.Get(p => p.Name.Equals("Coverage")).FirstOrDefault();
        //    if (Coverage == null)
        //    {
        //        DataType dataType = dataTypeManager.Repo.Get(p => p.SystemType.Equals("String")).FirstOrDefault();
        //        Unit unit = unitManager.Repo.Get(p => p.Name.Equals("None")).FirstOrDefault();

        //        Coverage = mdaManager.Create("Coverage", "Coverage", "Coverage", false, false, "David Blaa",
        //                MeasurementScale.Categorial, DataContainerType.ValueType, "", dataType, unit, null, null, null, null);
        //    }

        //    MetadataAttribute URI = mdaManager.MetadataAttributeRepo.Get(p => p.Name.Equals("URI")).FirstOrDefault();
        //    if (URI == null)
        //    {
        //        DataType dataType = dataTypeManager.Repo.Get(p => p.SystemType.Equals("String")).FirstOrDefault();
        //        Unit unit = unitManager.Repo.Get(p => p.Name.Equals("None")).FirstOrDefault();

        //        URI = mdaManager.Create("URI", "URI", "URI", false, false, "David Blaa",
        //                MeasurementScale.Categorial, DataContainerType.ValueType, "", dataType, unit, null, null, null, null);
        //    }

        //    if (Description.MetadataAttributeUsages != null & Description.MetadataAttributeUsages.Count > 0)
        //    {
        //        // add metadataAttributes to packages
        //        if (Description.MetadataAttributeUsages.Where(p => p.MetadataAttribute == Title).Count() <= 0)
        //            mdpManager.AddMetadataAtributeUsage(Description, Title, "Title", 1, 1);

        //        if (Description.MetadataAttributeUsages.Where(p => p.MetadataAttribute == RevisionData).Count() <= 0)
        //            mdpManager.AddMetadataAtributeUsage(Description, RevisionData, "RevisionData", 0, 1);

        //        if (Description.MetadataAttributeUsages.Where(p => p.MetadataAttribute == Details).Count() <= 0)
        //            mdpManager.AddMetadataAtributeUsage(Description, Details, "Details", 0, 1);

        //        if (Description.MetadataAttributeUsages.Where(p => p.MetadataAttribute == Coverage).Count() <= 0)
        //            mdpManager.AddMetadataAtributeUsage(Description, Coverage, "Coverage", 0, 1);

        //        if (Description.MetadataAttributeUsages.Where(p => p.MetadataAttribute == URI).Count() <= 0)
        //            mdpManager.AddMetadataAtributeUsage(Description, URI, "URI", 0, 1);
        //    }
        //    else
        //    {
        //        mdpManager.AddMetadataAtributeUsage(Description, Title, "Title", 1, 1);
        //        mdpManager.AddMetadataAtributeUsage(Description, RevisionData, "DateModified", 0, 1);
        //        mdpManager.AddMetadataAtributeUsage(Description, Details, "Details", 0, 1);
        //        mdpManager.AddMetadataAtributeUsage(Description, Coverage, "Coverage", 0, 1);
        //        mdpManager.AddMetadataAtributeUsage(Description, URI, "URI", 0, 1);
        //    }



        //    #endregion

        //    #region Owner package

        //    MetadataAttribute Role = mdaManager.MetadataAttributeRepo.Get(p => p.Name.Equals("Role")).FirstOrDefault();
        //    if (Role == null)
        //    {
        //        DataType dataType = dataTypeManager.Repo.Get(p => p.SystemType.Equals("String")).FirstOrDefault();
        //        Unit unit = unitManager.Repo.Get(p => p.Name.Equals("None")).FirstOrDefault();

        //        Role = mdaManager.Create("Role", "Role", "Role", false, false, "David Blaa",
        //                MeasurementScale.Categorial, DataContainerType.ValueType, "", dataType, unit, null, null, null, null);
        //    }

        //    if (Owner.MetadataAttributeUsages != null & Owner.MetadataAttributeUsages.Count > 0)
        //    {
        //        // add metadataAttributes to packages
        //        if (Owner.MetadataAttributeUsages.Where(p => p.MetadataAttribute == Name).Count() <= 0)
        //            mdpManager.AddMetadataAtributeUsage(Owner, Name, "Full Name", 1, 1);

        //        if (Owner.MetadataAttributeUsages.Where(p => p.MetadataAttribute == Name).Count() <= 0)
        //            mdpManager.AddMetadataAtributeUsage(Owner, Name, "Sorting Name", 0, 1);

        //        if (Owner.MetadataAttributeUsages.Where(p => p.MetadataAttribute == Name).Count() <= 0)
        //            mdpManager.AddMetadataAtributeUsage(Owner, Name, "Organisation Name", 0, 1);

        //        if (Owner.MetadataAttributeUsages.Where(p => p.MetadataAttribute == Role).Count() <= 0)
        //            mdpManager.AddMetadataAtributeUsage(Owner, Role, "Role", 0, 1);

        //        if (Owner.MetadataAttributeUsages.Where(p => p.MetadataAttribute == Address).Count() <= 0)
        //            mdpManager.AddMetadataAtributeUsage(Owner, Address, "Address", 0, 1);

        //        if (Owner.MetadataAttributeUsages.Where(p => p.MetadataAttribute == Email).Count() <= 0)
        //            mdpManager.AddMetadataAtributeUsage(Owner, Email, "Email", 0, 1);

        //        if (Owner.MetadataAttributeUsages.Where(p => p.MetadataAttribute == Phone).Count() <= 0)
        //            mdpManager.AddMetadataAtributeUsage(Owner, Phone, "Phone", 0, 1);

        //        if (Owner.MetadataAttributeUsages.Where(p => p.MetadataAttribute == URI).Count() <= 0)
        //            mdpManager.AddMetadataAtributeUsage(Owner, URI, "URI", 0, 1);

        //    }
        //    else
        //    {
        //        mdpManager.AddMetadataAtributeUsage(Owner, Name, "Full Name", 1, 1);
        //        mdpManager.AddMetadataAtributeUsage(Owner, Name, "Sorting Name", 0, 1);
        //        mdpManager.AddMetadataAtributeUsage(Owner, Name, "Organisation Name", 0, 1);
        //        mdpManager.AddMetadataAtributeUsage(Owner, Address, "Address", 0, 1);
        //        mdpManager.AddMetadataAtributeUsage(Owner, Email, "Email", 0, 1);
        //        mdpManager.AddMetadataAtributeUsage(Owner, Role, "Role", 0, 1);
        //        mdpManager.AddMetadataAtributeUsage(Owner, Phone, "Phone", 0, 1);
        //        mdpManager.AddMetadataAtributeUsage(Owner, URI, "URI", 0, 1);

        //    }

        //    #endregion

        //    #region Scope package

        //    MetadataAttribute TaxonomicTerm = mdaManager.MetadataAttributeRepo.Get(p => p.Name.Equals("TaxonomicTerm")).FirstOrDefault();
        //    if (TaxonomicTerm == null)
        //    {
        //        DataType dataType = dataTypeManager.Repo.Get(p => p.SystemType.Equals("String")).FirstOrDefault();
        //        Unit unit = unitManager.Repo.Get(p => p.Name.Equals("None")).FirstOrDefault();

        //        TaxonomicTerm = mdaManager.Create("TaxonomicTerm", "TaxonomicTerm", "TaxonomicTerm", false, false, "David Blaa",
        //                MeasurementScale.Categorial, DataContainerType.ValueType, "", dataType, unit, null, null, null, null);
        //    }

        //    MetadataAttribute GeoEcologicalTerm = mdaManager.MetadataAttributeRepo.Get(p => p.Name.Equals("GeoEcologicalTerm")).FirstOrDefault();
        //    if (GeoEcologicalTerm == null)
        //    {
        //        DataType dataType = dataTypeManager.Repo.Get(p => p.SystemType.Equals("String")).FirstOrDefault();
        //        Unit unit = unitManager.Repo.Get(p => p.Name.Equals("None")).FirstOrDefault();

        //        GeoEcologicalTerm = mdaManager.Create("GeoEcologicalTerm", "GeoEcologicalTerm", "GeoEcologicalTerm", false, false, "David Blaa",
        //                MeasurementScale.Categorial, DataContainerType.ValueType, "", dataType, unit, null, null, null, null);
        //    }

        //    if (Scope.MetadataAttributeUsages != null & Scope.MetadataAttributeUsages.Count > 0)
        //    {
        //        // add metadataAttributes to packages
        //        if (Scope.MetadataAttributeUsages.Where(p => p.MetadataAttribute == TaxonomicTerm).Count() <= 0)
        //            mdpManager.AddMetadataAtributeUsage(Scope, TaxonomicTerm, "TaxonomicTerm", 0, 10);

        //        if (Scope.MetadataAttributeUsages.Where(p => p.MetadataAttribute == GeoEcologicalTerm).Count() <= 0)
        //            mdpManager.AddMetadataAtributeUsage(Scope, GeoEcologicalTerm, "GeoEcologicalTerm", 0, 10);

        //    }
        //    else
        //    {
        //        mdpManager.AddMetadataAtributeUsage(Scope, TaxonomicTerm, "TaxonomicTerm", 0, 10);
        //        mdpManager.AddMetadataAtributeUsage(Scope, GeoEcologicalTerm, "GeoEcologicalTerm", 0, 10);
        //    }


        //    #endregion

        //    #region Unit (ABCD Part)

        //    MetadataPackage Unit = mdpManager.MetadataPackageRepo.Get(p => p.Name == "Unit").FirstOrDefault();
        //    if (Unit == null) Unit = mdpManager.Create("Unit", "A container for all data referring to a unit (specimen or observation record).", true);

        //    if (abcd.MetadataPackageUsages != null && abcd.MetadataPackageUsages.Count > 0)
        //    {
        //        if (abcd.MetadataPackageUsages.Where(p => p.MetadataPackage == Unit).Count() <= 0)
        //        {
        //            mdsManager.AddMetadataPackageUsage(abcd, Unit, "Unit", "", 1, 5);
        //        }

        //    }
        //    else
        //    {

        //        mdsManager.AddMetadataPackageUsage(abcd, Unit, "Unit", "", 1, 5);
        //    }

        //    // metadata attributes for Unit
        //    MetadataAttribute Id = mdaManager.MetadataAttributeRepo.Get(p => p.Name.Equals("Id")).FirstOrDefault();
        //    if (Id == null)
        //    {
        //        DataType dataType = dataTypeManager.Repo.Get(p => p.SystemType.Equals("String")).FirstOrDefault();
        //        Unit unit = unitManager.Repo.Get(p => p.Name.Equals("None")).FirstOrDefault();

        //        Id = mdaManager.Create("Id", "Id", "Name or code of the data source", false, false, "David Blaa",
        //                MeasurementScale.Categorial, DataContainerType.ValueType, "", dataType, unit, null, null, null, null);
        //    }

        //    // MetadataAttribute Usage
        //    if (Unit.MetadataAttributeUsages != null & Unit.MetadataAttributeUsages.Count > 0)
        //    {
        //        // add metadataAttributes to packages
        //        if (Unit.MetadataAttributeUsages.Where(p => p.MetadataAttribute == Id).Count() <= 0)
        //        {

        //            MetadataAttributeUsage mau = mdpManager.AddMetadataAtributeUsage(Unit, Id, "SourceInstitutionID", 1, 1);
        //        }

        //        if (Unit.MetadataAttributeUsages.Where(p => p.MetadataAttribute == Id).Count() <= 0)
        //            mdpManager.AddMetadataAtributeUsage(Unit, Id, "SourceID", 1, 1);

        //        if (Unit.MetadataAttributeUsages.Where(p => p.MetadataAttribute == Id).Count() <= 0)
        //            mdpManager.AddMetadataAtributeUsage(Unit, Id, "UnitID", 1, 1);

        //    }
        //    else
        //    {
        //        mdpManager.AddMetadataAtributeUsage(Unit, Id, "SourceInstitutionID", 1, 1);
        //        mdpManager.AddMetadataAtributeUsage(Unit, Id, "SourceID", 1, 1);
        //        mdpManager.AddMetadataAtributeUsage(Unit, Id, "UnitID", 1, 1);
        //    }


        //    #endregion

        //    #region Exploratory stuff

        //    MetadataStructure exploratory = mdsManager.Repo.Get(p => p.Name == "Exploratory").FirstOrDefault();
        //    if (exploratory == null) exploratory = mdsManager.Create("Exploratory", "This is the exploratory metadata structure", "", "", abcd);

        //    if (exploratory.Extra == null)
        //    {
        //        XmlDocument extraDoc = new XmlDocument();
        //        extraDoc.LoadXml("<infos>" +
        //                         " <info name='title' value=\"Metadata/Description/Description/Title/Title\" />" +
        //                         "</infos>"
        //            );

        //        exploratory.Extra = extraDoc;
        //        mdsManager.Update(exploratory);
        //    }

        //    MetadataPackage DataDescription = mdpManager.MetadataPackageRepo.Get(p => p.Name == "DataDescription").FirstOrDefault();
        //    if (DataDescription == null) DataDescription = mdpManager.Create("DataDescription", "Description of data", true);

        //    MetadataPackage ResearchObjects = mdpManager.MetadataPackageRepo.Get(p => p.Name == "ResearchObjects").FirstOrDefault();
        //    if (ResearchObjects == null) ResearchObjects = mdpManager.Create("ResearchObjects", "Information about research objects", true);

        //    // add package to structure
        //    if (exploratory.MetadataPackageUsages != null & exploratory.MetadataPackageUsages.Count > 0)
        //    {
        //        if (exploratory.MetadataPackageUsages.Where(p => p.MetadataPackage == DataDescription).Count() <= 0)
        //            mdsManager.AddMetadataPackageUsage(exploratory, DataDescription, "Data Description", "", 0, 1);

        //        if (exploratory.MetadataPackageUsages.Where(p => p.MetadataPackage == ResearchObjects).Count() <= 0)
        //            mdsManager.AddMetadataPackageUsage(exploratory, ResearchObjects, "Research Objects", "", 0, 1);
        //    }
        //    else
        //    {
        //        mdsManager.AddMetadataPackageUsage(exploratory, DataDescription, "Data Description", "", 0, 1);
        //        mdsManager.AddMetadataPackageUsage(exploratory, ResearchObjects, "Research Objects", "", 0, 1);
        //    }

        //    #region data description

        //    MetadataAttribute qualityLevel = mdaManager.MetadataAttributeRepo.Get(p => p.Name.Equals("QualityLevel")).FirstOrDefault();
        //    if (qualityLevel == null)
        //    {
        //        DataType dataType = dataTypeManager.Repo.Get(p => p.SystemType.Equals("String")).FirstOrDefault();
        //        Unit unit = unitManager.Repo.Get(p => p.Name.Equals("None")).FirstOrDefault();

        //        qualityLevel = mdaManager.Create("QualityLevel", "QualityLevel", "QualityLevel", false, false, "David Blaa",
        //                MeasurementScale.Categorial, DataContainerType.ValueType, "", dataType, unit, null, null, null, null);
        //    }

        //    MetadataAttribute dataStatus = mdaManager.MetadataAttributeRepo.Get(p => p.Name.Equals("DataStatus")).FirstOrDefault();
        //    if (dataStatus == null)
        //    {
        //        DataType dataType = dataTypeManager.Repo.Get(p => p.SystemType.Equals("String")).FirstOrDefault();
        //        Unit unit = unitManager.Repo.Get(p => p.Name.Equals("None")).FirstOrDefault();

        //        dataStatus = mdaManager.Create("DataStatus", "DataStatus", "DataStatus", false, false, "David Blaa",
        //                MeasurementScale.Categorial, DataContainerType.ValueType, "", dataType, unit, null, null, null, null);
        //    }

        //    if (DataDescription.MetadataAttributeUsages != null & DataDescription.MetadataAttributeUsages.Count > 0)
        //    {
        //        // add metadataAttributes to packages
        //        if (DataDescription.MetadataAttributeUsages.Where(p => p.MetadataAttribute == qualityLevel).Count() <= 0)
        //            mdpManager.AddMetadataAtributeUsage(DataDescription, qualityLevel, "Quality Level", 0, 1);

        //        if (DataDescription.MetadataAttributeUsages.Where(p => p.MetadataAttribute == dataStatus).Count() <= 0)
        //            mdpManager.AddMetadataAtributeUsage(DataDescription, dataStatus, "Data Status", 0, 1);

        //    }
        //    else
        //    {
        //        mdpManager.AddMetadataAtributeUsage(DataDescription, qualityLevel, "Quality Level", 0, 1);
        //        mdpManager.AddMetadataAtributeUsage(DataDescription, dataStatus, "Data Status", 0, 1);
        //    }

        //    #endregion


        //    #region research objects

        //    MetadataAttribute processes = mdaManager.MetadataAttributeRepo.Get(p => p.Name.Equals("Processe")).FirstOrDefault();
        //    if (processes == null)
        //    {
        //        DataType dataType = dataTypeManager.Repo.Get(p => p.SystemType.Equals("String")).FirstOrDefault();
        //        Unit unit = unitManager.Repo.Get(p => p.Name.Equals("None")).FirstOrDefault();

        //        processes = mdaManager.Create("Processe", "Processe", "Processe", false, false, "David Blaa",
        //                MeasurementScale.Categorial, DataContainerType.ValueType, "", dataType, unit, null, null, null, null);
        //    }

        //    MetadataAttribute Services = mdaManager.MetadataAttributeRepo.Get(p => p.Name.Equals("Service")).FirstOrDefault();
        //    if (Services == null)
        //    {
        //        DataType dataType = dataTypeManager.Repo.Get(p => p.SystemType.Equals("String")).FirstOrDefault();
        //        Unit unit = unitManager.Repo.Get(p => p.Name.Equals("None")).FirstOrDefault();

        //        Services = mdaManager.Create("Service", "Service", "Service", false, false, "David Blaa",
        //                MeasurementScale.Categorial, DataContainerType.ValueType, "", dataType, unit, null, null, null, null);
        //    }

        //    if (ResearchObjects.MetadataAttributeUsages != null & ResearchObjects.MetadataAttributeUsages.Count > 0)
        //    {
        //        // add metadataAttributes to packages
        //        if (ResearchObjects.MetadataAttributeUsages.Where(p => p.MetadataAttribute == processes).Count() <= 0)
        //            mdpManager.AddMetadataAtributeUsage(ResearchObjects, processes, "Processe", 0, 10);

        //        if (ResearchObjects.MetadataAttributeUsages.Where(p => p.MetadataAttribute == dataStatus).Count() <= 0)
        //            mdpManager.AddMetadataAtributeUsage(ResearchObjects, Services, "Service", 0, 10);

        //    }
        //    else
        //    {
        //        mdpManager.AddMetadataAtributeUsage(ResearchObjects, processes, "Processe", 0, 10);
        //        mdpManager.AddMetadataAtributeUsage(ResearchObjects, Services, "Service", 0, 10);
        //    }
        //    #endregion
        //    #endregion

        //    #endregion

        //    return View("CreateMetadataStructure");
        //}

        public ActionResult CreateEMLMetadataStructure()
        {

            #region eml

            //CreateEmlDatasetAdv();

            #endregion

            return View("CreateMetadataStructure");
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
        //        xmlDoc.AppendChild(eml.Extra);
        //    }

        //    eml.Extra = AddNodeReferncesToMetadatStructure(eml, "title", "Metadata/Description/DescriptionEML/Title/Title", xmlDoc);
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
        //            mdsManager.AddMetadataPackageUsage(eml, DescEml, "Description", "", 1, 1);

        //        if (eml.MetadataPackageUsages.Where(p => p.MetadataPackage == personEml).Count() <= 0)
        //            mdsManager.AddMetadataPackageUsage(eml, personEml, "Creator", "", 1, 5);

        //        if (eml.MetadataPackageUsages.Where(p => p.MetadataPackage == personEml).Count() <= 0)
        //            mdsManager.AddMetadataPackageUsage(eml, personEml, "Contact", "", 1, 5);

        //        if (eml.MetadataPackageUsages.Where(p => p.MetadataPackage == projectEml).Count() <= 0)
        //            mdsManager.AddMetadataPackageUsage(eml, projectEml, "Project", "", 1, 1);
        //    }
        //    else
        //    {
        //        mdsManager.AddMetadataPackageUsage(eml, DescEml, "Description", "", 1, 1);
        //        mdsManager.AddMetadataPackageUsage(eml, personEml, "Creator", "", 1, 5);
        //        mdsManager.AddMetadataPackageUsage(eml, personEml, "Contact", "", 1, 5);
        //        mdsManager.AddMetadataPackageUsage(eml, projectEml, "Project", "", 1, 1);
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
        //            mdpManager.AddMetadataAtributeUsage(DescEml, Title, "Title", 1, 1);

        //    }
        //    else
        //    {
        //        mdpManager.AddMetadataAtributeUsage(DescEml, Title, "Title", 1, 1);
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
        //            mdpManager.AddMetadataAtributeUsage(personEml, Name, "Given name", 1, 1);

        //        if (personEml.MetadataAttributeUsages.Where(p => p.MetadataAttribute == Name).Count() <= 0)
        //            mdpManager.AddMetadataAtributeUsage(personEml, Name, "Sur name", 1, 1);

        //        if (personEml.MetadataAttributeUsages.Where(p => p.MetadataAttribute == Name).Count() <= 0)
        //            mdpManager.AddMetadataAtributeUsage(personEml, Name, "Organization", 1, 1);

        //    }
        //    else
        //    {
        //        mdpManager.AddMetadataAtributeUsage(personEml, Name, "Given name", 1, 1);
        //        mdpManager.AddMetadataAtributeUsage(personEml, Name, "Sur name", 1, 1);
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
        //            mdpManager.AddMetadataAtributeUsage(projectEml, Title, "Title", 0, 1);

        //        if (projectEml.MetadataAttributeUsages.Where(p => p.MetadataAttribute == Name).Count() <= 0)
        //            mdpManager.AddMetadataAtributeUsage(projectEml, Name, "Personnel given name", 0, 1);

        //        if (projectEml.MetadataAttributeUsages.Where(p => p.MetadataAttribute == Name).Count() <= 0)
        //            mdpManager.AddMetadataAtributeUsage(projectEml, Name, "Personnel sur name", 0, 1);

        //        if (projectEml.MetadataAttributeUsages.Where(p => p.MetadataAttribute == Role).Count() <= 0)
        //            mdpManager.AddMetadataAtributeUsage(projectEml, Role, "Role", 0, 1);

        //        if (projectEml.MetadataAttributeUsages.Where(p => p.MetadataAttribute == DescriptionAttr).Count() <= 0)
        //            mdpManager.AddMetadataAtributeUsage(projectEml, DescriptionAttr, "Project description", 0, 1);

        //    }
        //    else
        //    {
        //        mdpManager.AddMetadataAtributeUsage(projectEml, Title, "Title", 0, 1);
        //        mdpManager.AddMetadataAtributeUsage(projectEml, Name, "Personnel given name", 0, 1);
        //        mdpManager.AddMetadataAtributeUsage(projectEml, Name, "Personnel sur name", 0, 1);
        //        mdpManager.AddMetadataAtributeUsage(projectEml, Role, "Role", 0, 1);
        //        mdpManager.AddMetadataAtributeUsage(projectEml, DescriptionAttr, "Project description", 0, 1);
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

        //    eml.Extra = AddNodeReferncesToMetadatStructure(eml, "title", "Metadata/Description/DescriptionEML/Title/Title", xmlDoc);
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
        //            mdsManager.AddMetadataPackageUsage(eml, DescEml, "Description","", 1, 1);

        //        if (eml.MetadataPackageUsages.Where(p => p.MetadataPackage == personEml).Count() <= 0)
        //            mdsManager.AddMetadataPackageUsage(eml, personEml, "Creator", "", 1, 5);

        //        if (eml.MetadataPackageUsages.Where(p => p.MetadataPackage == party).Count() <= 0)
        //            mdsManager.AddMetadataPackageUsage(eml, party, "Associated Party ", "", 1, 10);

        //        if (eml.MetadataPackageUsages.Where(p => p.MetadataPackage == personEml).Count() <= 0)
        //            mdsManager.AddMetadataPackageUsage(eml, personEml, "Contact", "", 1, 5);

        //        if (eml.MetadataPackageUsages.Where(p => p.MetadataPackage == projectEml).Count() <= 0)
        //            mdsManager.AddMetadataPackageUsage(eml, projectEml, "Project", "", 1, 1);

        //        //if (eml.MetadataPackageUsages.Where(p => p.MetadataPackage == coverage).Count() <= 0)
        //        //    mdsManager.AddMetadataPackageUsage(eml, coverage, "Coverage", 1, 1);
        //    }
        //    else
        //    {
        //        mdsManager.AddMetadataPackageUsage(eml, DescEml, "Description", "", 1, 1);
        //        mdsManager.AddMetadataPackageUsage(eml, personEml, "Creator", "", 1, 5);
        //        mdsManager.AddMetadataPackageUsage(eml, party, "Associated Parties ", "", 1, 10);
        //        mdsManager.AddMetadataPackageUsage(eml, personEml, "Contact", "", 1, 5);
        //        mdsManager.AddMetadataPackageUsage(eml, projectEml, "Project", "", 1, 1);
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
        //        DataType dataType = dataTypeManager.Repo.Get(p => p.SystemType.Equals("Text")).FirstOrDefault();
        //        Unit unit = unitManager.Repo.Get(p => p.Name.Equals("None")).FirstOrDefault();

        //        Info = mdaManager.Create("Information", "Information", "Information", false, false, "David Blaa",
        //                MeasurementScale.Categorial, DataContainerType.ValueType, "", dataType, unit, null, null, null, null);
        //    }


        //    #endregion

        //    #region add attr

        //        if (DescEml.MetadataAttributeUsages != null & DescEml.MetadataAttributeUsages.Count > 0)
        //        {
        //            // add metadataAttributes to packages
        //            if (DescEml.MetadataAttributeUsages.Where(p => p.MetadataAttribute == Name).Count() <= 0)
        //                mdpManager.AddMetadataAtributeUsage(DescEml, Name, "Short Name", 0, 1);

        //            if (DescEml.MetadataAttributeUsages.Where(p => p.MetadataAttribute == Title).Count() <= 0)
        //                mdpManager.AddMetadataAtributeUsage(DescEml, Title, "Title", 1, 1);

        //            if (DescEml.MetadataAttributeUsages.Where(p => p.MetadataAttribute == Date).Count() <= 0)
        //                mdpManager.AddMetadataAtributeUsage(DescEml, Date, "Publish Date", 0, 1);

        //            if (DescEml.MetadataAttributeUsages.Where(p => p.MetadataAttribute == Info).Count() <= 0)
        //                mdpManager.AddMetadataAtributeUsage(DescEml, Info, "Additional Information", 0, 1);


        //        }
        //        else
        //        {
        //            mdpManager.AddMetadataAtributeUsage(DescEml, Name, "Short Name", 0, 1);
        //            mdpManager.AddMetadataAtributeUsage(DescEml, Title, "Title", 1, 1);
        //            mdpManager.AddMetadataAtributeUsage(DescEml, Date, "Publish Date", 0, 1);
        //            mdpManager.AddMetadataAtributeUsage(DescEml, Info, "Additional Information", 0, 1);
        //        }


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
        //            mdpManager.AddMetadataAtributeUsage(personEml, Name, "Given name", 1, 1);

        //        if (personEml.MetadataAttributeUsages.Where(p => p.MetadataAttribute == Name).Count() <= 0)
        //            mdpManager.AddMetadataAtributeUsage(personEml, Name, "Surname", 1, 1);

        //        if (personEml.MetadataAttributeUsages.Where(p => p.MetadataAttribute == Name).Count() <= 0)
        //            mdpManager.AddMetadataAtributeUsage(personEml, Name, "Organization", 1, 1);

        //    }
        //    else
        //    {
        //        mdpManager.AddMetadataAtributeUsage(personEml, Name, "Given name", 1, 1);
        //        mdpManager.AddMetadataAtributeUsage(personEml, Name, "Sur name", 1, 1);
        //        mdpManager.AddMetadataAtributeUsage(personEml, Name, "Organization", 1, 1);
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
        //            mdpManager.AddMetadataAtributeUsage(party, Name, "Given name", 1, 1);

        //        if (party.MetadataAttributeUsages.Where(p => p.MetadataAttribute == Name).Count() <= 0)
        //            mdpManager.AddMetadataAtributeUsage(party, Name, "Surname", 1, 1);

        //        if (party.MetadataAttributeUsages.Where(p => p.MetadataAttribute == Name).Count() <= 0)
        //            mdpManager.AddMetadataAtributeUsage(party, Name, "Organization", 1, 1);

        //        if (party.MetadataAttributeUsages.Where(p => p.MetadataAttribute == Name).Count() <= 0)
        //            mdpManager.AddMetadataAtributeUsage(party, RoleType, "Role", 1, 1);

        //    }
        //    else
        //    {
        //        mdpManager.AddMetadataAtributeUsage(party, Name, "Given name", 1, 1);
        //        mdpManager.AddMetadataAtributeUsage(party, Name, "Sur name", 1, 1);
        //        mdpManager.AddMetadataAtributeUsage(party, Name, "Organization", 1, 1);
        //        mdpManager.AddMetadataAtributeUsage(party, RoleType, "Role", 1, 1);
        //    }

        //    #endregion

        //    #endregion

        //    #region Project Eml

        //    #region create attr

        //    MetadataAttribute DescriptionAttr = mdaManager.MetadataAttributeRepo.Get(p => p.Name.Equals("Description")).FirstOrDefault();
        //    if (DescriptionAttr == null)
        //    {
        //        DataType dataType = dataTypeManager.Repo.Get(p => p.SystemType.Equals("Text")).FirstOrDefault();
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

        //        MetadataAttribute Role = mdaManager.MetadataAttributeRepo.Get(p => p.Name.Equals("Role")).FirstOrDefault();
        //        if (Role == null)
        //        {
        //            DataType dataType = dataTypeManager.Repo.Get(p => p.SystemType.Equals("String")).FirstOrDefault();
        //            Unit unit = unitManager.Repo.Get(p => p.Name.Equals("None")).FirstOrDefault();

        //            Role = mdaManager.Create("Role", "Role", "Role", false, false, "David Blaa",
        //                    MeasurementScale.Categorial, DataContainerType.ValueType, "", dataType, unit, null, null, null, null);
        //        }

        //        // add metadataAttributes to packages
        //        if (projectEml.MetadataAttributeUsages.Where(p => p.MetadataAttribute == Title).Count() <= 0)
        //            mdpManager.AddMetadataAtributeUsage(projectEml, Title, "Title", 0, 1);

        //        if (projectEml.MetadataAttributeUsages.Where(p => p.MetadataAttribute == Name).Count() <= 0)
        //            mdpManager.AddMetadataAtributeUsage(projectEml, Name, "Personnel given name", 0, 1);

        //        if (projectEml.MetadataAttributeUsages.Where(p => p.MetadataAttribute == Name).Count() <= 0)
        //            mdpManager.AddMetadataAtributeUsage(projectEml, Name, "Personnel surname", 0, 1);

        //        if (projectEml.MetadataAttributeUsages.Where(p => p.MetadataAttribute == Role).Count() <= 0)
        //            mdpManager.AddMetadataAtributeUsage(projectEml, Role, "Role", 0, 1);

        //        if (projectEml.MetadataAttributeUsages.Where(p => p.MetadataAttribute == DescriptionAttr).Count() <= 0)
        //            mdpManager.AddMetadataAtributeUsage(projectEml, DescriptionAttr, "Project description", 0, 1);

        //    }
        //    else
        //    {
        //        mdpManager.AddMetadataAtributeUsage(projectEml, Title, "Title", 0, 1);
        //        mdpManager.AddMetadataAtributeUsage(projectEml, Name, "Personnel given name", 0, 1);
        //        mdpManager.AddMetadataAtributeUsage(projectEml, Name, "Personnel surname", 0, 1);
        //        mdpManager.AddMetadataAtributeUsage(projectEml, RoleType, "Role", 0, 1);
        //        mdpManager.AddMetadataAtributeUsage(projectEml, DescriptionAttr, "Project description", 0, 1);
        //    }


        //#endregion

        //#endregion

        //}

        #region helper

        private XmlDocument AddNodeReferncesToMetadatStructure(MetadataStructure metadataStructure, string nodeName, string nodePath, XmlDocument xmlDoc)
        {

            string destinationPath = "extra/nodeReferences/nodeRef";

            XmlDocument doc = xmlDoc;
            XmlNode extra;

            if(metadataStructure.Extra !=null)
            {

                extra = ((XmlDocument)metadataStructure.Extra).DocumentElement;
            }
            else
            {
                extra = doc.CreateElement("extra","");
            }

            doc.AppendChild(extra);

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
