using BExIS.Dlm.Entities.Party;
using BExIS.Dlm.Services.Party;
using BExIS.Modules.Sam.UI.Models;
using BExIS.Security.Entities.Requests;
using BExIS.Security.Entities.Subjects;
using BExIS.Security.Services.Authorization;
using BExIS.Security.Services.Objects;
using BExIS.Security.Services.Requests;
using BExIS.Security.Services.Utilities;
using BExIS.Utils.Config;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Telerik.Web.Mvc.Extensions;
using Vaiona.Persistence.Api;
using Vaiona.Web.Extensions;
using Vaiona.Web.Mvc.Models;

namespace BExIS.Modules.Sam.UI.Controllers
{
    public class RequestsAdminController : Controller
    {
        [HttpPost]
        public void Accept(long decisionId)
        {
            using (var decisionManager = new DecisionManager())
            using (var entityManager = new EntityManager())
            using (var uow = this.GetUnitOfWork())
            {
                decisionManager.Accept(decisionId, "");

                var requestRepository = uow.GetRepository<Request>();
                var request = requestRepository.Get(decisionId);

                if (request != null)
                {
                    var entityStore = (IEntityStore)Activator.CreateInstance(entityManager.FindById(request.Entity.Id).EntityStoreType);
                    string applicant = getPartyName(request.Applicant);
                    string emailDescionMaker = request.Decisions.FirstOrDefault().DecisionMaker.Email;

                    using (var emailService = new EmailService())
                    {
                        emailService.Send(MessageHelper.GetAcceptRequestHeader(request.Key, applicant),
                        MessageHelper.GetAcceptRequestMessage(request.Key, entityStore.GetTitleById(request.Key)),
                        new List<string> { request.Applicant.Email }, null, new List<string> { GeneralSettings.SystemEmail, emailDescionMaker }
                    );
                    }
                }
            }
        }

        public ActionResult Decisions(long entityId, string status = "")
        {
            using (var entityManager = new EntityManager())
            using (var entityPermissionManager = new EntityPermissionManager())
            using (var decisionManager = new DecisionManager())
            {
                var entityStore = (IEntityStore)Activator.CreateInstance(entityManager.FindById(entityId).EntityStoreType);

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
                    //check if the entity exist otherwise set a default text for the user;
                    string title = entityStore.Exist(m.Request.Key) ? entityStore.GetTitleById(m.Request.Key) : "Dataset currently / no longer accessible";
                    bool exist = entityStore.Exist(m.Request.Key);

                    model.Add(
                        new DecisionGridRowModel()
                        {
                            Id = m.Id,
                            RequestId = m.Request.Id,
                            Rights = string.Join(", ", entityPermissionManager.GetRightsAsync(m.Request.Rights).Result), //string.Join(",", Enum.GetNames(typeof(RightType)).Select(n => n).Where(n => (m.Request.Rights & (short)Enum.Parse(typeof(RightType), n)) > 0)),
                            Status = m.Status,
                            StatusAsText = Enum.GetName(typeof(DecisionStatus), m.Status),
                            InstanceId = m.Request.Key,
                            Title = title,
                            Applicant = getPartyName(m.Request.Applicant),
                            DecisionMaker = getPartyName(m.DecisionMaker),
                            Intention = m.Request.Intention,
                            RequestDate = m.Request.RequestDate,
                            EntityExist = exist
                        });
                }

                ViewData["entityID"] = entityId;

                return PartialView("_DecisionsAdmin", model.OrderBy(x => x.Status).ThenBy(n => n.Id));
            }
        }

        public ActionResult Index()
        {
            using (var entityManager = new EntityManager())
            {
                ViewBag.Title = PresentationModel.GetViewTitleForTenant("Manage Entity Requests and Decisions", Session.GetTenant());

                var entities =
                    entityManager.Entities.Select(e => EntityTreeViewItemModel.Convert(e, e.Parent.Id)).ToList();

                foreach (var entity in entities)
                {
                    entity.Children = entities.Where(e => e.ParentId == entity.Id).ToList();
                }

                return View(entities.AsEnumerable());
            }
        }

        [HttpPost]
        public void Reject(long requestId)
        {
            using (var decisionManager = new DecisionManager())
            using (var entityManager = new EntityManager())
            using (var uow = this.GetUnitOfWork())
            {
                try
                {
                    decisionManager.Reject(requestId, "");

                    var requestRepository = uow.GetRepository<Request>();
                    var request = requestRepository.Get(requestId);

                    if (request != null)
                    {
                        var entityStore = (IEntityStore)Activator.CreateInstance(entityManager.FindById(request.Entity.Id).EntityStoreType);
                        string applicant = getPartyName(request.Applicant);
                        string emailDescionMaker = request.Decisions.FirstOrDefault().DecisionMaker.Email;

                        using (var emailService = new EmailService())
                        {
                            emailService.Send(MessageHelper.GetRejectedRequestHeader(request.Key, applicant),
                        MessageHelper.GetRejectedRequestMessage(request.Key, entityStore.GetTitleById(request.Key)),
                        new List<string> { request.Applicant.Email }, null, new List<string> { GeneralSettings.SystemEmail, emailDescionMaker }
                        );
                        }
                    }
                }
                catch (Exception e)
                {
                    throw e;
                }
            }
        }

        [HttpPost]
        public void Withdraw(long requestId)
        {
            using (var decisionManager = new DecisionManager())
            using (var entityManager = new EntityManager())
            using (var uow = this.GetUnitOfWork())
            {
                try
                {
                    decisionManager.Withdraw(requestId);

                    var requestRepository = uow.GetRepository<Request>();
                    var request = requestRepository.Get(requestId);

                    if (request != null)
                    {
                        var entityStore = (IEntityStore)Activator.CreateInstance(entityManager.FindById(request.Entity.Id).EntityStoreType);

                        string emailDescionMaker = request.Decisions.FirstOrDefault().DecisionMaker.Email;
                        string applicant = getPartyName(request.Applicant);

                        using (var emailService = new EmailService())
                        {
                            emailService.Send(MessageHelper.GetWithdrawRequestHeader(request.Key, applicant),
                        MessageHelper.GetWithdrawRequestMessage(request.Key, entityStore.GetTitleById(request.Key), applicant),
                        new List<string> { emailDescionMaker }, null, new List<string> { GeneralSettings.SystemEmail, request.Applicant.Email }
                        );
                        }
                    }
                }
                catch (Exception e)
                {
                    throw e;
                }
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