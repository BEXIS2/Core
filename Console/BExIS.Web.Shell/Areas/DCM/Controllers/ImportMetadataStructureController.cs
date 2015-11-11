using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Xml;
using BExIS.Dcm.ImportMetadataStructureWizard;
using Vaiona.Utils.Cfg;
using Vaiona.Web.Mvc.Models;

namespace BExIS.Web.Shell.Areas.DCM.Controllers
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
            ViewBag.Title = PresentationModel.GetViewTitle("Import Metadata Structure ");

            Session["TaskManager"] = null;
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


            Session["Taskmanager"] = null;
            TaskManager = null;

            return RedirectToAction("ImportMetadataStructureWizard", "ImportMetadataStructure", new RouteValueDictionary { { "area", "DCM" } });
        }

        #endregion

    }
}
