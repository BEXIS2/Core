using BExIS.Modules.Sam.UI.Models;
using BExIS.Security.Services.Authorization;
using BExIS.Security.Services.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Telerik.Web.Mvc;
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

        public ActionResult Subjects(long key)
        {
            return PartialView("_Subjects", key);
        }

        public ActionResult Instances(long entityId)
        {
            return PartialView("_Instances", entityId);
        }

        [GridAction]
        public ActionResult Instances_Select(long entityId)
        {
            var entityManager = new EntityManager();
            var entityPermissionManager = new EntityPermissionManager();
            var instanceStore = (IEntityStore)Activator.CreateInstance(entityManager.FindById(entityId).EntityStoreType);

            var instances = instanceStore.GetEntities().Select(i => EntityInstanceGridRowModel.Convert(i, entityPermissionManager.Exists(null, entityId, i.Id))).ToList();

            return View(new GridModel<EntityInstanceGridRowModel> { Data = instances });
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