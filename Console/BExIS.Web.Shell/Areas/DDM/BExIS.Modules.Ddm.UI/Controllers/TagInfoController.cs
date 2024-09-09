using BExIS.UI.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace BExIS.Modules.Ddm.UI.Controllers
{
    public class TagInfoController : Controller
    {
        // GET: TagInfo
        public ActionResult Index(long id)
        {
            string module = "DDM";

            ViewData["app"] = SvelteHelper.GetApp(module);
            ViewData["start"] = SvelteHelper.GetStart(module);
            ViewData["id"] = id;

            return View();
        }

    }
}