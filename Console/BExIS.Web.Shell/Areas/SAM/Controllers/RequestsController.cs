using BExIS.Modules.Sam.UI.Models;
using BExIS.Security.Services.Objects;
using BExIS.Security.Services.Requests;
using System;
using System.Linq;
using System.Web.Mvc;
using BExIS.Security.Entities.Authorization;
using BExIS.Security.Services.Authorization;
using Telerik.Web.Mvc;
using Telerik.Web.Mvc.Extensions;
using Vaiona.Web.Extensions;
using Vaiona.Web.Mvc.Models;

namespace BExIS.Modules.Sam.UI.Controllers
{
    public class RequestsController : Controller
    {
        [HttpPost]
        public void Accept(long decisionId)
        {
            var decisionManager = new DecisionManager();

            try
            {
                decisionManager.Accept(decisionId, "");
            }
            catch (Exception e)
            {
                throw e;
            }
            finally
            {
                decisionManager.Dispose();
            }
        }

        public ActionResult Decisions(long entityId)
        {
            return PartialView("_Decisions", entityId);
        }

        [GridAction(EnableCustomBinding = true)]
        public ActionResult Decisions_Select(long entityId, GridCommand command)
        {
            var entityManager = new EntityManager();
            var entityPermissionManager = new EntityPermissionManager();
            var entityStore = (IEntityStore)Activator.CreateInstance(entityManager.FindById(entityId).EntityStoreType);

            var decisionManager = new DecisionManager();

            // Source + Transformation - Data
            var decisions = decisionManager.Decisions.Where(d => d.Request.Entity.Id == entityId && d.DecisionMaker.Name == HttpContext.User.Identity.Name);

            var results = decisions.Select(
                m =>
                    new DecisionGridRowModel()
                    {
                        Id = m.Id,
                        RequestId = m.Request.Id,
                        Rights = m.Request.Rights,
                        RightsAsText = string.Join(", ", entityPermissionManager.GetRights(m.Request.Rights)), //string.Join(",", Enum.GetNames(typeof(RightType)).Select(n => n).Where(n => (m.Request.Rights & (short)Enum.Parse(typeof(RightType), n)) > 0)),
                        Status = m.Status,
                        StatusAsText = m.Status.ToString(),
                        InstanceId = m.Request.Key,
                        Title = entityStore.GetTitleById(m.Request.Key),
                        Applicant = m.Request.Applicant.Name,
                        Intention = m.Request.Intention
                    }); ;

            // Filtering
            var total = results.Count();

            return View(new GridModel<DecisionGridRowModel> { Data = results.ToList(), Total = total });
        }

        public ActionResult Index()
        {
            var entityManager = new EntityManager();

            try
            {
                ViewBag.Title = PresentationModel.GetViewTitleForTenant("Manage Entity Requests and Decisions",
                    Session.GetTenant());

                var entities =
                    entityManager.Entities.Select(e => EntityTreeViewItemModel.Convert(e, e.Parent.Id)).ToList();

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

        [HttpPost]
        public void Reject(long requestId)
        {
            var decisionManager = new DecisionManager();

            try
            {
                decisionManager.Reject(requestId, "");
            }
            catch (Exception e)
            {
                throw e;
            }
            finally
            {
                decisionManager.Dispose();
            }
        }

        public ActionResult Requests(long entityId)
        {
            return PartialView("_Requests", entityId);
        }

        public ActionResult Requests_And_Decisions(long entityId)
        {
            return PartialView("_Requests_And_Decisions", entityId);
        }

        [GridAction(EnableCustomBinding = true)]
        public ActionResult Requests_Select(long entityId, GridCommand command)
        {
            var entityManager = new EntityManager();
            var entityPermissionManager = new EntityPermissionManager();

            var entityStore = (IEntityStore)Activator.CreateInstance(entityManager.FindById(entityId).EntityStoreType);

            var requestManager = new RequestManager();

            // Source + Transformation - Data
            var requests = requestManager.Requests.Where(r => r.Entity.Id == entityId && r.Applicant.Name == HttpContext.User.Identity.Name);

            var results = requests.Select(
                m => new RequestGridRowModel()
                {
                    Id = m.Key,
                    InstanceId = m.Key,
                    Title = entityStore.GetTitleById(m.Key),
                    Rights = m.Rights,
                    RightsAsText = string.Join(", ", entityPermissionManager.GetRights(m.Rights)), //string.Join(",", Enum.GetNames(typeof(RightType)).Select(n => n).Where(n => (m.Request.Rights & (short)Enum.Parse(typeof(RightType), n)) > 0)),
                    RequestStatus = m.Status,
                    Intention = m.Intention
                });

            // Filtering
            var total = results.Count();

            return View(new GridModel<RequestGridRowModel> { Data = results.ToList(), Total = total });
        }
    }
}