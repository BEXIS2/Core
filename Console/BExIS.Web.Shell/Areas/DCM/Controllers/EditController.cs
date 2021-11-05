using BExIS.App.Bootstrap.Attributes;
using BExIS.Dlm.Entities.Data;
using BExIS.Dlm.Services.Data;
using BExIS.Modules.Dcm.UI.Models.Edit;
using BExIS.Security.Entities.Authorization;
using BExIS.UI.Hooks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace BExIS.Modules.Dcm.UI.Controllers
{
    public class EditController : Controller
    {
        // GET: Edit
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
                    datasetVersion = datasetManager.GetDatasetLatestVersion(id);
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

                // run all checks
                string userName = "";
                if (HttpContext.User.Identity.IsAuthenticated)
                    userName = HttpContext.User.Identity.Name;

                model.Hooks.ForEach(h => h.Check(id, userName));

                return Json(model, JsonRequestBehavior.AllowGet);
            }
        }
    }
}