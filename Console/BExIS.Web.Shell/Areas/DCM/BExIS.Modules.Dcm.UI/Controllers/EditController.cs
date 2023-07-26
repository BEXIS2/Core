using BExIS.App.Bootstrap.Attributes;
using BExIS.App.Bootstrap.Helpers;
using BExIS.Dlm.Entities.Data;
using BExIS.Dlm.Services.Data;
using BExIS.Modules.Dcm.UI.Models.Edit;
using BExIS.Security.Entities.Authorization;
using BExIS.UI.Helpers;
using BExIS.UI.Hooks;
using System.Collections.Generic;
using System.Web.Mvc;

namespace BExIS.Modules.Dcm.UI.Controllers
{
    public class EditController : Controller
    {
        // GET: Edit
        [BExISEntityAuthorize(typeof(Dataset), "id", RightType.Write)]
        public ActionResult Index(long id, int version = 0)
        {
            string module = "DCM";

            ViewData["id"] = id;
            ViewData["version"] = version;
            ViewData["app"] = SvelteHelper.GetApp(module);
            ViewData["start"] = SvelteHelper.GetStart(module);

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
                string userName = BExISAuthorizeHelper.GetAuthorizedUserName(HttpContext);

                model.Hooks.ForEach(h => h.Check(id, userName));

                // add informations disbaled hooks from the enity template
                // based on the entity template, hooks can be disabled.
                foreach (var hook in model.Hooks)
                {
                    if (datasetVersion.Dataset.EntityTemplate.DisabledHooks != null && datasetVersion.Dataset.EntityTemplate.DisabledHooks.Contains(hook.DisplayName))
                        hook.Status = HookStatus.Disabled;
                }

                return Json(model, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// return hooks with new status, check was running again 
        /// </summary>
        /// <param name="id">subject id</param>
        /// <param name="versionId">specific subject version based on version id</param>
        /// <returns></returns>
        [BExISEntityAuthorize(typeof(Dataset), "id", RightType.Write)]
        [JsonNetFilter]
        public JsonResult Hooks(long id, int versionId = 0)
        {
            List<Hook> Hooks = new List<Hook>();

            using (var datasetManager = new DatasetManager())
            {
                // load dataset version
                // if version number = 0 load latest version
                DatasetVersion datasetVersion = null;
                if (versionId == 0) // get latest
                {
                    datasetVersion = datasetManager.GetDatasetLatestVersion(id);
                }
                else // get specific
                {
                  datasetVersion = datasetManager.GetDatasetVersion(versionId); // load datasetversion by id
                }

                // load all hooks for the edit view
                HookManager hooksManager = new HookManager();
                Hooks = hooksManager.GetHooksFor("dataset", "details", HookMode.edit);
           

                // run all checks
                string userName = BExISAuthorizeHelper.GetAuthorizedUserName(HttpContext);

                Hooks.ForEach(h => h.Check(id, userName));

                // add informations disbaled hooks from the enity template
                // based on the entity template, hooks can be disabled.
                foreach (var hook in Hooks)
                {
                    if (datasetVersion.Dataset.EntityTemplate.DisabledHooks != null && datasetVersion.Dataset.EntityTemplate.DisabledHooks.Contains(hook.DisplayName))
                        hook.Status = HookStatus.Disabled;
                }

                return Json(Hooks, JsonRequestBehavior.AllowGet);
            }
        }
    }
}