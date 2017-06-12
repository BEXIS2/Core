using BExIS.Dlm.Entities.Data;
using BExIS.Modules.Sam.UI.Models;
using BExIS.Security.Entities.Objects;
using BExIS.Security.Entities.Subjects;
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

            var entities = entityStore.GetAllEntities();

            return View();
        }

        public ActionResult Create(CreateEntityPermissionModel model)
        {
            return View();
        }

        [GridAction(EnableCustomBinding = true)]
        public ActionResult GroupEntityPermissions_Select(long entityId, GridCommand command)
        {
            var entityPermissionManager = new EntityPermissionManager();

            // Source + Transformation - Data
            var groupEntityPermissions = entityPermissionManager.EntityPermissions.Where(m => m.Subject is Group && m.Entity.Id == entityId).ToGroupEntityPermissionGridRowModel();

            // Filtering
            var total = groupEntityPermissions.Count();

            // Sorting
            var sorted = (IQueryable<GroupEntityPermissionGridRowModel>)groupEntityPermissions.Sort(command.SortDescriptors);

            // Paging
            var paged = sorted.Skip((command.Page - 1) * command.PageSize)
                .Take(command.PageSize);

            return View(new GridModel<GroupEntityPermissionGridRowModel> { Data = paged.ToList(), Total = total });
        }

        public ActionResult Index()
        {
            ViewBag.Title = PresentationModel.GetViewTitleForTenant("Manage Entity Permissions", Session.GetTenant());

            var entities = new List<EntityTreeViewItemModel>();

            var entityManager = new EntityManager();

            entityManager.Create(new Entity() { EntityType = typeof(Dataset) });

            var roots = entityManager.FindRoots();
            roots.ToList().ForEach(e => entities.Add(EntityTreeViewItemModel.Convert(e)));

            return View(entities.AsEnumerable());
        }

        public ActionResult Permissions(long entityId)
        {
            return PartialView("_Permissions", entityId);
        }

        [GridAction(EnableCustomBinding = true)]
        public ActionResult UserEntityPermissions_Select(long entityId, GridCommand command)
        {
            var entityPermissionManager = new EntityPermissionManager();

            // Source + Transformation - Data
            var groupEntityPermissions = entityPermissionManager.EntityPermissions.Where(m => m.Subject is User && m.Entity.Id == entityId).ToUserEntityPermissionGridRowModel();

            // Filtering
            var total = groupEntityPermissions.Count();

            // Sorting
            var sorted = (IQueryable<UserEntityPermissionGridRowModel>)groupEntityPermissions.Sort(command.SortDescriptors);

            // Paging
            var paged = sorted.Skip((command.Page - 1) * command.PageSize)
                .Take(command.PageSize);

            return View(new GridModel<UserEntityPermissionGridRowModel> { Data = paged.ToList(), Total = total });
        }
    }
}