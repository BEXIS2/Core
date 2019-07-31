using BExIS.Modules.Dcm.UI.Helpers.EntityReference;
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
            ViewData["Types"] = helper.GetEntityTypes();

            List<SelectListItem> tmp = new List<SelectListItem>();

            ViewData["Target"] = new SelectList(tmp, "Text", "Value");

            return View("Create", new CreateSimpleReferenceModel(sourceId, sourceTypeId));
        }

        public JsonResult GetTargets(string type)
        {
            List<SelectListItem> tmp = new List<SelectListItem>();
            tmp.Add(new SelectListItem() { Text = "a", Value = "1" });
            tmp.Add(new SelectListItem() { Text = "b", Value = "2" });
            tmp.Add(new SelectListItem() { Text = "sample1", Value = "4" });
            tmp.Add(new SelectListItem() { Text = "sample2", Value = "5" });

            return Json(tmp);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult Create(CreateSimpleReferenceModel model)
        {
            EntityReferenceManager entityReferenceManager = new EntityReferenceManager();

            try
            {
                if (!ModelState.IsValid) return PartialView("Create", model);

                EntityReference entityReference = new EntityReference();

                return PartialView("Create", model);
            }
            finally
            {
                entityReferenceManager.Dispose();
            }
        }
    }
}