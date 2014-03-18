using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Xml;
using BExIS.Io.Transform.Input;
using BExIS.Io.Transform.Validation.Exceptions;
using BExIS.Dlm.Entities.Administration;
using BExIS.Dlm.Entities.Data;
using BExIS.Dlm.Entities.DataStructure;
using BExIS.Dlm.Services.Administration;
using BExIS.Dlm.Services.Data;
using BExIS.Dlm.Services.DataStructure;
using BExIS.Web.Shell.Areas.DCM.Models;
using BExIS.Dcm.UploadWizard;
using Vaiona.Util.Cfg;
using System.Diagnostics;
using BExIS.Io.Transform.Output;
using BExIS.Xml.Services;
using BExIS.Dlm.Entities.MetadataStructure;
using BExIS.Dlm.Services.MetadataStructure;
using BExIS.Dcm.Wizard;

namespace BExIS.Web.Shell.Areas.DCM.Controllers
{
    public class SubmitController : Controller
    {
        //
        // GET: /Collect/Home/

        
        List<string> ids = new List<string>();
        private TaskManager TaskManager;
        private FileStream Stream;

        public ActionResult Index()
        {
            return View();
        }

        #region Upload Wizard

        public ActionResult UploadWizard()
        {
            Session["TaskManager"] = null;

            if (TaskManager == null) TaskManager = (TaskManager)Session["TaskManager"];

            if (TaskManager == null)
            {
                string path = Path.Combine(AppConfiguration.GetModuleWorkspacePath("DCM"), "SubmitTaskInfo.xml");
                XmlDocument xmlTaskInfo = new XmlDocument();
                xmlTaskInfo.Load(path);

                
                Session["TaskManager"] = TaskManager.Bind(xmlTaskInfo);
                Session["Filestream"] = Stream;

                TaskManager = (TaskManager)Session["TaskManager"];

                // get Lists of Dataset and Datastructure
                Session["DatasetVersionViewList"] = LoadDatasetVersionViewList();
                Session["DataStructureViewList"] = LoadDataStructureViewList();
                Session["ResearchPlanViewList"] = LoadResearchPlanViewList();
                
            }


            return View((TaskManager)Session["TaskManager"]);
        }

     
        #region UploadNavigation

        [HttpPost]
        public ActionResult RefreshNavigation()
        {
            TaskManager = (TaskManager)Session["TaskManager"];

            return PartialView("_uploadWizardNav", TaskManager);
        }

        [HttpPost]
        public ActionResult RefreshTaskList()
        {
            TaskManager = (TaskManager)Session["TaskManager"];

            return PartialView("_taskListView", TaskManager.GetStatusOfStepInfos());
        }

        #endregion

        #region Finish

        [HttpGet]
        public ActionResult FinishUpload()
        {
            TaskManager = (TaskManager)Session["TaskManager"];
            //TaskManager.SetCurrent(null);

            FinishUploadModel finishModel = new FinishUploadModel();
            finishModel.DatasetTitle = TaskManager.Bus[TaskManager.DATASET_TITLE].ToString();
            finishModel.Filename = TaskManager.Bus[TaskManager.FILENAME].ToString();

            Session["TaskManager"] = null;
            string path = Path.Combine(AppConfiguration.GetModuleWorkspacePath("DCM"), "SubmitTaskInfo.xml");
            XmlDocument xmlTaskInfo = new XmlDocument();
            xmlTaskInfo.Load(path);

            Session["TaskManager"] = TaskManager.Bind(xmlTaskInfo);

            return View(finishModel);
        }

        #endregion
        
        #region Navigation options

        public ActionResult CloseUpload()
        {
            Session["TaskManager"] = null;
            TaskManager = null;

            return RedirectToAction("UploadWizard");
        }

        #endregion

        #region Helper functions

            // chekc if user exist
            // if true return usernamem otherwise "DEFAULT"
            public string GetUserNameOrDefault()
            {
                string userName = string.Empty;
                try
                {
                    userName = HttpContext.User.Identity.Name;
                }
                catch { }

                return !string.IsNullOrWhiteSpace(userName) ? userName : "DEFAULT";
            }

            public List<ListViewItem> LoadDatasetVersionViewList()
            {
                DatasetManager dm = new DatasetManager();
                Dictionary<long, XmlDocument> dmtemp = new Dictionary<long, XmlDocument>();
                dmtemp = dm.GetDatasetLatestMetadataVersions();
                List<ListViewItem> temp = new List<ListViewItem>();

                foreach (long datasetid in dmtemp.Keys)
                {
                    if (dmtemp[datasetid] != null)
                    {
                        XmlNodeList xnl = dmtemp[datasetid].SelectNodes("Metadata/Description/Description/Title/Title");
                        string title = "";

                        if (xnl.Count > 0)
                        {
                            title = xnl[0].InnerText;
                        }

                        temp.Add(new ListViewItem(datasetid, title));
                    }
                }

               return temp.OrderBy(p => p.Title).ToList();
            }

