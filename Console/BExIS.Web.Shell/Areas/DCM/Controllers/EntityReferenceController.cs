using BExIS.Modules.Dcm.UI.Helpers;
using BExIS.Modules.Dcm.UI.Models.EntityReference;
using BExIS.Security.Entities.Objects;
using BExIS.Security.Services.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace BExIS.Modules.Dcm.UI.Controllers
{
    public class EntityReferenceController : Controller
    {
        // GET: EntityReference
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Create(long sourceId, long sourceTypeId)
        {
            EntityReferenceHelper helper = new EntityReferenceHelper();

            SetViewData(sourceId, sourceTypeId, true, false);

            return PartialView("_create", new CreateSimpleReferenceModel(sourceId, sourceTypeId, helper.CountVersions(sourceId, sourceTypeId)));
        }

        public JsonResult GetTargets(long id)
        {
            EntityReferenceHelper helper = new EntityReferenceHelper();
            List<SelectListItem> tmp = new List<SelectListItem>();

            if (id > 0)
                helper.GetEntities(id).ForEach(e => tmp.Add(new SelectListItem() { Text = e.Title, Value = e.Id.ToString() }));

            return Json(tmp, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetTargetVersions(long id, long type)
        {
            EntityReferenceHelper helper = new EntityReferenceHelper();
            SelectList tmp = new SelectList(new List<SelectListItem>());

            if (id > 0)
                tmp = helper.GetEntityVersions(id, type);

            return Json(tmp, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult Create(CreateSimpleReferenceModel model)
        {
            EntityReferenceHelper helper = new EntityReferenceHelper();
            EntityReferenceManager entityReferenceManager = new EntityReferenceManager();

            try
            {
                SetViewData(model.SourceId, model.SourceTypeId, false, true);

                if (!ModelState.IsValid)
                {
                    return PartialView("_create", model);
                }

                EntityReference entityReference = helper.Convert(model);
                entityReferenceManager.Create(entityReference);

                // if successfuly created a entity link return a json true
                return Json(true);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message.ToString());
                return PartialView("_create", model);
            }
            finally
            {
                entityReferenceManager.Dispose();
            }
        }

        [HttpPost]
        public JsonResult Delete(long id)
        {
            if (id == 0) return Json(false);

            EntityReferenceManager entityReferenceManager = new EntityReferenceManager();

            try
            {
                entityReferenceManager.Delete(id);

                return Json(true);
            }
            catch (Exception ex)
            {
                return Json("Error : " + ex.Message);
            }
            finally
            {
                entityReferenceManager.Dispose();
            }
        }

        public ActionResult Show(long sourceId, long sourceTypeId, int sourceVersion)
        {
            ReferencesModel model = new ReferencesModel();
            EntityReferenceHelper helper = new EntityReferenceHelper();

            model.Selected = helper.GetSimpleReferenceModel(sourceId, sourceTypeId, sourceVersion);
            model.TargetReferences = helper.GetTargetReferences(sourceId, sourceTypeId, sourceVersion);
            model.SourceReferences = helper.GetSourceReferences(sourceId, sourceTypeId, sourceVersion);

            return PartialView("Show", model);
        }

        public ActionResult Show2(long sourceId, long sourceTypeId, int sourceVersion)
        {
            ReferencesModel model = new ReferencesModel();
            EntityReferenceHelper helper = new EntityReferenceHelper();

            model.Selected = helper.GetSimpleReferenceModel(sourceId, sourceTypeId, sourceVersion);
            model.TargetReferences = helper.GetTargetReferences(sourceId, sourceTypeId, sourceVersion);
            model.SourceReferences = helper.GetSourceReferences(sourceId, sourceTypeId, sourceVersion);

            return View("Show", model);
        }

        #region ViewData

        //test
        private void SetViewData(long id, long type, bool init, bool isSuccess)
        {
            EntityReferenceHelper helper = new EntityReferenceHelper();

            // ViewData for Title
            ViewData["Title"] = helper.GetEntityTitle(id, type);
            ViewData["Version"] = helper.CountVersions(id, type);
            // ViewData for enitity type list
            ViewData["TargetType"] = helper.GetEntityTypes();
            ViewData["Target"] = new SelectList(new List<SelectListItem>(), "Text", "Value");
            ViewData["TargetVersion"] = new SelectList(new List<SelectListItem>(), "Text", "Value");
            ViewData["ReferenceType"] = helper.GetReferencesTypes();
            //"This references is saved."
            if (isSuccess) ViewData["Success"] = "This references is saved.";

            ViewData["Init"] = init;
        }

        #endregion ViewData
    }
}