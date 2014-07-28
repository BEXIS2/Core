using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using BExIS.Dlm.Entities.DataStructure;
using BExIS.Dlm.Entities.MetadataStructure;
using BExIS.Dlm.Services.DataStructure;
using BExIS.Dlm.Services.MetadataStructure;
using BExIS.Xml.Services;

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

        public ActionResult CreateMetadataStructure()
        {
            MetadataStructureManager mdsManager = new MetadataStructureManager();
            MetadataPackageManager mdpManager = new MetadataPackageManager();
            MetadataAttributeManager mdaManager = new MetadataAttributeManager();

            DataTypeManager dataTypeManager = new DataTypeManager();
            UnitManager unitManager = new UnitManager();


            MetadataStructure root = mdsManager.Repo.Get(p => p.Name == "ABCD").FirstOrDefault();
            if (root == null) root = mdsManager.Create("ABCD", "This is the ABCD structure", "", "", null);

            //package Person ( Tecnical contact /ContentContact)
            MetadataPackage contact = mdpManager.MetadataPackageRepo.Get(p => p.Name == "Contact").FirstOrDefault();
            if (contact == null) contact = mdpManager.Create("Contact", "Contact", true);

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
            if (root.MetadataPackageUsages!=null && root.MetadataPackageUsages.Count > 0)
            {
                if (root.MetadataPackageUsages.Where(p => p.MetadataPackage == contact).Count() <= 0)
                    mdsManager.AddMetadataPackageUsage(root, contact, "Content Contact", 0, 3);

                if (root.MetadataPackageUsages.Where(p => p.MetadataPackage == Description).Count() <= 0)
                    mdsManager.AddMetadataPackageUsage(root, Description, "Description", 0, 1);

                if (root.MetadataPackageUsages.Where(p => p.MetadataPackage == Owner).Count() <= 0)
                    mdsManager.AddMetadataPackageUsage(root, Owner, "Owner", 1, 5);

                if (root.MetadataPackageUsages.Where(p => p.MetadataPackage == Scope).Count() <= 0)
                    mdsManager.AddMetadataPackageUsage(root, Scope, "Scope", 0, 1);
            }
            else
            {
                
                mdsManager.AddMetadataPackageUsage(root, contact, "Technical Contact", 1, 1);
                mdsManager.AddMetadataPackageUsage(root, contact, "Content Contact", 1, 3);
                mdsManager.AddMetadataPackageUsage(root, Description, "Description", 1, 1);
                mdsManager.AddMetadataPackageUsage(root, Owner, "Owner", 1, 5);
                mdsManager.AddMetadataPackageUsage(root, Scope, "Scope", 0, 1);

            }


            #region person

            MetadataAttribute Name = mdaManager.MetadataAttributeRepo.Get(p => p.Name.Equals("Name")).FirstOrDefault();
            if (Name == null)
            {
                DataType dataType = dataTypeManager.Repo.Get(p => p.SystemType.Equals("String")).FirstOrDefault();
                Unit unit = unitManager.Repo.Get(p => p.Name.Equals("None")).FirstOrDefault();

                Name = mdaManager.Create("Name", "Name", "first and last name", false, false, "David Blaa",
                        MeasurementScale.Categorial, DataContainerType.ValueType, "", dataType, unit, null, null, null, null);
            }


            MetadataAttribute Email = mdaManager.MetadataAttributeRepo.Get(p => p.Name.Equals("EMail")).FirstOrDefault();
            if (Email == null)
            {
                DataType dataType = dataTypeManager.Repo.Get(p => p.SystemType.Equals("String")).FirstOrDefault();
                Unit unit = unitManager.Repo.Get(p => p.Name.Equals("None")).FirstOrDefault();

                Email = mdaManager.Create("EMail", "EMail", "EMail address", false, false, "David Blaa",
                        MeasurementScale.Categorial, DataContainerType.ValueType, "", dataType, unit, null, null, null, null);
            }

            MetadataAttribute Address = mdaManager.MetadataAttributeRepo.Get(p => p.Name.Equals("Address")).FirstOrDefault();
            if (Address == null)
            {
                DataType dataType = dataTypeManager.Repo.Get(p => p.SystemType.Equals("String")).FirstOrDefault();
                Unit unit = unitManager.Repo.Get(p => p.Name.Equals("None")).FirstOrDefault();

                Address = mdaManager.Create("Address", "Address", "Address", false, false, "David Blaa",
                        MeasurementScale.Categorial, DataContainerType.ValueType, "", dataType, unit, null, null, null, null);
            }

            MetadataAttribute Phone = mdaManager.MetadataAttributeRepo.Get(p => p.Name.Equals("Phone")).FirstOrDefault();
            if (Phone == null)
            {
                DataType dataType = dataTypeManager.Repo.Get(p => p.SystemType.Equals("String")).FirstOrDefault();
                Unit unit = unitManager.Repo.Get(p => p.Name.Equals("None")).FirstOrDefault();

                Phone = mdaManager.Create("Phone", "Phone", "Phone", false, false, "David Blaa",
                        MeasurementScale.Categorial, DataContainerType.ValueType, "", dataType, unit, null, null, null, null);
            }


            if (contact.MetadataAttributeUsages!=null &contact.MetadataAttributeUsages.Count > 0)
            {
                // add metadataAttributes to packages
                if (contact.MetadataAttributeUsages.Where(p => p.MetadataAttribute == Name).Count() <= 0)
                    mdpManager.AddMetadataAtributeUsage(contact, Name, "Name", 1, 1);

                if (contact.MetadataAttributeUsages.Where(p => p.MetadataAttribute == Email).Count() <= 0)
                    mdpManager.AddMetadataAtributeUsage(contact, Email, "EMail", 0, 1);

                if (contact.MetadataAttributeUsages.Where(p => p.MetadataAttribute == Address).Count() <= 0)
                    mdpManager.AddMetadataAtributeUsage(contact, Address, "Address", 0, 1);

                if (contact.MetadataAttributeUsages.Where(p => p.MetadataAttribute == Phone).Count() <= 0)
                    mdpManager.AddMetadataAtributeUsage(contact, Phone, "Phone", 0, 1);
            }
            else
            {
                mdpManager.AddMetadataAtributeUsage(contact, Name, "Name", 1, 1);
                mdpManager.AddMetadataAtributeUsage(contact, Email, "EMail", 0, 1);
                mdpManager.AddMetadataAtributeUsage(contact, Address, "Address", 0, 1);
                mdpManager.AddMetadataAtributeUsage(contact, Phone, "Phone", 0, 1);
            }

            #endregion

            #region metadata

            MetadataAttribute Title = mdaManager.MetadataAttributeRepo.Get(p => p.Name.Equals("Title")).FirstOrDefault();
            if (Title == null)
            {
                DataType dataType = dataTypeManager.Repo.Get(p => p.SystemType.Equals("String")).FirstOrDefault();
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
                DataType dataType = dataTypeManager.Repo.Get(p => p.SystemType.Equals("String") && p.Name.Equals("Text")).FirstOrDefault();
                Unit unit = unitManager.Repo.Get(p => p.Name.Equals("None")).FirstOrDefault();

                Details = mdaManager.Create("Details", "Details", "Details", false, false, "David Blaa",
                        MeasurementScale.Categorial, DataContainerType.ValueType, "", dataType, unit, null, null, null, null);
            }

            MetadataAttribute Coverage = mdaManager.MetadataAttributeRepo.Get(p => p.Name.Equals("Coverage")).FirstOrDefault();
            if (Coverage == null)
            {
                DataType dataType = dataTypeManager.Repo.Get(p => p.SystemType.Equals("String")).FirstOrDefault();
                Unit unit = unitManager.Repo.Get(p => p.Name.Equals("None")).FirstOrDefault();

                Coverage = mdaManager.Create("Coverage", "Coverage", "Coverage", false, false, "David Blaa",
                        MeasurementScale.Categorial, DataContainerType.ValueType, "", dataType, unit, null, null, null, null);
            }

            MetadataAttribute Uri = mdaManager.MetadataAttributeRepo.Get(p => p.Name.Equals("Uri")).FirstOrDefault();
            if (Uri == null)
            {
                DataType dataType = dataTypeManager.Repo.Get(p => p.SystemType.Equals("String")).FirstOrDefault();
                Unit unit = unitManager.Repo.Get(p => p.Name.Equals("None")).FirstOrDefault();

                Uri = mdaManager.Create("Uri", "Uri", "Uri", false, false, "David Blaa",
                        MeasurementScale.Categorial, DataContainerType.ValueType, "", dataType, unit, null, null, null, null);
            }

            if (Description.MetadataAttributeUsages!=null & Description.MetadataAttributeUsages.Count > 0)
            {
                // add metadataAttributes to packages
                if (Description.MetadataAttributeUsages.Where(p => p.MetadataAttribute == Title).Count() <= 0)
                    mdpManager.AddMetadataAtributeUsage(Description, Title, "Title", 1, 2);

                if (Description.MetadataAttributeUsages.Where(p => p.MetadataAttribute == RevisionData).Count() <= 0)
                    mdpManager.AddMetadataAtributeUsage(Description, RevisionData, "RevisionData", 0, 1);

                if (Description.MetadataAttributeUsages.Where(p => p.MetadataAttribute == Details).Count() <= 0)
                    mdpManager.AddMetadataAtributeUsage(Description, Details, "Details", 0, 1);

                if (Description.MetadataAttributeUsages.Where(p => p.MetadataAttribute == Coverage).Count() <= 0)
                    mdpManager.AddMetadataAtributeUsage(Description, Coverage, "Coverage", 0, 1);

                if (Description.MetadataAttributeUsages.Where(p => p.MetadataAttribute == Uri).Count() <= 0)
                    mdpManager.AddMetadataAtributeUsage(Description, Uri, "Uri", 0, 1);
            }
            else
            {
                mdpManager.AddMetadataAtributeUsage(Description, Title, "Title", 1, 2);
                mdpManager.AddMetadataAtributeUsage(Description, RevisionData, "DateModified of the dataset", 0, 1);
                mdpManager.AddMetadataAtributeUsage(Description, Details, "Details of the dataset", 0, 1);
                mdpManager.AddMetadataAtributeUsage(Description, Coverage, "Coverage", 0, 1);
                mdpManager.AddMetadataAtributeUsage(Description, Uri, "Uri", 0, 1);
            }



            #endregion

            #region Owner package

            MetadataAttribute Role = mdaManager.MetadataAttributeRepo.Get(p => p.Name.Equals("Role")).FirstOrDefault();
            if (Role == null)
            {
                DataType dataType = dataTypeManager.Repo.Get(p => p.SystemType.Equals("String")).FirstOrDefault();
                Unit unit = unitManager.Repo.Get(p => p.Name.Equals("None")).FirstOrDefault();

                Role = mdaManager.Create("Role", "Role", "Role", false, false, "David Blaa",
                        MeasurementScale.Categorial, DataContainerType.ValueType, "", dataType, unit, null, null, null, null);
            }

            if (Owner.MetadataAttributeUsages!=null & Owner.MetadataAttributeUsages.Count > 0)
            {
                // add metadataAttributes to packages
                if (Owner.MetadataAttributeUsages.Where(p => p.MetadataAttribute == Name).Count() <= 0)
                    mdpManager.AddMetadataAtributeUsage(Owner, Name, "Full Name", 1, 1);

                if (Owner.MetadataAttributeUsages.Where(p => p.MetadataAttribute == Name).Count() <= 0)
                    mdpManager.AddMetadataAtributeUsage(Owner, Name, "Sorting Name", 0, 1);

                if (Owner.MetadataAttributeUsages.Where(p => p.MetadataAttribute == Name).Count() <= 0)
                    mdpManager.AddMetadataAtributeUsage(Owner, Name, "Organisation Name", 0, 1);

                if (Owner.MetadataAttributeUsages.Where(p => p.MetadataAttribute == Role).Count() <= 0)
                    mdpManager.AddMetadataAtributeUsage(Owner, Role, "Role", 0, 1);

                if (Owner.MetadataAttributeUsages.Where(p => p.MetadataAttribute == Address).Count() <= 0)
                    mdpManager.AddMetadataAtributeUsage(Owner, Address, "Address", 0, 3);

                if (Owner.MetadataAttributeUsages.Where(p => p.MetadataAttribute == Email).Count() <= 0)
                    mdpManager.AddMetadataAtributeUsage(Owner, Email, "Email", 0, 4);

                if (Owner.MetadataAttributeUsages.Where(p => p.MetadataAttribute == Phone).Count() <= 0)
                    mdpManager.AddMetadataAtributeUsage(Owner, Phone, "Phone", 0, 4);

                if (Owner.MetadataAttributeUsages.Where(p => p.MetadataAttribute == Uri).Count() <= 0)
                    mdpManager.AddMetadataAtributeUsage(Owner, Uri, "Uri", 0, 4);

            }
            else
            {
                mdpManager.AddMetadataAtributeUsage(Owner, Name, "Full Name", 1, 1);
                mdpManager.AddMetadataAtributeUsage(Owner, Name, "Sorting Name", 0, 1);
                mdpManager.AddMetadataAtributeUsage(Owner, Name, "Organisation Name", 0, 1);
                mdpManager.AddMetadataAtributeUsage(Owner, Address, "Address", 0, 3);
                mdpManager.AddMetadataAtributeUsage(Owner, Email, "Email", 0, 4);
                mdpManager.AddMetadataAtributeUsage(Owner, Role, "Role", 0, 1);
                mdpManager.AddMetadataAtributeUsage(Owner, Phone, "Phone", 0, 4);
                mdpManager.AddMetadataAtributeUsage(Owner, Uri, "Uri", 0, 4);

            }

            #endregion

            #region Scope package

            MetadataAttribute TaxonomicTerm = mdaManager.MetadataAttributeRepo.Get(p => p.Name.Equals("TaxonomicTerm")).FirstOrDefault();
            if (TaxonomicTerm == null)
            {
                DataType dataType = dataTypeManager.Repo.Get(p => p.SystemType.Equals("String")).FirstOrDefault();
                Unit unit = unitManager.Repo.Get(p => p.Name.Equals("None")).FirstOrDefault();

                TaxonomicTerm = mdaManager.Create("TaxonomicTerm", "TaxonomicTerm", "TaxonomicTerm", false, false, "David Blaa",
                        MeasurementScale.Categorial, DataContainerType.ValueType, "", dataType, unit, null, null, null, null);
            }

            MetadataAttribute GeoEcologicalTerm = mdaManager.MetadataAttributeRepo.Get(p => p.Name.Equals("GeoEcologicalTerm")).FirstOrDefault();
            if (GeoEcologicalTerm == null)
            {
                DataType dataType = dataTypeManager.Repo.Get(p => p.SystemType.Equals("String")).FirstOrDefault();
                Unit unit = unitManager.Repo.Get(p => p.Name.Equals("None")).FirstOrDefault();

                GeoEcologicalTerm = mdaManager.Create("GeoEcologicalTerm", "GeoEcologicalTerm", "GeoEcologicalTerm", false, false, "David Blaa",
                        MeasurementScale.Categorial, DataContainerType.ValueType, "", dataType, unit, null, null, null, null);
            }

            if (Scope.MetadataAttributeUsages!=null & Scope.MetadataAttributeUsages.Count > 0)
            {
                // add metadataAttributes to packages
                if (Scope.MetadataAttributeUsages.Where(p => p.MetadataAttribute == TaxonomicTerm).Count() <= 0)
                    mdpManager.AddMetadataAtributeUsage(Scope, TaxonomicTerm, "TaxonomicTerm", 0, 10);

                if (Scope.MetadataAttributeUsages.Where(p => p.MetadataAttribute == GeoEcologicalTerm).Count() <= 0)
                    mdpManager.AddMetadataAtributeUsage(Scope, GeoEcologicalTerm, "GeoEcologicalTerm", 0, 10);

            }
            else
            {
                mdpManager.AddMetadataAtributeUsage(Scope, TaxonomicTerm, "TaxonomicTerm", 0, 10);
                mdpManager.AddMetadataAtributeUsage(Scope, GeoEcologicalTerm, "GeoEcologicalTerm", 0, 10);

            }


            #endregion


            #region Exploratory stuff

            MetadataStructure exploratory = mdsManager.Repo.Get(p => p.Name == "Exploratory").FirstOrDefault();
            if (exploratory == null) exploratory = mdsManager.Create("Exploratory", "This is the exploratory metadata structure", "", "", root);

            MetadataPackage DataDescription = mdpManager.MetadataPackageRepo.Get(p => p.Name == "DataDescription").FirstOrDefault();
            if (DataDescription == null) DataDescription = mdpManager.Create("DataDescription", "Description of data", true);

            MetadataPackage ResearchObjects = mdpManager.MetadataPackageRepo.Get(p => p.Name == "ResearchObjects").FirstOrDefault();
            if (ResearchObjects == null) ResearchObjects = mdpManager.Create("ResearchObjects", "Information about research objects", true);

            // add package to structure
            if (exploratory.MetadataPackageUsages!=null & exploratory.MetadataPackageUsages.Count > 0)
            {
                if (exploratory.MetadataPackageUsages.Where(p => p.MetadataPackage == DataDescription).Count() <= 0)
                    mdsManager.AddMetadataPackageUsage(exploratory, DataDescription, "Data Description", 0, 1);

                if (exploratory.MetadataPackageUsages.Where(p => p.MetadataPackage == ResearchObjects).Count() <= 0)
                    mdsManager.AddMetadataPackageUsage(exploratory, ResearchObjects, "Research Objects", 0, 1);
            }
            else
            {
                mdsManager.AddMetadataPackageUsage(exploratory, DataDescription, "Data Description", 0, 1);
                mdsManager.AddMetadataPackageUsage(exploratory, ResearchObjects, "Research Objects", 0, 1);
            }

                #region data description

                    MetadataAttribute qualityLevel = mdaManager.MetadataAttributeRepo.Get(p => p.Name.Equals("QualityLevel")).FirstOrDefault();
                    if (qualityLevel == null)
                    {
                        DataType dataType = dataTypeManager.Repo.Get(p => p.SystemType.Equals("String")).FirstOrDefault();
                        Unit unit = unitManager.Repo.Get(p => p.Name.Equals("None")).FirstOrDefault();

                        qualityLevel = mdaManager.Create("QualityLevel", "QualityLevel", "QualityLevel", false, false, "David Blaa",
                                MeasurementScale.Categorial, DataContainerType.ValueType, "", dataType, unit, null, null, null, null);
                    }

                    MetadataAttribute dataStatus = mdaManager.MetadataAttributeRepo.Get(p => p.Name.Equals("DataStatus")).FirstOrDefault();
                    if (dataStatus == null)
                    {
                        DataType dataType = dataTypeManager.Repo.Get(p => p.SystemType.Equals("String")).FirstOrDefault();
                        Unit unit = unitManager.Repo.Get(p => p.Name.Equals("None")).FirstOrDefault();

                        dataStatus = mdaManager.Create("DataStatus", "DataStatus", "DataStatus", false, false, "David Blaa",
                                MeasurementScale.Categorial, DataContainerType.ValueType, "", dataType, unit, null, null, null, null);
                    }

                    if (DataDescription.MetadataAttributeUsages!=null & DataDescription.MetadataAttributeUsages.Count > 0)
                    {
                        // add metadataAttributes to packages
                        if (DataDescription.MetadataAttributeUsages.Where(p => p.MetadataAttribute == qualityLevel).Count() <= 0)
                            mdpManager.AddMetadataAtributeUsage(DataDescription, qualityLevel, "Quality Level", 0, 1);

                        if (DataDescription.MetadataAttributeUsages.Where(p => p.MetadataAttribute == dataStatus).Count() <= 0)
                            mdpManager.AddMetadataAtributeUsage(DataDescription, dataStatus, "Data Status", 0, 1);

                    }
                    else
                    {
                        mdpManager.AddMetadataAtributeUsage(DataDescription, qualityLevel, "Quality Level", 0, 1);
                        mdpManager.AddMetadataAtributeUsage(DataDescription, dataStatus, "Data Status", 0, 1);
                    }

                #endregion


                #region research objects

                    MetadataAttribute processes = mdaManager.MetadataAttributeRepo.Get(p => p.Name.Equals("Processe")).FirstOrDefault();
                    if (processes == null)
                    {
                        DataType dataType = dataTypeManager.Repo.Get(p => p.SystemType.Equals("String")).FirstOrDefault();
                        Unit unit = unitManager.Repo.Get(p => p.Name.Equals("None")).FirstOrDefault();

                        processes = mdaManager.Create("Processe", "Processe", "Processe", false, false, "David Blaa",
                                MeasurementScale.Categorial, DataContainerType.ValueType, "", dataType, unit, null, null, null, null);
                    }

                    MetadataAttribute Services = mdaManager.MetadataAttributeRepo.Get(p => p.Name.Equals("Service")).FirstOrDefault();
                    if (Services == null)
                    {
                        DataType dataType = dataTypeManager.Repo.Get(p => p.SystemType.Equals("String")).FirstOrDefault();
                        Unit unit = unitManager.Repo.Get(p => p.Name.Equals("None")).FirstOrDefault();

                        Services = mdaManager.Create("Service", "Service", "Service", false, false, "David Blaa",
                                MeasurementScale.Categorial, DataContainerType.ValueType, "", dataType, unit, null, null, null, null);
                    }

                    if (ResearchObjects.MetadataAttributeUsages!=null & ResearchObjects.MetadataAttributeUsages.Count > 0)
                    {
                        // add metadataAttributes to packages
                        if (ResearchObjects.MetadataAttributeUsages.Where(p => p.MetadataAttribute == processes).Count() <= 0)
                            mdpManager.AddMetadataAtributeUsage(ResearchObjects, processes, "Processe", 0, 10);

                        if (ResearchObjects.MetadataAttributeUsages.Where(p => p.MetadataAttribute == dataStatus).Count() <= 0)
                            mdpManager.AddMetadataAtributeUsage(ResearchObjects, Services, "Service", 0, 10);

                    }
                    else
                    {
                        mdpManager.AddMetadataAtributeUsage(ResearchObjects, processes, "Processe", 0, 10);
                        mdpManager.AddMetadataAtributeUsage(ResearchObjects, Services, "Service", 0, 10);
                    }
                #endregion
            #endregion


                    return View("CreateMetadataStructure");
        }


        #endregion

    }
}
