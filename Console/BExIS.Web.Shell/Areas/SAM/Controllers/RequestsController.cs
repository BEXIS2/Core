using BExIS.Modules.Sam.UI.Models;
using BExIS.Security.Services.Objects;
using BExIS.Security.Services.Requests;
using System;
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
        public ActionResult Decisions(long entityId)
        {
            return PartialView("_Decisions", entityId);
        }

        [GridAction(EnableCustomBinding = true)]
        public ActionResult Decisions_Select(long entityId, GridCommand command)
        {
            var entityManager = new EntityManager();
            var entityStore = (IEntityStore)Activator.CreateInstance(entityManager.FindById(entityId).EntityStoreType);

            var decisionManager = new DecisionManager();

            // Source + Transformation - Data
            var decisions = decisionManager.Decisions.Where(d => d.Request.Entity.Id == entityId); //&& d.DecisionMaker.Name == HttpContext.User.Identity.Name);

            var results = decisions.Select(
                m => new DecisionGridRowModel() { Id = m.Id, RequestId = m.Request.Id, Rights = m.Request.Rights, Status = m.Status, Applicant = m.Request.Applicant.Name });

            // Filtering
            var total = results.Count();

            return View(new GridModel<DecisionGridRowModel> { Data = results.ToList(), Total = total });
        }

        public ActionResult Requests_And_Decisions(long entityId)
        {
            return PartialView("_Requests_And_Decisions", entityId);
        }

        public ActionResult Index()
        {
            var entityManager = new EntityManager();

            try
            {
                ViewBag.Title = PresentationModel.GetViewTitleForTenant("Manage Entity Requests and Decisions", Session.GetTenant());

                var entities = entityManager.Entities.Select(e => EntityTreeViewItemModel.Convert(e, e.Parent.Id)).ToList();

                foreach (var entity in entities)
                {
                    entity.Children = entities.Where(e => e.ParentId == entity.Id).ToList();
                }

                return View(entities.AsEnumerable());
            }
            finally
            {
                entityManager.Dispose();
            }
        }

        public ActionResult Requests(long entityId)
        {
            return PartialView("_Requests", entityId);
        }

        [HttpPost]
        public void Accept(long requestId)
        {
            var requestManager = new RequestManager();

            try
            {
                requestManager.FindByIdAsync(userId).Result;
            }
            finally
            {
                requestManager.Dispose();
            }
        }

        [HttpPost]
        public void Reject(long requestId)
        {
            var requestManager = new RequestManager();

            try
            {
                requestManager.A
            }
            finally
            {
                requestManager.Dispose();
            }
        }

        [GridAction(EnableCustomBinding = true)]
        public ActionResult Requests_Select(long entityId, GridCommand command)
        {
            var entityManager = new EntityManager();
            var entityStore = (IEntityStore)Activator.CreateInstance(entityManager.FindById(entityId).EntityStoreType);

            var requestManager = new RequestManager();

            // Source + Transformation - Data
            var requests = requestManager.Requests.Where(r => r.Entity.Id == entityId); // && r.Applicant.Name == HttpContext.User.Identity.Name);

            var results = requests.Select(
                m => new RequestGridRowModel() { Id = m.Key, Rights = m.Rights, RequestStatus = m.Status });

            // Filtering
            var total = results.Count();

            return View(new GridModel<RequestGridRowModel> { Data = results.ToList(), Total = total });
        }
    }
}