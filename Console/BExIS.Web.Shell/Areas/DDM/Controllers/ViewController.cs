using BExIS.App.Bootstrap.Attributes;
using BExIS.Dlm.Entities.Data;
using BExIS.Security.Entities.Authorization;
using BExIS.UI.Hooks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace BExIS.Modules.Ddm.UI.Controllers
{
    public class ViewController : Controller
    {
        // GET: View
        public ActionResult Index()
        {
            HookManager hooksManager = new HookManager();
            var hooks = hooksManager.GetHooksFor("dataset", "details", HookMode.view);

            return View(hooks);
        }

        [BExISEntityAuthorize(typeof(Dataset), "id", RightType.Read)]
        public ActionResult Start(long id)
        {
            return RedirectToAction("LoadMetadata", "Form", new {area="DCM", entityId = id, locked = true, created = false, fromEditMode = true });
        }
    }
}