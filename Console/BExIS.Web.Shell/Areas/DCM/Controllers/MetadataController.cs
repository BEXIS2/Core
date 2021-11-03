using BExIS.App.Bootstrap.Attributes;
using BExIS.Dlm.Entities.Data;
using BExIS.Security.Entities.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace BExIS.Modules.Dcm.UI.Controllers
{
    public class MetadataController : Controller
    {
        // GET: Metadat
        public ActionResult Index()
        {
            return View();
        }

        [BExISEntityAuthorize(typeof(Dataset), "id", RightType.Write)]
        public ActionResult Start(long id, int version)
        {
            return RedirectToAction("LoadMetadata", "Form", new { entityId = id, locked = false, created = false, fromEditMode = true });
        }
    }
}