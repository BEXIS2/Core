using BExIS.Modules.Sam.UI.Models;
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
    public class RequestsController : Controller
    {
        [GridAction(EnableCustomBinding = true)]
        public ActionResult Decisions_Select(long entityId, GridCommand command)
        {
            var entityManager = new EntityManager();
            var entityStore = (IEntityStore)Activator.CreateInstance(entityManager.FindById(entityId).EntityStoreType);

            var requestManager = new RequestManager();

            // Source + Transformation - Data
            var requests = requestManager.Requests.Where(d => d.Entity.Id == entityId);

            // Filtering
            var total = requests.Count();

            // Sorting
            //var sorted = (IQueryable<UserEntityPermissionGridRowModel>)groupEntityPermissions.Sort(command.SortDescriptors);

            // Paging
            var paged = requests.Skip((command.Page - 1) * command.PageSize)
                .Take(command.PageSize);

            var results =
                paged.Select(
                        x => new RequestGridRowModel() { Applicant = x.Requester.Name, Id = x.Key, Status = x.Status, StatusName = x.Status.ToString() })
                    .ToList();

            return View(new GridModel<RequestGridRowModel> { Data = results, Total = total });
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

        public ActionResult Requests(long entityId)
        {
            return PartialView("_Requests", entityId);
        }

        [GridAction(EnableCustomBinding = true)]
        public ActionResult Requests_Select(long entityId, GridCommand command)
        {
            var entityManager = new EntityManager();
            var entityStore = (IEntityStore)Activator.CreateInstance(entityManager.FindById(entityId).EntityStoreType);

            var requestManager = new RequestManager();

            // Source + Transformation - Data
            var requests = requestManager.Requests.Where(d => d.Entity.Id == entityId);

            // Filtering
            var total = requests.Count();

            // Sorting
            //var sorted = (IQueryable<UserEntityPermissionGridRowModel>)groupEntityPermissions.Sort(command.SortDescriptors);

            // Paging
            var paged = requests.Skip((command.Page - 1) * command.PageSize)
                .Take(command.PageSize);

            var results =
                paged.Select(
                        x => new RequestGridRowModel() { Applicant = x.Requester.Name, Id = x.Key, Status = x.Status, StatusName = x.Status.ToString() })
                    .ToList();

            return View(new GridModel<RequestGridRowModel> { Data = results, Total = total });
        }
    }
}