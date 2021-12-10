using BExIS.App.Bootstrap.Attributes;
using BExIS.Dlm.Entities.Data;
using BExIS.Security.Entities.Authorization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Vaiona.Utils.Cfg;

namespace BExIS.Modules.Dcm.UI.Controllers
{
    public class ValidationController : Controller
    {
        // GET: Validation
        public ActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// validation view need to load the svelte builded script
        /// </summary>
        /// <returns></returns>
        [BExISEntityAuthorize(typeof(Dataset), "id", RightType.Write)]
        public ActionResult Start(long id, int version = 0)
        {
            string filepath = Path.Combine(AppConfiguration.AppRoot, "Areas/DCM/Scripts/svelte/validation.js");
            return File(filepath, "application/javascript");
        }
    }
}