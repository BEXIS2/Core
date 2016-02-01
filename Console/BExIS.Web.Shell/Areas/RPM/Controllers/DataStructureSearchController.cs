using System;
using System.Web.Mvc;
using Telerik.Web.Mvc;

using BExIS.Web.Shell.Areas.RPM.Models;

namespace BExIS.Web.Shell.Areas.RPM.Controllers
{
    public class DataStructureSearchController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        [GridAction]
        public ActionResult _dataStructureResultGridBinding(long[] previewIds, string searchTerms)
        {
            return View(new GridModel(new DataStructureResultsModel(previewIds, searchTerms).dataStructureResults));
        }

        [GridAction]
        public ActionResult _dataStructurePreviewGridBinding(long dataStructureId)
        {
            return View(new GridModel(new StructuredDataStructurePreviewModel(dataStructureId).VariablePreviews));
        }

        public ActionResult _dataStructurePreviewBinding(long dataStructureId, bool structured)
        {
            return PartialView("_dataStructuredStructurePreview", dataStructureId);
        }

        public ActionResult _dataStructureResultBinding(string searchTerms)
        {
            return PartialView("_datatructureSearchResult", searchTerms);
        }

    }

}
