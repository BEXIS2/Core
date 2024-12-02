using BExIS.App.Bootstrap.Attributes;
using BExIS.Dlm.Entities.Meanings;
using BExIS.Dlm.Services.Meanings;
using BExIS.Modules.Rpm.UI.Helpers;
using BExIS.Modules.Rpm.UI.Models;
using BExIS.UI.Helpers;
using BExIS.UI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
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
                List<Meaning> res = _meaningManager.GetMeanings();
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
            try
            {
                using (var _meaningManager = new MeaningManager())
                {
                    Meaning m = MeaningsHelper.ConvertTo(data);
                    Meaning res = _meaningManager.AddMeaning(m);

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
        [System.Web.Http.HttpPost]
        public JsonResult Update(MeaningModel data)
        {
            try
            {
                using (var _meaningManager = new MeaningManager())
                {
                    Meaning m = MeaningsHelper.ConvertTo(data);
                    Meaning res = _meaningManager.EditMeaning(m);
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
                    _meaningManager.DeleteMeaning(id);
                    return Json(true);
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Meaning was not generated.", ex);
            }
        }

        #endregion meanings

        #region External Links

        [BExISAuthorize]
        [JsonNetFilter]
        [System.Web.Http.HttpGet]
        public JsonResult GetLinks()
        {
            using (var _meaningManager = new MeaningManager())
            {
                List<ExternalLink> res = _meaningManager.GetExternalLinks();
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
                List<ExternalLink> res = _meaningManager.GetExternalLinks();
                List<ListItem> items = new List<ListItem>();
                res.ForEach(l => items.Add(MeaningsHelper.ConvertToListItem(l)));

                return Json(res, JsonRequestBehavior.AllowGet);
            }
        }

        [JsonNetFilter]
        public JsonResult GetConstraints()
        {
            VariableHelper helper = new VariableHelper();
            List<ListItem> list = helper.GetConstraints();

            return Json(list.OrderBy(l => l.Text), JsonRequestBehavior.AllowGet);
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
                    ExternalLink res = _meaningManager.AddExternalLink(link);
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
                    ExternalLink res = _meaningManager.EditExternalLink(link);
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
                    _meaningManager.DeleteExternalLink(id);
                    return Json(true);
                }
            }
            catch (Exception ex)
            {
                throw new Exception("External link was not generated.", ex);
            }
        }

        #endregion External Links

        #region category prefix

        [BExISAuthorize]
        [JsonNetFilter]
        [System.Web.Http.HttpGet]
        public JsonResult GetPrefixCategory()
        {
            using (var _meaningManager = new MeaningManager())
            {
                List<PrefixCategory> res = _meaningManager.GetPrefixCategory();
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
                    PrefixCategory res = _meaningManager.AddPrefixCategory(data);
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
                    PrefixCategory res = _meaningManager.EditPrefixCategory(data);
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
                    List<PrefixCategory> res = _meaningManager.DeletePrefixCategory(id);
                    return Json(res);
                }
            }
            catch (Exception ex)
            {
                throw new Exception("External link was not generated.", ex);
            }
        }

        #endregion category prefix
    }
}