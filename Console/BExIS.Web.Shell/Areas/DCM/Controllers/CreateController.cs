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
            Session["MetadatStructureList"] = LoadMetadataStructureViewList();

            return View();
        }

        public ActionResult CreateDataset()
        {
            Session["CreateDatasetTaskmanager"] = null;
            if (TaskManager == null) TaskManager = (CreateDatasetTaskmanager)Session["CreateDatasetTaskmanager"];

            if (TaskManager == null)
            {
                string path = Path.Combine(AppConfiguration.GetModuleWorkspacePath("DCM"), "CreateTaskInfo.xml");
                XmlDocument xmlTaskInfo = new XmlDocument();
                xmlTaskInfo.Load(path);
                TaskManager = CreateDatasetTaskmanager.Bind(xmlTaskInfo);
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

        public ActionResult Submit()
        {
            #region create dataset

            TaskManager = (CreateDatasetTaskmanager)Session["CreateDatasetTaskmanager"];

            if (TaskManager.Bus.ContainsKey(CreateDatasetTaskmanager.DATASTRUCTURE_ID)
                && TaskManager.Bus.ContainsKey(CreateDatasetTaskmanager.RESEARCHPLAN_ID)
                && TaskManager.Bus.ContainsKey(CreateDatasetTaskmanager.METADATASTRUCTURE_ID))
            {
                Dataset ds;
                DatasetManager dm = new DatasetManager();
                long datasetId = 0;
                // for e new dataset
                if (!TaskManager.Bus.ContainsKey(CreateDatasetTaskmanager.DATASET_ID))
                {
                    long datastructureId = Convert.ToInt64(TaskManager.Bus[CreateDatasetTaskmanager.DATASTRUCTURE_ID]);
                    long researchPlanId = Convert.ToInt64(TaskManager.Bus[CreateDatasetTaskmanager.RESEARCHPLAN_ID]);
                    long metadataStructureId = Convert.ToInt64(TaskManager.Bus[CreateDatasetTaskmanager.METADATASTRUCTURE_ID]);

                    DataStructureManager dsm = new DataStructureManager();
                    DataStructure dataStructure = dsm.StructuredDataStructureRepo.Get(datastructureId);

                    ResearchPlanManager rpm = new ResearchPlanManager();
                    ResearchPlan rp = rpm.Repo.Get(researchPlanId);

                    MetadataStructureManager msm = new MetadataStructureManager();
                    MetadataStructure metadataStructure = msm.Repo.Get(metadataStructureId);

                    ds = dm.CreateEmptyDataset(dataStructure, rp, metadataStructure);
                    datasetId = ds.Id;
                }
                else
                {
                    datasetId = Convert.ToInt64(TaskManager.Bus[CreateDatasetTaskmanager.DATASET_ID]);
                }

                TaskManager = (CreateDatasetTaskmanager)Session["CreateDatasetTaskmanager"];





                if (dm.IsDatasetCheckedOutFor(datasetId, GetUserNameOrDefault()) || dm.CheckOutDataset(datasetId, GetUserNameOrDefault()))
                {
                    DatasetVersion workingCopy = dm.GetDatasetWorkingCopy(datasetId);

                    if (TaskManager.Bus.ContainsKey(CreateDatasetTaskmanager.METADATA_XML))
                    {
                        XDocument xMetadata = (XDocument)TaskManager.Bus[CreateDatasetTaskmanager.METADATA_XML];
                        workingCopy.Metadata = XmlMetadataWriter.ToXmlDocument(xMetadata);
                    }

                    TaskManager.AddToBus(CreateDatasetTaskmanager.DATASET_ID, datasetId);

                    dm.EditDatasetVersion(workingCopy, null, null, null);

                    dm.CheckInDataset(datasetId, "Metadata was submited.", GetUserNameOrDefault());
                }
            }

            #endregion

            return View("FinishCreation");
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
