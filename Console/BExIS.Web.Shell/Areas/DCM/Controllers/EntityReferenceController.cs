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

            // ViewData for Title
            ViewData["Title"] = helper.GetEntityTitle(sourceId, sourceTypeId);
            // ViewData for enitity type list
            ViewData["TargetType"] = helper.GetEntityTypes();
            ViewData["Target"] = new SelectList(new List<SelectListItem>(), "Text", "Value");
            ViewData["Init"] = true;

            return View("Create", new CreateSimpleReferenceModel(sourceId, sourceTypeId));
        }

        public JsonResult GetTargets(long id)
        {
            EntityReferenceHelper helper = new EntityReferenceHelper();
            List<SelectListItem> tmp = new List<SelectListItem>();

            if (id > 0)
                helper.GetEntities(id).ForEach(e => tmp.Add(new SelectListItem() { Text = e.Title, Value = e.Id.ToString() }));

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
                if (!ModelState.IsValid) return PartialView("Create", model);

                EntityReference entityReference = helper.Convert(model);
                entityReferenceManager.Create(entityReference);

                ViewData["Title"] = helper.GetEntityTitle(model.SourceId, model.SourceTypeId);
                ViewData["TargetType"] = helper.GetEntityTypes();
                ViewData["Target"] = new SelectList(new List<SelectListItem>(), "Text", "Value");
                ViewData["Success"] = "This references is saved.";
                ViewData["Init"] = false;
                return PartialView("Create", model);
            }
            finally
            {
                entityReferenceManager.Dispose();
            }
        }
    }
}