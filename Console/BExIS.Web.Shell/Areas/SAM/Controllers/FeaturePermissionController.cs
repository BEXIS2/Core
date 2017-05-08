using BExIS.Modules.Sam.UI.Models;
using BExIS.Security.Services.Objects;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Vaiona.Web.Extensions;
using Vaiona.Web.Mvc.Models;

namespace BExIS.Modules.Sam.UI.Controllers
{
    public class FeaturePermissionController : Controller
    {
        public ActionResult Index()
        {
            ViewBag.Title = PresentationModel.GetViewTitleForTenant("Manage Features", this.Session.GetTenant());

            var featureManager = new FeatureManager();

            var features = new List<FeatureTreeViewItemModel>();

            var roots = featureManager.FindRoots();
            roots.ToList().ForEach(f => features.Add(FeatureTreeViewItemModel.Convert(f)));

            return View(features.AsEnumerable());
        }

        public void SetFeaturePublicity(long featureId, bool value)
        {
            var featureManager = new FeatureManager();

            var feature = featureManager.FindById(featureId);
            feature.IsPublic = value;

            featureManager.Update(feature);
        }
    }
}