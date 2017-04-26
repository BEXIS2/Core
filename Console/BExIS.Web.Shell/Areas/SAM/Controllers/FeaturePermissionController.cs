using BExIS.Modules.Sam.UI.Models;
using BExIS.Security.Services.Authorization;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Telerik.Web.Mvc;

namespace BExIS.Modules.Sam.UI.Controllers
{
    public class FeaturePermissionController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        [GridAction]
        public ActionResult Subjects_Select(GridActionAttribute filters)
        {
            FeaturePermissionManager featurePermissionManager = new FeaturePermissionManager();
            List<FeaturePermissionGridRowModel> featurePermissions = featurePermissionManager.FeaturePermissions.Select(f => FeaturePermissionGridRowModel.Convert(f)).ToList();

            return View(new GridModel<FeaturePermissionGridRowModel> { Data = featurePermissions });
        }
    }
}