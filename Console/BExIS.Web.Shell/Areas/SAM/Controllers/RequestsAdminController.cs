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
using BExIS.Security.Entities.Requests;
using BExIS.Security.Entities.Subjects;
using Vaiona.Persistence.Api;
using BExIS.Dlm.Services.Party;
using BExIS.Dlm.Entities.Party;
using BExIS.Security.Services.Utilities;
using System.Collections.Generic;
using System.Configuration;

namespace BExIS.Modules.Sam.UI.Controllers
{
    public class RequestsAdminController : Controller
    {
        public ActionResult Decisions(long entityId, string status = "")
        {
            var entityManager = new EntityManager();
            var entityPermissionManager = new EntityPermissionManager();
            var entityStore = (IEntityStore)Activator.CreateInstance(entityManager.FindById(entityId).EntityStoreType);

            var decisionManager = new DecisionManager();
            IQueryable<Decision> decisions = null;

            if (status == "Open")
            {
                ViewData["status"] = "Open";
                decisions = decisionManager.Decisions.Where(d => d.Request.Entity.Id == entityId && d.Request.Status == 0);
            }
            else
            {
                decisions = decisionManager.Decisions.Where(d => d.Request.Entity.Id == entityId);
            }
           

            List<DecisionGridRowModel> model = new List<DecisionGridRowModel>();

            foreach (var m in decisions)
            {
                model.Add(
                    new DecisionGridRowModel()
                    {
                        Id = m.Id,
                        RequestId = m.Request.Id,
                        Rights = string.Join(", ", entityPermissionManager.GetRights(m.Request.Rights)), //string.Join(",", Enum.GetNames(typeof(RightType)).Select(n => n).Where(n => (m.Request.Rights & (short)Enum.Parse(typeof(RightType), n)) > 0)),
                        Status = m.Status,
                        StatusAsText = Enum.GetName(typeof(DecisionStatus), m.Status),
                        InstanceId = m.Request.Key,
                        Title = entityStore.GetTitleById(m.Request.Key),
                        Applicant = getPartyName(m.Request.Applicant),
                        DecisionMaker = getPartyName(m.DecisionMaker),
                        Intention = m.Request.Intention,
                        RequestDate = m.Request.RequestDate
                    });


            }
            ViewData["entityID"] = entityId;

            return PartialView("_DecisionsAdmin", model.OrderBy(x=>x.Status).ThenBy(n => n.Id));
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
        public void Accept(long decisionId)
        {
            var decisionManager = new DecisionManager();

            try
            {
                decisionManager.Accept(decisionId, "");

                var es = new EmailService();
                var entityManager = new EntityManager();

                using (var uow = this.GetUnitOfWork())
                {
                    var requestRepository = uow.GetRepository<Request>();
                    var request = requestRepository.Get(decisionId);

                    if (request != null)
                    {
                        var entityStore = (IEntityStore)Activator.CreateInstance(entityManager.FindById(request.Entity.Id).EntityStoreType);
                        string applicant = getPartyName(request.Applicant);
                        string emailDescionMaker = request.Decisions.FirstOrDefault().DecisionMaker.Email;

                        es.Send(MessageHelper.GetAcceptRequestHeader(request.Key, applicant),
                            MessageHelper.GetAcceptRequestMessage(request.Key, "title"),
                            new List<string> { request.Applicant.Email }, null, new List<string> { ConfigurationManager.AppSettings["SystemEmail"] , emailDescionMaker }
                        );
                    }
                }

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

        [HttpPost]
        public void Reject(long requestId)
        {
            var decisionManager = new DecisionManager();

            try
            {
                decisionManager.Reject(requestId, "");

                var es = new EmailService();
                var entityManager = new EntityManager();

                using (var uow = this.GetUnitOfWork())
                {
                    var requestRepository = uow.GetRepository<Request>();
                    var request = requestRepository.Get(requestId);

                    if (request != null)
                    {
                        var entityStore = (IEntityStore)Activator.CreateInstance(entityManager.FindById(request.Entity.Id).EntityStoreType);
                        string applicant = getPartyName(request.Applicant);
                        string emailDescionMaker = request.Decisions.FirstOrDefault().DecisionMaker.Email;

                        es.Send(MessageHelper.GetRejectedRequestHeader(request.Key, applicant),
                        MessageHelper.GetRejectedRequestMessage(request.Key, entityStore.GetTitleById(request.Key)),
                        new List<string> { request.Applicant.Email }, null, new List<string> { ConfigurationManager.AppSettings["SystemEmail"] , emailDescionMaker }
                        );
                    }
                }
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

        [HttpPost]
        public void Withdraw(long requestId)
        {
            var decisionManager = new DecisionManager();

            try
            {
                decisionManager.Withdraw(requestId);

                var es = new EmailService();
                var entityManager = new EntityManager();

                using (var uow = this.GetUnitOfWork())
                {
                    var requestRepository = uow.GetRepository<Request>();
                    var request = requestRepository.Get(requestId);

                    if (request != null)
                    {
                        var entityStore = (IEntityStore)Activator.CreateInstance(entityManager.FindById(request.Entity.Id).EntityStoreType);

                        string emailDescionMaker = request.Decisions.FirstOrDefault().DecisionMaker.Email;
                        string applicant = getPartyName(request.Applicant);

                        es.Send(MessageHelper.GetWithdrawRequestHeader(request.Key, applicant),
                        MessageHelper.GetWithdrawRequestMessage(request.Key, entityStore.GetTitleById(request.Key), applicant),
                        new List<string> { emailDescionMaker }, null, new List<string> { ConfigurationManager.AppSettings["SystemEmail"], request.Applicant.Email }
                        );
                    }
                }
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

        private string getPartyName(User user)
        {
            using (var uow = this.GetUnitOfWork())
            using (var partyManager = new PartyManager())
            {
                if (user != null)
                {
                    Party party = partyManager.GetPartyByUser(user.Id);
                    if (party != null)
                    {
                        return party.Name;
                    }
                }
            }
            return user.Name;
        }
    }
}