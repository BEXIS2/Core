using BExIS.UI.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace BExIS.Web.Shell.Controllers
{
    public class DocsController : Controller
    {
        // GET: Docs
        public ActionResult Index(string id)
        {
            string module = "Shell";

            ViewData["app"] = SvelteHelper.GetApp(module);
            ViewData["start"] = SvelteHelper.GetStart(module);

            return View();
        }

    }
}