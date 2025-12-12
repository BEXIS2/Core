using BExIS.App.Bootstrap.Attributes;
using BExIS.Modules.Smm.UI.Models;
using BExIS.UI.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace BExIS.Modules.Smm.UI.Controllers
{
    public class SpeciesController : Controller
    {
        // GET: Species

        public ActionResult Index()
        {
            string module = "SMM";

            ViewData["app"] = SvelteHelper.GetApp(module);
            ViewData["start"] = SvelteHelper.GetStart(module);

            return View();
        }

        [JsonNetFilter]
        public JsonResult Load()
        {
            SpeciesModel model = new SpeciesModel();
            model.Count = 2021;
            model.Name = "David";


            return Json(model, JsonRequestBehavior.AllowGet);
        }



    }
}