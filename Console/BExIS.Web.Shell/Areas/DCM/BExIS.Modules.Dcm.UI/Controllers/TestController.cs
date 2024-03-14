using BExIS.Dlm.Entities.Data;
using BExIS.Dlm.Services.Administration;
using BExIS.Dlm.Services.Data;
using BExIS.Dlm.Services.DataStructure;
using BExIS.Dlm.Services.MetadataStructure;
using BExIS.Security.Services.Objects;
using BExIS.UI.Helpers;
using BExIS.Xml.Helpers;
using System;
using System.Diagnostics;
using System.Web.Mvc;

namespace BExIS.Modules.Dcm.UI.Controllers
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Reliability", "CA2000:Objekte verwerfen, bevor Bereich verloren geht", Justification = "<Ausstehend>")]
    public class TestController : Controller
    {
        // GET: Test
        public ActionResult Index(long id)
        {
            string module = "DCM";

            ViewData["app"] = SvelteHelper.GetApp(module);
            ViewData["start"] = SvelteHelper.GetStart(module);
            ViewData["id"] = id;

            return View();
        }


    }
}