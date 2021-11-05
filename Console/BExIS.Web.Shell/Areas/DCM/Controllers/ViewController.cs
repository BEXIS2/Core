using BExIS.App.Bootstrap.Attributes;
using BExIS.Dlm.Entities.Data;
using BExIS.Security.Entities.Authorization;
using BExIS.UI.Hooks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace BExIS.Modules.Dcm.UI.Controllers
{
    public class ViewController : Controller
    {
        // GET: View
        /// <summary>
        /// this action loads the main view page of the dataset.
        /// here all available hooks are loaded and checked and forwarded to ui.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="version"></param>
        /// <returns></returns>
        public ActionResult Index(long id, int version = 0)
        {
            ViewBag.id = id;
            ViewBag.version = version;

            HookManager hooksManager = new HookManager();
            var hooks = hooksManager.GetHooksFor("dataset", "details", HookMode.view);

            // get user and run check
            string username = "";
            username = HttpContext.User?.Identity?.Name;
     

            foreach (var h in hooks)
            {
                h.Check(id, username);
            }


            return View(hooks);
        }

        /// <summary>
        /// Start from Metadata Hook - view
        /// </summary>
        /// <param name="id"></param>
        /// <param name="version"></param>
        /// <returns></returns>
        [BExISEntityAuthorize(typeof(Dataset), "id", RightType.Read)]
        public ActionResult Start(long id, int version)
        {
            throw new NotImplementedException();

            //return RedirectToAction("LoadMetadata", "Form", new {area="DCM", entityId = id, locked = true, created = false, fromEditMode = true });
        }

        /// <summary>
        /// Start from Data Hook - view
        /// </summary>
        /// <param name="id"></param>
        /// <param name="version"></param>
        /// <returns></returns>
        [BExISEntityAuthorize(typeof(Dataset), "id", RightType.Read)]
        public ActionResult StartData(long id, int version)
        {
            throw new NotImplementedException();

            //return RedirectToAction("ShowPrimaryData", "Data", new { area = "DDM", datasetID = id, versionId = version});
        }
    }
}