using BExIS.App.Bootstrap.Attributes;
using BExIS.Dlm.Services.DataStructure;
using BExIS.UI.Helpers;
using System;
using System.Collections.Generic;
using System.Web.Mvc;
using BExIS.Modules.Rpm.UI.Models.Dimensions;
using BExIS.Dlm.Entities.DataStructure;
using System.Linq;
using BExIS.Modules.Rpm.UI.Models.Units;
using BExIS.Modules.Rpm.UI.Models;
using System.Runtime.CompilerServices;

namespace BExIS.Modules.Rpm.UI.Controllers
{
    public class ConstraintController : Controller
    {
        public ActionResult Index()
        {
            string module = "RPM";

            ViewData["app"] = SvelteHelper.GetApp(module);
            ViewData["start"] = SvelteHelper.GetStart(module);

            return View();
        }

        [JsonNetFilter]
        [HttpGet]
        public JsonResult GetConstraints()
        {
            using (DataContainerManager dataContainerManager = new DataContainerManager())
            {
                return null;
            }
        }
    }
}