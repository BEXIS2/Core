using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using BExIS.Dcm.Wizard;
using BExIS.Dcm.CreateDatasetWizard;
using BExIS.Web.Shell.Areas.DCM.Models;
using BExIS.Dlm.Services.DataStructure;
using BExIS.Dlm.Services.Administration;
using BExIS.Dlm.Entities.Administration;
using BExIS.Dlm.Services.MetadataStructure;
using BExIS.Dlm.Entities.MetadataStructure;
using BExIS.Dlm.Entities.DataStructure;
using System.Web.Routing;
using System.Xml;
using System.IO;
using Vaiona.Util.Cfg;
using BExIS.Web.Shell.Areas.DCM.Models.Create;
using BExIS.Web.Shell.Areas.DCM.Models.Metadata;
using BExIS.Dlm.Entities.Data;
using System.Xml.Linq;
using BExIS.Xml.Services;
using BExIS.Dlm.Services.Data;


namespace BExIS.Web.Shell.Areas.DCM.Controllers
{
    public class CreateController : Controller
    {

        private CreateDatasetTaskmanager TaskManager;
       

        //
        // GET: /DCM/Create/

        public ActionResult Index()
        {
            //DatasetManager dm = new DatasetManager();
            //dm.GetDatasetVersion(1);

            //dm.VariableValueRepo.Get();

            //DataStructureManager dsm = new DataStructureManager();
            //dsm.StructuredDataStructureRepo.Get();

            //UnitManager um = new UnitManager();
            //IList<Unit> u = um.Repo.Get();

            //foreach (Unit unit in u)
            //{
            //    DataType d = unit.AssociatedDataTypes.First();
            //    d.DataContainers.First();
            //}
            
            //Session["MetadatStructureList"] = LoadMetadataStructureViewList();

            

            return View();
        }

        public ActionResult CreateDataset()
        {

            Session["CreateDatasetTaskmanager"] = null;
            if (TaskManager == null) TaskManager = (CreateDatasetTaskmanager)Session["CreateDatasetTaskmanager"];

            if (TaskManager == null)
            {
                try
                {
                    string path = Path.Combine(AppConfiguration.GetModuleWorkspacePath("DCM"), "CreateTaskInfo.xml");
                    XmlDocument xmlTaskInfo = new XmlDocument();
                    xmlTaskInfo.Load(path);
                    TaskManager = CreateDatasetTaskmanager.Bind(xmlTaskInfo);
                }
                catch (Exception e)
                {
                    ModelState.AddModelError(String.Empty, e.Message);
                }


                Session["CreateDatasetTaskmanager"] = TaskManager;
                Session["MetadataStructureViewList"] = LoadMetadataStructureViewList();
                Session["ResearchPlanViewList"] = LoadResearchPlanViewList();
                Session["DataStructureViewList"] = LoadDataStructureViewList();
            }

            
        
            return View((CreateDatasetTaskmanager)Session["CreateDatasetTaskmanager"]);
        }

        #region Navigation

        [HttpPost]
        public ActionResult RefreshNavigation()
        {
            TaskManager = (CreateDatasetTaskmanager)Session["CreateDatasetTaskmanager"];

            return PartialView("_createWizardNav", TaskManager);
        }

        [HttpPost]
        public ActionResult RefreshTaskList()
        {
            TaskManager = (CreateDatasetTaskmanager)Session["CreateDatasetTaskmanager"];

            return PartialView("_taskListView", TaskManager);
        }

        public ActionResult Jump(string title)
        {

            return View();
        }

        public ActionResult CloseUpload()
        {
            Session["CreateDatasetTaskmanager"] = null;
            TaskManager = null;

            return RedirectToAction("CreateDataset");
        }

        public ActionResult Close()
        {
            Session["CreateDatasetTaskmanager"] = null;
            TaskManager = null;

            return View("FinishCreation");
        }

        public ActionResult StartUploadWizard()
        {
            TaskManager = (CreateDatasetTaskmanager)Session["CreateDatasetTaskmanager"];

            DataStructureType type = new DataStructureType();

            if (TaskManager.Bus.ContainsKey(CreateDatasetTaskmanager.DATASTRUCTURE_TYPE))
            {
                type = (DataStructureType)TaskManager.Bus[CreateDatasetTaskmanager.DATASTRUCTURE_TYPE];
            }

            
            Session["CreateDatasetTaskmanager"] = null;
            TaskManager = null;

            return RedirectToAction("UploadWizard", "Submit", new RouteValueDictionary { {"area","DCM"} , {"type",type}});
        }


        #endregion

        #region helper

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

        public List<ListViewItem> LoadDataStructureViewList()
        {
            DataStructureManager dsm = new DataStructureManager();
            List<ListViewItem> temp = new List<ListViewItem>();
            string title = "";
            foreach (DataStructure datasStructure in dsm.StructuredDataStructureRepo.Get())
            {
                title = datasStructure.Name;
                temp.Add(new ListViewItem(datasStructure.Id, title));
            }

            foreach (DataStructure datasStructure in dsm.UnStructuredDataStructureRepo.Get())
            {
                title = datasStructure.Name;
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

        public List<ListViewItem> LoadMetadataStructureViewList()
        {
            MetadataStructureManager msm = new MetadataStructureManager();
            List<ListViewItem> temp = new List<ListViewItem>();

            foreach (MetadataStructure metadataStructure in msm.Repo.Get())
            {
                string title = metadataStructure.Name;

                temp.Add(new ListViewItem(metadataStructure.Id, title));
            }

            return temp.OrderBy(p => p.Title).ToList();
        }

        

        #endregion

    }
}
