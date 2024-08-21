using BExIS.Dcm.UploadWizard;
using BExIS.Dlm.Entities.Administration;
using BExIS.Dlm.Entities.Data;
using BExIS.Dlm.Entities.DataStructure;
using BExIS.Dlm.Services.Administration;
using BExIS.Dlm.Services.Data;
using BExIS.Dlm.Services.DataStructure;
using BExIS.Dlm.Services.MetadataStructure;
using BExIS.Modules.Dcm.UI.Models;
using BExIS.Security.Entities.Authorization;
using BExIS.Security.Services.Authorization;
using BExIS.Security.Services.Objects;
using BExIS.UI.Helpers;
using BExIS.Utils.Data.Upload;
using BExIS.Xml.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using System.Web.Routing;
using System.Xml;
using Vaiona.Persistence.Api;
using Vaiona.Utils.Cfg;
using Vaiona.Web.Extensions;
using Vaiona.Web.Mvc;
using Vaiona.Web.Mvc.Models;

namespace BExIS.Modules.Dcm.UI.Controllers
{
    public class OldSubmitController : BaseController
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
            // if dataset id is set it possible to check the entity
            //get the researchobject (cuurently called dataset) to get the id of a metadata structure
            Dataset researcobject = this.GetUnitOfWork().GetReadOnlyRepository<Dataset>().Get(datasetid);
            string defaultAction = "Upload";
            long entityId = datasetid;
            if (researcobject != null)
            {
                long metadataStrutcureId = researcobject.MetadataStructure.Id;

                using (MetadataStructureManager metadataStructureManager = new MetadataStructureManager())
                {
                    string entityName = xmlDatasetHelper.GetEntityNameFromMetadatStructure(metadataStrutcureId, metadataStructureManager);
                    string entityType = xmlDatasetHelper.GetEntityTypeFromMetadatStructure(metadataStrutcureId, metadataStructureManager);

                    //ToDo in the entity table there must be the information
                    using (EntityManager entityManager = new EntityManager())
                    {
                        var entity = entityManager.Entities.Where(e => e.Name.Equals(entityName)).FirstOrDefault();

                        string moduleId = "";
                        Tuple<string, string, string> action = null;

                        if (entity != null && entity.Extra != null)
                        {
                            var node = entity.Extra.SelectSingleNode("extra/modules/module");

                            if (node != null) moduleId = node.Attributes["value"].Value;

                            string modus = "upload";

                            action = EntityViewerHelper.GetEntityViewAction(entityName, moduleId, modus);
                        }
                        if (action == null) RedirectToAction(defaultAction, new { type, entityId });

                        try
                        {
                            return RedirectToAction(action.Item3, action.Item2, new { area = action.Item1, type, entityId });
                        }
                        catch
                        {
                            return RedirectToAction(defaultAction, new { type, entityId });
                        }
                    }
                }
            }

            return RedirectToAction(defaultAction, new { type, entityId });
        }

        public ActionResult Upload(DataStructureType type, long entityId = 0)
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

                    if (entityId > 0) TaskManager.AddToBus(TaskManager.DATASET_ID, entityId);

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
                SetParametersToTaskmanager(entityId);
            }

            return View("UploadWizard", (TaskManager)Session["TaskManager"]);
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

            long datasetId = (long)TaskManager.Bus[TaskManager.DATASET_ID];
            Session["TaskManager"] = null;

            return ShowData(datasetId);
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

        public ActionResult ShowDashboard()
        {
            return RedirectToAction("Index", "Dashboard", new RouteValueDictionary { { "area", "DDM" } });
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
                List<long> datasetIds = entityPermissionManager.GetKeys(GetUsernameOrDefault(), "Dataset", typeof(Dataset), RightType.Write).Result.ToList();

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

                foreach (var datasStructure in dsm.GetStructuredDataStructuresAsKVP())
                {
                    string title = datasStructure.Value;

                    temp.Add(new ListViewItem(datasStructure.Key, title));
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
            using (ResearchPlanManager rpm = new ResearchPlanManager())
            {
                List<ListViewItem> temp = new List<ListViewItem>();

                foreach (ResearchPlan researchPlan in rpm.Repo.Get())
                {
                    string title = researchPlan.Title;

                    temp.Add(new ListViewItem(researchPlan.Id, title));
                }

                return temp.OrderBy(p => p.Title).ToList();
            }
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
                    using (DatasetManager dm = new DatasetManager())
                    {
                        string title = "";
                        // is checkedIn?
                        if (dm.IsDatasetCheckedIn(datasetid))
                        {
                            var dsv = dm.GetDatasetLatestVersion(datasetid);
                            title = dsv.Title;
                        }

                        TaskManager.AddToBus(TaskManager.DATASET_TITLE, title);
                    }
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