using BExIS.App.Bootstrap.Attributes;
using BExIS.Dlm.Entities.Data;
using BExIS.Modules.Dcm.UI.Models.Edit;
using BExIS.Security.Entities.Authorization;
using System;
using System.Web.Mvc;
using Vaiona.Web.Mvc.Modularity;

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
            return RedirectToAction("load", new { id, version });
        }

        [JsonNetFilter]
        public JsonResult Load(long id, int version)
        {
            // check incoming variables
            if (id <= 0) throw new ArgumentException("id must be greater than 0");
            MetadataModel model = new MetadataModel();

            var settings = ModuleManager.GetModuleSettings("dcm");
            var useExternalMetadataForm = (bool)settings.GetValueByKey("useExternalMetadataForm");
            var externalMetadataFormUrl = settings.GetValueByKey("externalMetadataFormUrl").ToString();

            model.ExternalMetadataFormUrl = externalMetadataFormUrl;
            model.UseExternalMetadataForm = useExternalMetadataForm;

            return Json(model, JsonRequestBehavior.AllowGet);
        }

        public ActionResult LoadForm(long id, int version)
        {
            return RedirectToAction("LoadMetadata", "Form", new { entityId = id, version, locked = false, created = false, fromEditMode = true, asPartial = false });
        }
    }
}