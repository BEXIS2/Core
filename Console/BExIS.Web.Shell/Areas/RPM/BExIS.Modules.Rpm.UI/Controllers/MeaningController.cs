using BExIS.App.Bootstrap.Attributes;
using BExIS.Dlm.Entities.Meanings;
using BExIS.Dlm.Services.Meanings;
using BExIS.UI.Helpers;
using BExIS.Utils.Route;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Web.Helpers;
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
        [System.Web.Http.HttpGet]
        public JsonResult Get()
        {
            using (var _meaningManager = new MeaningManager())
            {
                List<Meaning> res = _meaningManager.getMeanings();
                return Json(res, JsonRequestBehavior.AllowGet);
            }

        }


        [BExISAuthorizeAttribute]
        [JsonNetFilter]
        [System.Web.Http.HttpPost]
        public JsonResult Create(Meaning data)
        {
            try {

                using (var _meaningManager = new MeaningManager())
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
        [System.Web.Http.HttpPost]
        public JsonResult Update(Meaning data)
        {
            try
            {
                using (var _meaningManager = new MeaningManager())
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
        [System.Web.Http.HttpDelete]
        public JsonResult Delete(long id)
        {
            try
            {

                using (var _meaningManager = new MeaningManager())
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
        [System.Web.Http.HttpGet]
        public JsonResult GetLinks()
        {
            using (var _meaningManager = new MeaningManager())
            {
                List<ExternalLink> res = _meaningManager.getExternalLinks();
                return Json(res, JsonRequestBehavior.AllowGet);
            }
        }

        [BExISAuthorizeAttribute]
        [JsonNetFilter]
        [System.Web.Http.HttpPost]
        public JsonResult CreateLink(ExternalLink data)
        {
            try
            {

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
        [System.Web.Http.HttpPost]
        public JsonResult UpdateExternalLink(ExternalLink data)
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
        [System.Web.Http.HttpDelete]
        public JsonResult DeleteLink(long id)
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

        #region category prefix

        [BExISAuthorize]
        [JsonNetFilter]
        [System.Web.Http.HttpGet]
        public JsonResult GetPrefixCategory()
        {
            using (var _meaningManager = new MeaningManager())
            {
                List<PrefixCategory> res = _meaningManager.getPrefixCategory();
                return Json(res, JsonRequestBehavior.AllowGet);
            }
        }

        [BExISAuthorize]
        [JsonNetFilter]
        [System.Web.Http.HttpGet]
        public JsonResult createPrefixCategory(PrefixCategory data)
        {
            try
            {
                using (var _meaningManager = new MeaningManager())
                {
                    PrefixCategory res = _meaningManager.addPrefixCategory(data);
                    return Json(res, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                throw new Exception("External link was not generated.", ex);
            }
        }

        [BExISAuthorizeAttribute]
        [JsonNetFilter]
        [System.Web.Http.HttpPost]
        public JsonResult UpdatePrefixCategory(PrefixCategory data)
        {
            ExternalLink m = null;
            try
            {
                using (var _meaningManager = new MeaningManager())
                {
                    PrefixCategory res = _meaningManager.editPrefixCategory(data);
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
        [System.Web.Http.HttpDelete]
        public JsonResult DeletePrefixCategory(long id)
        {
            ExternalLink m = null;
            try
            {
                using (var _meaningManager = new MeaningManager())
                {
                    List<PrefixCategory> res = _meaningManager.deletePrefixCategory(id);
                    return Json(res);
                }
            }
            catch (Exception ex)
            {
                throw new Exception("External link was not generated.", ex);
            }
        }
        #endregion
    }
}