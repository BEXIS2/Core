using BExIS.App.Bootstrap.Attributes;
using BExIS.Dlm.Entities.Meanings;
using BExIS.Dlm.Services.Meanings;
using BExIS.Modules.Rpm.UI.Helpers;
using BExIS.Modules.Rpm.UI.Models;
using BExIS.UI.Helpers;
using BExIS.UI.Models;
using BExIS.Utils.Route;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
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
                List<MeaningModel> resModel = new List<MeaningModel>();

                if (res.Any())
                {
                    res.ForEach(m => resModel.Add(MeaningsHelper.ConvertTo(m)));
                }
                
                return Json(resModel, JsonRequestBehavior.AllowGet);
            }

        }


        [BExISAuthorizeAttribute]
        [JsonNetFilter]
        [System.Web.Http.HttpPost]
        public JsonResult Create(MeaningModel data)
        {
            try {

                using (var _meaningManager = new MeaningManager())
                {
                    Meaning m = MeaningsHelper.ConvertTo(data);
                    Meaning res = _meaningManager.addMeaning(m);
                    
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
        public JsonResult Update(MeaningModel data)
        {
            try
            {
                using (var _meaningManager = new MeaningManager())
                {
                    Meaning m = MeaningsHelper.ConvertTo(data);
                    Meaning res = _meaningManager.editMeaning(m);
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
                List<ExternalLinkModel> links = new List<ExternalLinkModel>();
                res.ForEach(l => links.Add(MeaningsHelper.ConvertTo(l)));
                return Json(links, JsonRequestBehavior.AllowGet);
            }
        }

        [BExISAuthorize]
        [JsonNetFilter]
        [System.Web.Http.HttpGet]
        public JsonResult GetLinkListItems()
        {
            using (var _meaningManager = new MeaningManager())
            {

                List<ExternalLink> res = _meaningManager.getExternalLinks();
                List<ListItem> items = new List<ListItem>();
                res.ForEach(l => items.Add(MeaningsHelper.ConvertToListItem(l)));

                return Json(res, JsonRequestBehavior.AllowGet);
            }
        }



        [BExISAuthorizeAttribute]
        [JsonNetFilter]
        [System.Web.Http.HttpPost]
        public JsonResult CreateLink(ExternalLinkModel data)
        {
            try
            {
                using (var _meaningManager = new MeaningManager())
                {
                    var link = MeaningsHelper.ConvertTo(data);
                    ExternalLink res = _meaningManager.addExternalLink(link);
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
        public JsonResult UpdateExternalLink(ExternalLinkModel data)
        {
            try
            {
                using (var _meaningManager = new MeaningManager())
                {
                    var link = MeaningsHelper.ConvertTo(data);
                    ExternalLink res = _meaningManager.editExternalLink(link);
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