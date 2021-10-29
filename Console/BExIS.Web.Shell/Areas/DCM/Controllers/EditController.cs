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
        public ActionResult Index()
        {
            HookManager hooksManager = new HookManager();
            var hooks = hooksManager.GetHooksFor("dataset", "details", HookMode.edit);

            return View(hooks);
        }
    }
}