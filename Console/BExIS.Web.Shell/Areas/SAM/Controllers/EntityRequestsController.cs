using BExIS.Modules.Sam.UI.Models;
using BExIS.Security.Entities.Subjects;
using BExIS.Security.Services.Authorization;
using BExIS.Security.Services.Objects;
using BExIS.Security.Services.Requests;
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
    public class EntityRequestsController : Controller
    {
        [GridAction(EnableCustomBinding = true)]
        public ActionResult EntityDecisions_Select(long entityId, GridCommand command)
        {
            var entityManager = new EntityManager();
            var entityStore = (IEntityStore)Activator.CreateInstance(entityManager.FindById(entityId).EntityStoreType);

            var entities = entityStore.GetAllEntities();

            var decisionManager = new DecisionManager();

            // Source + Transformation - Data
            var decisions = decisionManager.Decisions.Where(d => d.Request.Entity.Id == entityId);

            // Filtering
            var total = decisions.Count();

            // Sorting
            //var sorted = (IQueryable<UserEntityPermissionGridRowModel>)groupEntityPermissions.Sort(command.SortDescriptors);
            var sorted = from d in decisions
                         join c in entities on d.Request.Key equals c.Id
                         select new RequestGridRowModel() { Id = d.Id, EntityId = c.Id, Properties = c.Properties, };

            // Paging
            var paged = sorted.Skip((command.Page - 1) * command.PageSize)
                .Take(command.PageSize);

            return View(new GridModel<RequestGridRowModel> { Data = paged, Total = total });
        }

        [GridAction(EnableCustomBinding = true)]
        public ActionResult EntityRequests_Select(long entityId, GridCommand command)
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

        public ActionResult Index()
        {
            ViewBag.Title = PresentationModel.GetViewTitleForTenant("Manage Entity Requests and Decisions", Session.GetTenant());

            var entities = new List<EntityTreeViewItemModel>();

            var entityManager = new EntityManager();

            var roots = entityManager.FindRoots();
            roots.ToList().ForEach(e => entities.Add(EntityTreeViewItemModel.Convert(e)));

            return View(entities.AsEnumerable());
        }
    }
}