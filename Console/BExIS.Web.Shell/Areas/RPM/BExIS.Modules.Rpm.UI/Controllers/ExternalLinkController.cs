using BExIS.App.Bootstrap.Attributes;
using BExIS.Dlm.Entities.Meanings;
using BExIS.Dlm.Services.Meanings;
using BExIS.Modules.Rpm.UI.Helpers;
using BExIS.Modules.Rpm.UI.Models;
using BExIS.UI.Helpers;
using BExIS.UI.Models;
using BExIS.Utils.Route;
using System;
using System.Collections.Generic;
using System.Linq;
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


        [BExISAuthorize]
        [JsonNetFilter]
        [System.Web.Http.HttpGet]
        public JsonResult GetPrefixListItems()
        {
            using (var _meaningManager = new MeaningManager())
            {

                List<ExternalLink> res = _meaningManager.getPrefixes();
                List<PrefixListItem> items = new List<PrefixListItem>();
                res.ForEach(l => items.Add(MeaningsHelper.ConvertToPrefixListItem(l)));

                return Json(items, JsonRequestBehavior.AllowGet);
            }
        }

        [BExISAuthorize]
        [JsonNetFilter]
        [System.Web.Http.HttpGet]
        public JsonResult GetLinkTypes()
        {
            var types = Enum.GetValues(typeof(ExternalLinkType));
            List<ListItem> items = new List<ListItem>();

            foreach (var type in types)
            {
                ListItem item = new ListItem();
                item.Id = (Int32)type;
                item.Text = type.ToString();
                items.Add(item);
            }

            return Json(items, JsonRequestBehavior.AllowGet);
        }

        [BExISAuthorizeAttribute]
        [JsonNetFilter]
        [HttpPost]
        public JsonResult Create(ExternalLinkModel data)
        {
            try
            {
                if (data == null) throw new NullReferenceException("external link should not be null.");
                if (string.IsNullOrEmpty(data.Name)) throw new NullReferenceException("Name of external link should not be null or empty.");
                if (string.IsNullOrEmpty(data.Uri)) throw new NullReferenceException("Uri of external link should not be null or empty.");

                data.Name = data.Name.Trim();
                data.Uri  = data.Uri.Trim();

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
        [HttpPost]
        public JsonResult Update(ExternalLinkModel data)
        {
            try
            {
                ExternalLink link = null;

                using (var _meaningManager = new MeaningManager())
                {
                    link = MeaningsHelper.ConvertTo(data);
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

        [BExISAuthorize]
        [JsonNetFilter]
        [HttpGet]
        public JsonResult GetPrefixCategoriesAsList()
        {
            using (var _meaningManager = new MeaningManager())
            {
                List<PrefixCategoryListItem> list = new List<PrefixCategoryListItem>();
                List<PrefixCategory> res = _meaningManager.getPrefixCategory();
                if (res.Any()) res.ForEach(x => list.Add(new PrefixCategoryListItem(x.Id, x.Name, x.Description)));
                return Json(list, JsonRequestBehavior.AllowGet);
            }
        }

        #endregion;

        #region prefix category

        // GET: ExternalLink
        public ActionResult PrefixCategory()
        {
            string module = "rpm";

            ViewData["app"] = SvelteHelper.GetApp(module);
            ViewData["start"] = SvelteHelper.GetStart(module);

            return View("PrefixCategory");
        }

        [BExISAuthorize]
        [JsonNetFilter]
        [HttpGet]
        public JsonResult GetPrefixCategories()
        {
            using (var _meaningManager = new MeaningManager())
            {
                List<PrefixCategory> res = _meaningManager.getPrefixCategory();
                return Json(res, JsonRequestBehavior.AllowGet);
            }
        }

        [BExISAuthorize]
        [JsonNetFilter]
        [HttpPost]
        public JsonResult CreatePrefixCategories(PrefixCategory data)
        {
            if (data == null) throw new NullReferenceException("prefix category should not be null.");
            if (string.IsNullOrEmpty(data.Name)) throw new NullReferenceException("Name of prefix category should not be null or empty.");

            using (var _meaningManager = new MeaningManager())
            {
                _meaningManager.addPrefixCategory(data);
                return Json(true);
            }

            return Json(false);
        }

        [BExISAuthorize]
        [JsonNetFilter]
        [HttpPost]
        public JsonResult updatePrefixCategories(PrefixCategory data)
        {
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
                throw new Exception("Prefix Category was not generated.", ex);
            }
        }

        [BExISAuthorizeAttribute]
        [JsonNetFilter]
        [HttpDelete]
        public JsonResult DeletePrefixCategory(long id)
        {
            try
            {

                using (var _meaningManager = new MeaningManager())
                {
                    _meaningManager.deletePrefixCategory(id);
                    return Json(true);
                }

            }
            catch (Exception ex)
            {
                throw new Exception("Prefix Category was not generated.", ex);
            }
        }

        #endregion

    }
}