using BExIS.Modules.Sam.UI.Models;
using BExIS.Security.Services.Authorization;
using BExIS.Security.Services.Objects;
using BExIS.Security.Services.Subjects;
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
        public ActionResult Subjects(long entityId, long instanceId)
        {
            return PartialView("_Subjects", new EntityInstanceModel() { EntityId = entityId, InstanceId = instanceId });
        }

        [GridAction]
        public ActionResult Subjects_Select(long entityId, long instanceId)
        {
            var subjectManager = new SubjectManager();
            var entityPermissionManager = new EntityPermissionManager();

            var subjects = subjectManager.Subjects.Select(s => EntityPermissionGridRowModel.Convert(s, entityPermissionManager.GetRights(s.Id, entityId, instanceId))).ToList();


            return View(new GridModel<EntityPermissionGridRowModel> { Data = subjects });
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