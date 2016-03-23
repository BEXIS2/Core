using System;
using System.Web.Mvc;
using Telerik.Web.Mvc;

using BExIS.Web.Shell.Areas.RPM.Models;

namespace BExIS.Web.Shell.Areas.RPM.Controllers
{
    public class DataStructureEditController : Controller
    {
        public ActionResult Index(long DataStructureId = 0)
        {
            return View(DataStructureId);
        }

        public ActionResult _attributeResultBinding()
        {
            return PartialView("_attributeSearchResult", new AttributePreviewModel().fill());
        }

        public ActionResult _attributeFilterBinding()
        {
            return PartialView("_attributeFilter", new AttributeFilterModel().fill());
        }

        public ActionResult _dataStructureBinding(long dataStructureId)
        {
            return PartialView("_dataStructure", new DataStructurePreviewModel().fill(dataStructureId));
        }
    }
}