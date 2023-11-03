using BExIS.App.Bootstrap.Attributes;
using BExIS.Dlm.Entities.Meanings;
using BExIS.Dlm.Services.Meanings;
using BExIS.UI.Helpers;
using BExIS.Utils.Route;
using System;
using System.Collections.Generic;
using System.Web.Mvc;

namespace BExIS.Modules.Rpm.UI.Controllers
{
    public class ExternalLinkController : Controller
    {
        // GET: ExternalLink
        public ActionResult Index()
        {
            string module = "rpm";

            ViewData["app"] = SvelteHelper.GetApp(module);
            ViewData["start"] = SvelteHelper.GetStart(module);

            return View();
        }

        #region External Links


        [BExISAuthorize]
        [JsonNetFilter]
        [HttpGet]
        public JsonResult Get()
        {
            using (var _meaningManager = new MeaningManager())
            {
                List<ExternalLink> res = _meaningManager.getExternalLinks();
                return Json(res, JsonRequestBehavior.AllowGet);
            }
        }

        [BExISAuthorizeAttribute]
        [JsonNetFilter]
        [HttpPost]
        public JsonResult Create(ExternalLink data)
        {
            try
            {
                if (data == null) throw new NullReferenceException("external link should not be null.");
                if (string.IsNullOrEmpty(data.Name)) throw new NullReferenceException("Name of external link should not be null or empty.");
                if (string.IsNullOrEmpty(data.URI)) throw new NullReferenceException("Uri of external link should not be null or empty.");

                data.Name = data.Name.Trim();
                data.URI  = data.URI.Trim();

                using (var _meaningManager = new MeaningManager())
                {
                    ExternalLink res = _meaningManager.addExternalLink(data);
                    return Json(res);
                }

            }
            catch (Exception ex)
            {
                throw new Exception("External link was not generated.", ex);
            }
        }

        [BExISAuthorizeAttribute]
        [JsonNetFilter]
        [HttpPost]
        public JsonResult Update(ExternalLink data)
        {
            try
            {
                using (var _meaningManager = new MeaningManager())
                {
                    ExternalLink res = _meaningManager.editExternalLink(data);
                    return Json(res);
                }

            }
            catch (Exception ex)
            {
                throw new Exception("External link was not generated.", ex);
            }
        }



        [BExISAuthorizeAttribute]
        [JsonNetFilter]
        [HttpDelete]
        public JsonResult Delete(long id)
        {
            try
            {

                using (var _meaningManager = new MeaningManager())
                {
                    _meaningManager.deleteExternalLink(id);
                    return Json(true);
                }

            }
            catch (Exception ex)
            {
                throw new Exception("External link was not generated.", ex);
            }
        }

        #endregion;

    }
}