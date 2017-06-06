using BExIS.Dcm.UploadWizard;
using BExIS.Dlm.Entities.Administration;
using BExIS.Dlm.Entities.DataStructure;
using BExIS.Dlm.Services.Administration;
using BExIS.Dlm.Services.Data;
using BExIS.Dlm.Services.DataStructure;
using BExIS.Security.Entities.Objects;
using BExIS.Security.Services.Authorization;
using BExIS.Security.Services.Subjects;
using BExIS.Web.Shell.Areas.DCM.Models;
using BExIS.Xml.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Xml;
using Vaiona.Utils.Cfg;
using Vaiona.Web.Extensions;
using Vaiona.Web.Mvc.Models;

namespace BExIS.Web.Shell.Areas.DCM.Controllers
{
    public class EasyUploadController : Controller
    {
        List<string> ids = new List<string>();
        private EasyUploadTaskManager TaskManager;

        // GET: DCM/EasyUpload
        public ActionResult Index()
        {
            return RedirectToActionPermanent("UploadWizard");
        }

        public ActionResult UploadWizard()
        {
            ViewBag.Title = PresentationModel.GetViewTitleForTenant("Upload Data", this.Session.GetTenant());

            Session["TaskManager"] = null;
            TaskManager = null;
            
            try
            {
                string path = "";

                path = Path.Combine(AppConfiguration.GetModuleWorkspacePath("DCM"), "EasyUploadTaskInfo.xml");

                XmlDocument xmlTaskInfo = new XmlDocument();
                xmlTaskInfo.Load(path);

                Session["TaskManager"] = EasyUploadTaskManager.Bind(xmlTaskInfo);

                TaskManager = (EasyUploadTaskManager)Session["TaskManager"];
                
            }
            catch (Exception e)
            {
                ModelState.AddModelError(String.Empty, e.Message);
            }

            TaskManager = (EasyUploadTaskManager)Session["TaskManager"];

            return View((EasyUploadTaskManager)Session["TaskManager"]);
        }

        [HttpGet]
        public ActionResult FinishUpload()
        {
            TaskManager = (EasyUploadTaskManager)Session["TaskManager"];
            //TaskManager.SetCurrent(null);


            FinishUploadModel finishModel = new FinishUploadModel();
            if (TaskManager != null)
            {
                finishModel.DatasetTitle = TaskManager.Bus[EasyUploadTaskManager.DATASET_TITLE].ToString();
                finishModel.Filename = TaskManager.Bus[EasyUploadTaskManager.FILENAME].ToString();
            }

            Session["TaskManager"] = null;
            try
            {
                string path = Path.Combine(AppConfiguration.GetModuleWorkspacePath("DCM"), "SubmitTaskInfo.xml");
                XmlDocument xmlTaskInfo = new XmlDocument();
                xmlTaskInfo.Load(path);


                Session["TaskManager"] = EasyUploadTaskManager.Bind(xmlTaskInfo);
            }
            catch (Exception e)
            {
                ModelState.AddModelError(String.Empty, e.Message);
            }


            return ShowData((long)TaskManager.Bus[EasyUploadTaskManager.DATASET_ID]);
        }

        #region UploadNavigation

        [HttpPost]
        public ActionResult RefreshNavigation()
        {
            TaskManager = (EasyUploadTaskManager)Session["TaskManager"];

            return PartialView("_uploadWizardNav", TaskManager);
        }

        [HttpPost]
        public ActionResult RefreshTaskList()
        {
            TaskManager = (EasyUploadTaskManager)Session["TaskManager"];

            return PartialView("_taskListView", TaskManager.GetStatusOfStepInfos());
        }

        #endregion

        #region Navigation options

        public ActionResult CancelUpload()
        {
            TaskManager = (EasyUploadTaskManager)Session["Taskmanager"];

            Session["Taskmanager"] = null;
            TaskManager = null;

            return RedirectToAction("UploadWizard", "EasyUpload", new RouteValueDictionary { { "area", "DCM" }});
        }

        public ActionResult ShowData(long id)
        {
            return RedirectToAction("ShowData", "Data", new RouteValueDictionary { { "area", "DDM" }, { "id", id } });
        }

        #endregion
    }
}