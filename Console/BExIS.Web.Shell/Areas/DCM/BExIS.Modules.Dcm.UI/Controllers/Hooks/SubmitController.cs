using BExIS.App.Bootstrap.Attributes;
using BExIS.App.Bootstrap.Helpers;
using BExIS.Dlm.Entities.Data;
using BExIS.Modules.Dcm.UI.Helpers;
using BExIS.Modules.Dcm.UI.Models.Edit;
using BExIS.Security.Entities.Authorization;
using BExIS.Security.Entities.Subjects;
using BExIS.Security.Services.Utilities;
using BExIS.UI.Hooks;
using BExIS.UI.Hooks.Caches;
using BExIS.UI.Hooks.Logs;
using BExIS.Utils.Config;
using BExIS.Utils.Upload;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Web.Routing;
using Vaiona.Entities.Common;
using Vaiona.Web.Mvc;
using Vaiona.Web.Mvc.Modularity;

namespace BExIS.Modules.Dcm.UI.Controllers
{
    public class SubmitController : BaseController
    {
        private User _user;
        private FileStream Stream;

        private static IDictionary<Guid, int> tasks = new Dictionary<Guid, int>();

        private UploadHelper uploadWizardHelper = new UploadHelper();

        // GET: Validation
        public ActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// validation view need to load the svelte builded script
        /// </summary>
        /// <returns></returns>
        [BExISEntityAuthorize(typeof(Dataset), "id", RightType.Write)]
        [JsonNetFilter]
        public ActionResult Start(long id, int version = 0)
        {
            return RedirectToAction("Load", new { id, version });
        }

        [BExISEntityAuthorize(typeof(Dataset), "id", RightType.Write)]
        [JsonNetFilter]
        public JsonResult Load(long id, int version = 0)
        {
            if (id <= 0) throw new ArgumentException(nameof(id), "id should be greater than 0");

            SubmitHelper helper = new SubmitHelper();
            SubmitModel model = helper.GetModel(id);

            return Json(model, JsonRequestBehavior.AllowGet);
        }

        [BExISEntityAuthorize(typeof(Dataset), "id", RightType.Write)]
        [JsonNetFilter]
        public JsonResult Submit(long id, int version = 0)
        {
            if (id <= 0) throw new ArgumentException(nameof(id), "id should be greater then 0");

            SubmitHelper helper = new SubmitHelper();
            SubmitModel model = helper.GetModel(id);
            SubmitResponce responce = new SubmitResponce();

            HookManager hookManager = new HookManager();
            // load cache to get informations about the current upload workflow
            EditDatasetDetailsCache cache = hookManager.LoadCache<EditDatasetDetailsCache>("dataset", "details", HookMode.edit, id);
            EditDatasetDetailsLog log = hookManager.LoadLog<EditDatasetDetailsLog>("dataset", "details", HookMode.edit, id);

            DataASyncUploadHelper asyncUploadHelper = new DataASyncUploadHelper(cache, log, "dataset");
            asyncUploadHelper.User = BExISAuthorizeHelper.GetUserFromAuthorization(HttpContext);
            asyncUploadHelper.RunningASync = true;

            if (asyncUploadHelper.RunningASync && model.HasStructrue) //async
            {
                Task.Run(() => asyncUploadHelper.FinishUpload(id, AuditActionType.Edit, model.StructureId));

                // send email after starting the upload
                var es = new EmailService();
                var user = asyncUploadHelper.User;

                es.Send(MessageHelper.GetASyncStartUploadHeader(id, model.Title),
                    MessageHelper.GetASyncStartUploadMessage(id, model.Title, model.Files.Select(f => f.Name)),
                    new List<string>() { user.Email }, null,
                    new List<string>() { GeneralSettings.SystemEmail }
                    );

                responce.Success = true;
                responce.AsyncUpload = true;
                responce.AsyncUploadMessage = "All upload information has been entered and the upload will start now. After completion an email will be sent.";
            }
            else
            {
                var errors = asyncUploadHelper.FinishUpload(id, AuditActionType.Edit, model.StructureId).Result;
                responce.Errors = EditHelper.SortFileErrors(errors);
                responce.Success = responce.Errors.Any() ? false : true;


            }

            // load settings
            var moduleSettings = ModuleManager.GetModuleSettings("Ddm");
            bool use_tags = (bool)moduleSettings.GetValueByKey("use_tags");

            // Reindexes a single item in the search index if the "DDM" module is accessible and the "use_tags" setting is false.
            if (this.IsAccessible("DDM", "SearchIndex", "ReIndexSingle") && !use_tags)
            {
                var x = this.Run("DDM", "SearchIndex", "ReIndexSingle", new RouteValueDictionary() { { "id", id } });
            }


            return Json(responce, JsonRequestBehavior.AllowGet);
        }
    }
}