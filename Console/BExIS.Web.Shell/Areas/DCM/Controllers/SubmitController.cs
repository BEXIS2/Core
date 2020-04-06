using BExIS.Dcm.UploadWizard;
using BExIS.Dlm.Entities.Administration;
using BExIS.Dlm.Entities.Data;
using BExIS.Dlm.Entities.DataStructure;
using BExIS.Dlm.Services.Administration;
using BExIS.Dlm.Services.Data;
using BExIS.Dlm.Services.DataStructure;
using BExIS.Modules.Dcm.UI.Models;
using BExIS.Security.Entities.Authorization;
using BExIS.Security.Services.Authorization;
using BExIS.Utils.Data.Upload;
using BExIS.Xml.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using System.Web.Routing;
using System.Xml;
using Vaiona.Utils.Cfg;
using Vaiona.Web.Extensions;
using Vaiona.Web.Mvc;
using Vaiona.Web.Mvc.Models;

namespace BExIS.Modules.Dcm.UI.Controllers
{
    public class SubmitController : BaseController
    {
        //
        // GET: /Collect/Home/

        private List<string> ids = new List<string>();
        private FileStream Stream;
        private TaskManager TaskManager;

        private XmlDatasetHelper xmlDatasetHelper = new XmlDatasetHelper();


        public ActionResult Index()
        {
            ViewBag.Title = PresentationModel.GetViewTitleForTenant("Upload Data", Session.GetTenant());
            return View();
        }

        #region Upload Wizard

