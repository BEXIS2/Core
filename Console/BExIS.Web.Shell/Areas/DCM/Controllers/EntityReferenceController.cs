using BExIS.App.Bootstrap.Attributes;
using BExIS.Dlm.Entities.Data;
using BExIS.Dlm.Services.Data;
using BExIS.Modules.Dcm.UI.Helpers;
using BExIS.Modules.Dcm.UI.Models.EntityReference;
using BExIS.Security.Entities.Authorization;
using BExIS.Security.Entities.Objects;
using BExIS.Security.Services.Authorization;
using BExIS.Security.Services.Objects;
using BExIS.Security.Services.Subjects;
using NHibernate.Util;
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
            if (hasUserRights(sourceId, sourceTypeId, RightType.Write))
            {
                EntityReferenceHelper helper = new EntityReferenceHelper();

                SetViewData(sourceId, sourceTypeId, true, false);

                return PartialView("_create", new CreateSimpleReferenceModel(sourceId, sourceTypeId, helper.CountVersions(sourceId, sourceTypeId)));
            }

            return null;
        }

        [HttpPost]
        public ActionResult Create(CreateSimpleReferenceModel model)
        {
            EntityReferenceHelper helper = new EntityReferenceHelper();
            EntityReferenceManager entityReferenceManager = new EntityReferenceManager();

            try
            {
                if (hasUserRights(model.SourceId, model.SourceTypeId, RightType.Write))
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

                ModelState.AddModelError("", "you are not authorized!");
                return PartialView("_create", model);
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

        public JsonResult GetTargets(long id)
        {
            EntityReferenceHelper helper = new EntityReferenceHelper();
            List<SelectListItem> tmp = new List<SelectListItem>();

            if (id > 0)
                helper.GetEntities(id).OrderByDescending(s => s.Id).ForEach(e => tmp.Add(new SelectListItem() { Text = e.Id + ": " + e.Title, Value = e.Id.ToString() }));

            return Json(tmp, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetTargetVersions(long id, long type)
        {
            EntityReferenceHelper helper = new EntityReferenceHelper();
            SelectList tmp = new SelectList(new List<SelectListItem>());

            if (id > 0)
                tmp = helper.GetEntityVersions(id, type);
            var tmpDecending = tmp.OrderByDescending(x => x.Value).ToList();

            return Json(tmpDecending, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>

        [HttpPost]
        public JsonResult Delete(long id)
        {
            if (id == 0) return Json(false);

            EntityReferenceManager entityReferenceManager = new EntityReferenceManager();

            try
            {
                //check if entity ref in db exist
                var entityRef = entityReferenceManager.References.FirstOrDefault(e => e.Id.Equals(id));
                if (entityRef != null)
                {
                    //check if user has rights to edit a dataset
                    if (hasUserRights(entityRef.SourceId, entityRef.SourceEntityId, RightType.Write))
                    {
                        entityReferenceManager.Delete(id);
                        return Json(true);
                    }
                    else
                    {
                        return Json("Error : " + "you are not authorized!");
                    }
                }
            }
            catch (Exception ex)
            {
                return Json("Error : " + ex.Message);
            }
            finally
            {
                entityReferenceManager.Dispose();
            }

            return Json(false);
        }

        public ActionResult Show(long sourceId, long sourceTypeId, int sourceVersion)
        {
            ReferencesModel model = new ReferencesModel();
            EntityReferenceHelper helper = new EntityReferenceHelper();

            model.Selected = helper.GetSimpleReferenceModel(sourceId, sourceTypeId, sourceVersion);
            model.TargetReferences = helper.GetTargetReferences(sourceId, sourceTypeId, sourceVersion);
            model.SourceReferences = helper.GetSourceReferences(sourceId, sourceTypeId, sourceVersion);
            model.HasEditRights = hasUserRights(sourceId, RightType.Write);

     
           return PartialView("Show", model);
        }

        public ActionResult Show2(long sourceId, long sourceTypeId, int sourceVersion)
        {
            ReferencesModel model = new ReferencesModel();
            EntityReferenceHelper helper = new EntityReferenceHelper();

            model.Selected = helper.GetSimpleReferenceModel(sourceId, sourceTypeId, sourceVersion);
            model.TargetReferences = helper.GetTargetReferences(sourceId, sourceTypeId, sourceVersion);
            model.SourceReferences = helper.GetSourceReferences(sourceId, sourceTypeId, sourceVersion);
            model.HasEditRights = hasUserRights(sourceId, RightType.Write);

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
            ViewData["ReferenceTypeHelp"] = helper.GetReferencesHelpTypes();
            //"This references is saved."
            if (isSuccess) ViewData["Success"] = "This references is saved.";

            ViewData["Init"] = init;
        }

        #endregion ViewData

        #region helpers

        public string GetUsernameOrDefault()
        {
            var username = string.Empty;
            try
            {
                username = HttpContext.User.Identity.Name;
            }
            catch { }

            return !string.IsNullOrWhiteSpace(username) ? username : "DEFAULT";
        }

        private bool hasUserRights(long instanceId, RightType rightType)
        {
            EntityManager entityManager = new EntityManager();

            try
            {
                var entity = entityManager.FindByName("Dataset");
                return hasUserRights(instanceId, entity.Id, rightType);
            }
            catch
            {
                return false;
            }
            finally
            {
                entityManager.Dispose();
            }
        }

        private bool hasUserRights(long instanceId, long entityId, RightType rightType)
        {
            EntityPermissionManager entityPermissionManager = new EntityPermissionManager();
            UserManager userManager = new UserManager();
            EntityManager entityManager = new EntityManager();

            try
            {
                #region security permissions and authorisations check

                var user = userManager.FindByNameAsync(GetUsernameOrDefault()).Result;
                if (user == null) return false;

                var entity = entityManager.FindByName("Dataset");
                if (entity == null) return false;
                return entityPermissionManager.HasEffectiveRight(user.UserName, typeof(Dataset), instanceId, rightType);

                #endregion security permissions and authorisations check
            }
            catch
            {
                return false;
            }
            finally
            {
                entityPermissionManager.Dispose();
                userManager.Dispose();
                entityManager.Dispose();
            }
        }

        #endregion helpers
    }
}