            public List<ListViewItem> LoadDataStructureViewList()
            {
                DataStructureManager dsm = new DataStructureManager();
                List<ListViewItem> temp = new List<ListViewItem>();

                foreach (DataStructure datasStructure in dsm.StructuredDataStructureRepo.Get())
                {
                    string title = datasStructure.Name;

                    temp.Add(new ListViewItem(datasStructure.Id, title));
                }



                return temp.OrderBy(p => p.Title).ToList();
            }

            public List<ListViewItem> LoadResearchPlanViewList()
            {
                ResearchPlanManager rpm = new ResearchPlanManager();
                List<ListViewItem> temp = new List<ListViewItem>();

                foreach (ResearchPlan researchPlan in rpm.Repo.Get())
                {
                    string title = researchPlan.Title;

                    temp.Add(new ListViewItem(researchPlan.Id, title));
                }

                return temp.OrderBy(p => p.Title).ToList();
            }

        #endregion

        #endregion

        #region metadata

        public ActionResult XsdRead()
        {
            XsdSchemaReader xsdSchemaReader = new XsdSchemaReader();

            xsdSchemaReader.Read();


            return View("Index");
        }

        public ActionResult Create()
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
            if (contact == null) contact = mdpManager.Create("Contact", "Content Contact", true);

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
            if (root.MetadataPackageUsages.Count > 0)
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

                Email = mdaManager.Create("EMail", "EMail", "EMail adress", false, false, "David Blaa",
                        MeasurementScale.Categorial, DataContainerType.ValueType, "", dataType, unit, null, null, null, null);
            }

            MetadataAttribute Adress = mdaManager.MetadataAttributeRepo.Get(p => p.Name.Equals("Adress")).FirstOrDefault();
            if (Adress == null)
            {
                DataType dataType = dataTypeManager.Repo.Get(p => p.SystemType.Equals("String")).FirstOrDefault();
                Unit unit = unitManager.Repo.Get(p => p.Name.Equals("None")).FirstOrDefault();

                Adress = mdaManager.Create("Adress", "Adress", "Adress", false, false, "David Blaa",
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


            if (contact.MetadataAttributeUsages.Count > 0)
            {
                // add metadataAttributes to packages
                if (contact.MetadataAttributeUsages.Where(p => p.MetadataAttribute == Name).Count() <= 0)
                    mdpManager.AddMetadataAtributeUsage(contact, Name, "Name", 1, 1);

                if (contact.MetadataAttributeUsages.Where(p => p.MetadataAttribute == Email).Count() <= 0)
                    mdpManager.AddMetadataAtributeUsage(contact, Email, "EMail", 0, 1);

                if (contact.MetadataAttributeUsages.Where(p => p.MetadataAttribute == Adress).Count() <= 0)
                    mdpManager.AddMetadataAtributeUsage(contact, Adress, "Adress", 0, 1);

                if (contact.MetadataAttributeUsages.Where(p => p.MetadataAttribute == Phone).Count() <= 0)
                    mdpManager.AddMetadataAtributeUsage(contact, Phone, "Phone", 0, 1);
            }
            else
            {
                mdpManager.AddMetadataAtributeUsage(contact, Name, "Name", 0, 1);
                mdpManager.AddMetadataAtributeUsage(contact, Email, "EMail", 0, 1);
                mdpManager.AddMetadataAtributeUsage(contact, Adress, "Adress", 0, 1);
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
                DataType dataType = dataTypeManager.Repo.Get(p => p.SystemType.Equals("String")).FirstOrDefault();
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

            if (Description.MetadataAttributeUsages.Count > 0)
            {
                // add metadataAttributes to packages
                if (Description.MetadataAttributeUsages.Where(p => p.MetadataAttribute == Title).Count() <= 0)
                    mdpManager.AddMetadataAtributeUsage(Description, Title, "Description in Metadata", 0, 1);

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
                mdpManager.AddMetadataAtributeUsage(Description, Title, "Title in Metadata", 0, 1);
                mdpManager.AddMetadataAtributeUsage(Description, RevisionData, "DateModified in Metadata", 0, 1);
                mdpManager.AddMetadataAtributeUsage(Description, Details, "Details", 0, 1);
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

            if (Owner.MetadataAttributeUsages.Count > 0)
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

                if (Owner.MetadataAttributeUsages.Where(p => p.MetadataAttribute == Adress).Count() <= 0)
                    mdpManager.AddMetadataAtributeUsage(Owner, Adress, "Adress", 0, 3);

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
                mdpManager.AddMetadataAtributeUsage(Owner, Adress, "Adress", 0, 3);
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

            if (Scope.MetadataAttributeUsages.Count > 0)
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


            return View("Index");
        }


        #endregion




    }

    public class UpdateNameModel
    {
        public string Name { get; set; }
        public IEnumerable<int> Numbers { get; set; }
    } 

}