        public ActionResult UploadWizard(DataStructureType type, long datasetid = 0)
        {
            ViewBag.Title = PresentationModel.GetViewTitleForTenant("Upload Data", Session.GetTenant());

            Session["TaskManager"] = null;

            if (TaskManager == null) TaskManager = (TaskManager)Session["TaskManager"];

            if (TaskManager == null)
            {
                try
                {
                    string path = "";

                    if (type == DataStructureType.Unstructured)
                        path = Path.Combine(AppConfiguration.GetModuleWorkspacePath("DCM"), "SubmitUnstructuredDataTaskInfo.xml");

                    if (type == DataStructureType.Structured)
                        path = Path.Combine(AppConfiguration.GetModuleWorkspacePath("DCM"), "SubmitTaskInfo.xml");

                    XmlDocument xmlTaskInfo = new XmlDocument();
                    xmlTaskInfo.Load(path);

                    Session["TaskManager"] = TaskManager.Bind(xmlTaskInfo);

                    TaskManager = (TaskManager)Session["TaskManager"];
                    TaskManager.AddToBus(TaskManager.DATASTRUCTURE_TYPE, type);

                    Session["TaskManager"] = TaskManager;
                }
                catch (Exception e)
                {
                    ModelState.AddModelError(String.Empty, e.Message);
                }

                Session["Filestream"] = Stream;

                TaskManager = (TaskManager)Session["TaskManager"];

                // get Lists of Dataset and Datastructure
                Session["DatasetVersionViewList"] = LoadDatasetVersionViewList(type);
                Session["DataStructureViewList"] = LoadDataStructureViewList(type);
                Session["ResearchPlanViewList"] = LoadResearchPlanViewList();

                // setparameters
                SetParametersToTaskmanager(datasetid);
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

        #endregion UploadNavigation

        #region Finish

        [HttpGet]
        public ActionResult FinishUpload()
        {
            TaskManager = (TaskManager)Session["TaskManager"];
            //TaskManager.SetCurrent(null);

            FinishUploadModel finishModel = new FinishUploadModel();
            if (TaskManager != null)
            {
                // add title if exists
                if (TaskManager.Bus.ContainsKey(TaskManager.DATASET_TITLE) && TaskManager.Bus[TaskManager.DATASET_TITLE] != null)
                {
                    finishModel.DatasetTitle = TaskManager.Bus[TaskManager.DATASET_TITLE].ToString();
                }
                finishModel.Filename = TaskManager.Bus[TaskManager.FILENAME].ToString();
            }

            Session["TaskManager"] = null;
            try
            {
                string path = Path.Combine(AppConfiguration.GetModuleWorkspacePath("DCM"), "SubmitTaskInfo.xml");
                XmlDocument xmlTaskInfo = new XmlDocument();
                xmlTaskInfo.Load(path);

                Session["TaskManager"] = TaskManager.Bind(xmlTaskInfo);
            }
            catch (Exception e)
            {
                ModelState.AddModelError(String.Empty, e.Message);
            }

            return ShowData((long)TaskManager.Bus[TaskManager.DATASET_ID]);
        }

        #endregion Finish

        #region Navigation options

        public ActionResult CancelUpload()
        {
            TaskManager = (TaskManager)Session["Taskmanager"];

            DataStructureType type = new DataStructureType();

            if (TaskManager.Bus.ContainsKey(TaskManager.DATASTRUCTURE_TYPE))
            {
                type = (DataStructureType)TaskManager.Bus[TaskManager.DATASTRUCTURE_TYPE];
            }

            Session["Taskmanager"] = null;
            TaskManager = null;

            return RedirectToAction("UploadWizard", "Submit", new RouteValueDictionary { { "area", "DCM" }, { "type", type } });
        }

        public ActionResult ShowData(long id)
        {
            return RedirectToAction("ShowData", "Data", new RouteValueDictionary { { "area", "DDM" }, { "id", id } });
        }

        #endregion Navigation options

        #region Helper functions

        // chekc if user exist
        // if true return usernamem otherwise "DEFAULT"
        public string GetUsernameOrDefault()
        {
            string username = string.Empty;
            try
            {
                username = HttpContext.User.Identity.Name;
            }
            catch { }

            return !string.IsNullOrWhiteSpace(username) ? username : "DEFAULT";
        }

        public List<ListViewItem> LoadDatasetVersionViewList(DataStructureType dataStructureType)
        {
            EntityPermissionManager entityPermissionManager = new EntityPermissionManager();
            DataStructureManager dataStructureManager = new DataStructureManager();
            DatasetManager dm = new DatasetManager();

            try
            {
                List<long> datasetIds = entityPermissionManager.GetKeys(GetUsernameOrDefault(), "Dataset", typeof(Dataset), RightType.Write).ToList();

                List<ListViewItem> tempStructured = new List<ListViewItem>();
                List<ListViewItem> tempUnStructured = new List<ListViewItem>();

                var DatasetVersions = dm.GetDatasetLatestVersions(datasetIds, false);

                foreach (var dsv in DatasetVersions)
                {
                    if (dsv.Dataset.DataStructure.Self.GetType().Equals(typeof(StructuredDataStructure)))
                    {
                        tempStructured.Add(new ListViewItem(dsv.Dataset.Id, dsv.Title));
                    }
                    else
                    {
                        tempUnStructured.Add(new ListViewItem(dsv.Dataset.Id, dsv.Title));
                    }
                }

                if (dataStructureType.Equals(DataStructureType.Structured))
                {
                    return tempStructured.OrderBy(p => p.Title).ToList();
                }
                else
                {
                    return tempUnStructured.OrderBy(p => p.Title).ToList();
                }
  
            }
            finally
            {
                entityPermissionManager.Dispose();
                dataStructureManager.Dispose();
                dm.Dispose();
            }
        }

        public List<ListViewItem> LoadDataStructureViewList(DataStructureType dataStructureType)
        {
            DataStructureManager dsm = new DataStructureManager();

            try
            {
                List<ListViewItem> temp = new List<ListViewItem>();

                foreach (DataStructure datasStructure in dsm.StructuredDataStructureRepo.Get())
                {
                    string title = datasStructure.Name;

                    temp.Add(new ListViewItem(datasStructure.Id, title));
                }

                return temp.OrderBy(p => p.Title).ToList();
            }
            finally
            {
                dsm.Dispose();
            }
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

        private void SetParametersToTaskmanager(long datasetId)
        {
            if (TaskManager == null)
            {
                TaskManager = (TaskManager)Session["TaskManager"];
            }

            #region set dataset id & dataset title

            if (datasetId > 0)
            {
                try
                {
                    long datasetid = Convert.ToInt64(datasetId);
                    TaskManager.AddToBus(TaskManager.DATASET_ID, datasetid);

                    // get title
                    DatasetManager dm = new DatasetManager();
                    string title = "";
                    // is checkedIn?
                    if (dm.IsDatasetCheckedIn(datasetid))
                    {
                        var dsv = dm.GetDatasetLatestVersion(datasetid);
                        title = dsv.Title;
                    }

                    TaskManager.AddToBus(TaskManager.DATASET_TITLE, title);
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }

            #endregion set dataset id & dataset title
        }

        #endregion Helper functions

        #endregion Upload Wizard
    }

    public class UpdateNameModel
    {
        public string Name { get; set; }
        public IEnumerable<int> Numbers { get; set; }
    }
}