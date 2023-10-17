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
    public class MeaningController : Controller
    {
        // GET: Meanings
        public ActionResult Index()
        {
            string module = "rpm";

            ViewData["app"] = SvelteHelper.GetApp(module);
            ViewData["start"] = SvelteHelper.GetStart(module);

            return View();
        }

        #region meanings

        [BExISAuthorize]
        [JsonNetFilter]
        [HttpGet]
        public JsonResult Get()
        {
            using (var _meaningManager = new meaningManager())
            {
                List<Meaning> res = _meaningManager.getMeanings();
                return Json(res, JsonRequestBehavior.AllowGet);
            }

        }


        [BExISAuthorizeAttribute]
        [JsonNetFilter]
        [HttpPost]
        public JsonResult Create(Meaning data)
        {
            try { 

                using (var _meaningManager = new meaningManager())
                {
                    Meaning res = _meaningManager.addMeaning(data);
                    return Json(res);
                }
                    
            }
            catch(Exception ex)
            {
                throw new Exception("Meaning was not generated.", ex);
            }
        }

 

        [BExISAuthorizeAttribute]
        [JsonNetFilter]
        [HttpPost]
        public JsonResult Update(Meaning data)
        {
            try
            {
                using (var _meaningManager = new meaningManager())
                {
                    Meaning res = _meaningManager.editMeaning(data);
                    return Json(res);
                }

            }
            catch (Exception ex)
            {
                throw new Exception("Meaning was not generated.", ex);
            }
        }

        [BExISAuthorizeAttribute]
        [JsonNetFilter]
        [HttpDelete]
        public JsonResult Delete(long id)
        {
            try
            {

                using (var _meaningManager = new meaningManager())
                {
                    _meaningManager.deleteMeaning(id);
                    return Json(true);
                }

            }
            catch (Exception ex)
            {
                throw new Exception("Meaning was not generated.", ex);
            }
        }

        #endregion

        #region External Links


        [BExISAuthorize]
        [JsonNetFilter]
        [HttpGet]
        public JsonResult GetLinks()
        {
            using (var _meaningManager = new meaningManager())
            {
                List<ExternalLink> res = _meaningManager.getExternalLinks();
                return Json(res, JsonRequestBehavior.AllowGet);
            }
        }

        [BExISAuthorizeAttribute]
        [JsonNetFilter]
        [HttpPost]
        public JsonResult CreateLink(ExternalLink data)
        {
            try
            {

                using (var _meaningManager = new meaningManager())
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
        public JsonResult UpdateExternalLink(ExternalLink data)
        {
            try
            {
                using (var _meaningManager = new meaningManager())
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
        public JsonResult DeleteLink(long id)
        {
            try
            {

                using (var _meaningManager = new meaningManager())
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