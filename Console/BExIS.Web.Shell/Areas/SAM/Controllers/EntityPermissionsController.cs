using BExIS.Modules.Sam.UI.Models;
using BExIS.Security.Services.Authorization;
using BExIS.Security.Services.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Telerik.Web.Mvc;
using Telerik.Web.Mvc.Extensions;
using Vaiona.Web.Extensions;
using Vaiona.Web.Mvc.Models;

namespace BExIS.Modules.Sam.UI.Controllers
{
    public class EntityPermissionsController : Controller
    {
        public ActionResult Create(long entityId)
        {
            var entityManager = new EntityManager();
            var entityStore = (IEntityStore)Activator.CreateInstance(entityManager.FindById(entityId).EntityStoreType);

            var entities = entityStore.GetEntities();

            return View();
        }

        public ActionResult Create(CreateEntityPermissionModel model)
        {
            return View();
        }

        [GridAction]
        public ActionResult EntityPermissions_Select(long entityId)
        {
            var entityPermissionManager = new EntityPermissionManager();
            var entityPermissions = entityPermissionManager.EntityPermissions.Where(p => p.Entity.Id == entityId).Select(p => EntityPermissionGridRowModel.Convert(p)).ToList();
            return View(new GridModel<EntityPermissionGridRowModel> { Data = entityPermissions });
        }

        public ActionResult Index()
        {
            ViewBag.Title = PresentationModel.GetViewTitleForTenant("Manage Entity Permissions", Session.GetTenant());

            var entities = new List<EntityTreeViewItemModel>();

            var entityManager = new EntityManager();
            var roots = entityManager.FindRoots();
            roots.ToList().ForEach(e => entities.Add(EntityTreeViewItemModel.Convert(e)));

            return View(entities.AsEnumerable());
        }

        public ActionResult Permissions(long entityId)
        {
            return PartialView("_Permissions", entityId);
        }
    }
}