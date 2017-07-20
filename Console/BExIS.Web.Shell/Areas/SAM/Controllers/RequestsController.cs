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

            var decisionManager = new DecisionManager();

            // Source + Transformation - Data
            var decisions = decisionManager.Decisions.Where(d => d.Request.Entity.Id == entityId && d.DecisionMaker.Name == HttpContext.User.Identity.Name);

            var results = decisions.Select(
                m => new DecisionGridRowModel() { Id = m.Request.Id, Rights = m.Request.Rights, Status = m.Status, Applicant = m.Request.Applicant.Name });

            // Filtering
            var total = results.Count();

            return View(new GridModel<DecisionGridRowModel> { Data = results.ToList(), Total = total });
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
            var requests = requestManager.Requests.Where(r => r.Entity.Id == entityId && r.Applicant.Name == HttpContext.User.Identity.Name);

            var results = requests.Select(
                m => new RequestGridRowModel() { Id = m.Key, Rights = m.Rights, RequestStatus = m.Status });

            // Filtering
            var total = results.Count();

            return View(new GridModel<RequestGridRowModel> { Data = results.ToList(), Total = total });
        }
    }
}