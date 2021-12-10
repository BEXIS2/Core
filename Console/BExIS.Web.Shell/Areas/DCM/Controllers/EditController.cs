using BExIS.App.Bootstrap.Attributes;
using BExIS.Dlm.Entities.Data;
using BExIS.Dlm.Services.Data;
using BExIS.Modules.Dcm.UI.Models.Edit;
using BExIS.Security.Entities.Authorization;
using BExIS.UI.Hooks;
using BExIS.UI.Hooks.Caches;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Vaiona.Utils.Cfg;

namespace BExIS.Modules.Dcm.UI.Controllers
{
    public class EditController : Controller
    {
        // GET: Edit
        [BExISEntityAuthorize(typeof(Dataset), "id", RightType.Write)]
        public ActionResult Index(long id, int version = 0)
        {
            ViewBag.id = id;
            ViewBag.version = version;

            return View();
        }

        /// <summary>
        /// load the edit model of a dataset based on the id and the version number
        /// if version = 0 then it loads the latest version
        /// </summary>
        /// <param name="id">identifier of the dataset</param>
        /// <param name="version">version number of the dataset</param>
        /// <returns></returns>
        [BExISEntityAuthorize(typeof(Dataset), "id", RightType.Write)]
        [JsonNetFilter]
        public JsonResult Load(long id, int version = 0)
        {
            EditModel model = new EditModel();
            model.Id = id;
            model.Version = version;

            using (var datasetManager = new DatasetManager())
            {
                // load dataset version
                // if version number = 0 load latest version
                DatasetVersion datasetVersion = null;
                if (version == 0) // get latest
                {
                    datasetVersion = datasetManager.GetDatasetLatestVersion(id);
                    model.Version = datasetManager.GetDatasetVersionCount(id); // get number of the latest version
                }
                else // get specific
                {
                    model.VersionId = datasetManager.GetDatasetVersionId(id, version); // get version id
                    datasetVersion = datasetManager.GetDatasetVersion(model.VersionId); // load datasetversion by id
                }

                // get title
                model.Title = datasetVersion.Title;

                // load all hooks for the edit view
                HookManager hooksManager = new HookManager();
                model.Hooks = hooksManager.GetHooksFor("dataset", "details", HookMode.edit);
                model.Views = hooksManager.GetViewsFor("dataset", "details", HookMode.edit);

                // run all checks
                string userName = "";
                if (HttpContext.User.Identity.IsAuthenticated)
                    userName = HttpContext.User.Identity.Name;

                model.Hooks.ForEach(h => h.Check(id, userName));

                return Json(model, JsonRequestBehavior.AllowGet);
            }
        }

        #region ResultMessages

        /// <summary>
        /// load the patial view of the resultmessages for svelte
        /// </summary>
        /// <param name="id"></param>
        /// <param name="version"></param>
        /// <returns></returns>
        [BExISEntityAuthorize(typeof(Dataset), "id", RightType.Write)]
        public ActionResult StartResultMessages(long id, int version = 0)
        {
            ViewBag.id = id;
            ViewBag.version = version;

            return PartialView("_resultMessages");
        }

        /// <summary>
        /// load the needed svelte builded script for the resultmessages view
        /// </summary>
        /// <returns></returns>
        [BExISEntityAuthorize(typeof(Dataset), "id", RightType.Write)]
        public ActionResult LoadViewScript(long id, int version = 0)
        {
            string filepath = Path.Combine(AppConfiguration.AppRoot, "Areas/DCM/Scripts/svelte/messages.js");
            return File(filepath, "application/javascript");
        }

        /// <summary>
        /// load ResultMessages from the cuirrent EditDatasetDetailsCache
        /// </summary>
        /// <param name="id"></param>
        /// <param name="version"></param>
        /// <returns></returns>
        [BExISEntityAuthorize(typeof(Dataset), "id", RightType.Write)]
        [JsonNetFilter]
        public JsonResult LoadResultMessages(long id, int version = 0)
        {
            List<ResultMessage> messages = new List<ResultMessage>();

            var hookManager = new HookManager();
            var cache = hookManager.LoadCache<EditDatasetDetailsCache>("dataset", "details", HookMode.edit, id);

            if (cache != null && cache.Messages.Any())
            {
                messages = cache.Messages.ToList();
            }

            return Json(messages, JsonRequestBehavior.AllowGet);
        }

        #endregion ResultMessages
    }
}