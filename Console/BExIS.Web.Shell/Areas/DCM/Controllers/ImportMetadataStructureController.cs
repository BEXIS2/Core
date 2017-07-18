using System;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using System.Web.Routing;
using System.Xml;
using BExIS.Dcm.ImportMetadataStructureWizard;
using BExIS.Dlm.Entities.MetadataStructure;
using BExIS.Dlm.Services.MetadataStructure;
using Vaiona.Utils.Cfg;
using Vaiona.Web.Extensions;
using Vaiona.Web.Mvc.Models;

namespace BExIS.Modules.Dcm.UI.Controllers
{
    public class ImportMetadataStructureController : Controller
    {
        private ImportMetadataStructureTaskManager TaskManager;

        //
        // GET: /DCM/ImportMetadataStructure/

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult ImportMetadataStructureWizard()
        {
            ViewBag.Title = PresentationModel.GetViewTitleForTenant("Import Metadata Structure", this.Session.GetTenant()); 

            Session["TaskManager"] = null;
            TaskManager = null;
            if (TaskManager == null) TaskManager = (ImportMetadataStructureTaskManager)Session["TaskManager"];

            if (TaskManager == null)
            {
                try
                {
                    string path = "";
                    path = Path.Combine(AppConfiguration.GetModuleWorkspacePath("DCM"), "ImportMetadataStructureTaskInfo.xml");
                    XmlDocument xmlTaskInfo = new XmlDocument();
                    xmlTaskInfo.Load(path);
                    TaskManager = ImportMetadataStructureTaskManager.Bind(xmlTaskInfo);
                    Session["TaskManager"] = TaskManager;

                }
                catch(Exception e)
                {
                    ModelState.AddModelError(String.Empty, e.Message);
                }
            }

            return View((ImportMetadataStructureTaskManager)Session["TaskManager"]);
        }


        #region Navigation

        [HttpPost]
        public ActionResult RefreshNavigation()
        {
            TaskManager = (ImportMetadataStructureTaskManager)Session["TaskManager"];

            return PartialView("_wizardNav", TaskManager);
        }

        [HttpPost]
        public ActionResult RefreshTaskList()
        {
            TaskManager = (ImportMetadataStructureTaskManager)Session["TaskManager"];

            return PartialView("_taskListView", TaskManager.GetStatusOfStepInfos());
        }

        #endregion

        #region Navigation options

        public ActionResult CancelUpload()
        {
            TaskManager = (ImportMetadataStructureTaskManager)Session["Taskmanager"];

            // delete created metadatastructure
            #region delete mds

            if (TaskManager.Bus.ContainsKey(ImportMetadataStructureTaskManager.SCHEMA_NAME))
            {
                string schemaName = TaskManager.Bus[ImportMetadataStructureTaskManager.SCHEMA_NAME].ToString();
                MetadataStructureManager msm = new MetadataStructureManager();

                if (msm.Repo.Query(m => m.Name.Equals(schemaName)).Any())
                {
                    MetadataStructure ms = msm.Repo.Query(m => m.Name.Equals(schemaName)).FirstOrDefault();
                    msm.Delete(ms);
                }
            }

            #endregion
            Session["Taskmanager"] = null;
            TaskManager = null;



            return RedirectToAction("ImportMetadataStructureWizard", "ImportMetadataStructure", new RouteValueDictionary { { "area", "DCM" } });
        }

        #endregion

        public ActionResult FinishUpload()
        {
            return RedirectToAction("ShowMyDatasetsInFullPage", "Home", new {area = "DDM" });
        }
    }
}
