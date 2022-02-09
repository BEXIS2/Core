﻿using BExIS.Modules.DDM.UI.Models;
using BExIS.Security.Entities.Requests;
using BExIS.Security.Entities.Subjects;
using BExIS.Security.Services.Authorization;
using BExIS.Security.Services.Objects;
using BExIS.Security.Services.Requests;
using BExIS.Security.Services.Utilities;
using BExIS.Utils.Config;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web.Mvc;
using Vaiona.IoC;
using Vaiona.Persistence.Api;
using Vaiona.Web.Extensions;
using Vaiona.Web.Mvc.Models;

namespace BExIS.Modules.Ddm.UI.Controllers
{
    public class RequestsManageController : Controller
    {
        public ActionResult Decisions(long entityId)
        {
            using (var entityManager = new EntityManager())
            using (var entityPermissionManager = new EntityPermissionManager())
            using (var decisionManager = new DecisionManager())
            {
                var entityStore = (IEntityStore)Activator.CreateInstance(entityManager.FindById(entityId).EntityStoreType);

                // Source + Transformation - Data
                var decisions = decisionManager.Decisions.Where(d => d.Request.Entity.Id == entityId && d.DecisionMaker.Name == HttpContext.User.Identity.Name);

                List<DecisionGridRowModel> model = new List<DecisionGridRowModel>();

                foreach (var m in decisions)
                {
                    // add the descicion to the model if the entity exist in the database
                    // exclude when enity was deleted
                    if (entityStore.Exist(m.Request.Key))
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
                                Intention = m.Request.Intention,
                                RequestDate = m.Request.RequestDate
                            });
                    }
                }
                return PartialView("_Decisions", model.OrderBy(x => x.Status).ThenBy(n => n.Id));
            }
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
            using (var entityManager = new EntityManager())
            using (var entityPermissionManager = new EntityPermissionManager())
            using (var decisionManager = new DecisionManager())
            using (var uow = this.GetUnitOfWork())
            {
                try
                {
                    decisionManager.Accept(decisionId, "");

                    var es = new EmailService();
                    var requestRepository = uow.GetRepository<Request>();
                    var request = requestRepository.Get(decisionId);

                    if (request != null)
                    {
                        var entityStore = (IEntityStore)Activator.CreateInstance(entityManager.FindById(request.Entity.Id).EntityStoreType);
                        string applicant = getPartyName(request.Applicant);

                        es.Send(MessageHelper.GetAcceptRequestHeader(request.Key, applicant),
                            MessageHelper.GetAcceptRequestMessage(request.Key, entityStore.GetTitleById(request.Key)),
                            new List<string> { request.Applicant.Email }, null, new List<string> { GeneralSettings.SystemEmail }
                        );
                    }
                }
                catch (Exception e)
                {
                    throw e;
                }
            }
        }

        [HttpPost]
        public void Reject(long requestId)
        {
            using (var entityManager = new EntityManager())
            using (var entityPermissionManager = new EntityPermissionManager())
            using (var decisionManager = new DecisionManager())
            using (var uow = this.GetUnitOfWork())
            {
                try
                {
                    decisionManager.Reject(requestId, "");

                    var es = new EmailService();
                    var requestRepository = uow.GetRepository<Request>();
                    var request = requestRepository.Get(requestId);

                    if (request != null)
                    {
                        var entityStore = (IEntityStore)Activator.CreateInstance(entityManager.FindById(request.Entity.Id).EntityStoreType);
                        string applicant = getPartyName(request.Applicant);

                        es.Send(MessageHelper.GetRejectedRequestHeader(request.Key, applicant),
                        MessageHelper.GetRejectedRequestMessage(request.Key, entityStore.GetTitleById(request.Key)),
                        new List<string> { request.Applicant.Email }, null, new List<string> { GeneralSettings.SystemEmail }
                        );
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

                    var es = new EmailService();
                    var requestRepository = uow.GetRepository<Request>();
                    var request = requestRepository.Get(requestId);

                    if (request != null)
                    {
                        var entityStore = (IEntityStore)Activator.CreateInstance(entityManager.FindById(request.Entity.Id).EntityStoreType);

                        string emailDescionMaker = request.Decisions.FirstOrDefault().DecisionMaker.Email;
                        string applicant = getPartyName(request.Applicant);

                        es.Send(MessageHelper.GetWithdrawRequestHeader(request.Key, applicant),
                        MessageHelper.GetWithdrawRequestMessage(request.Key, entityStore.GetTitleById(request.Key), applicant),
                        new List<string> { emailDescionMaker }, null, new List<string> { GeneralSettings.SystemEmail }
                        );
                    }
                }
                catch (Exception e)
                {
                    throw e;
                }
            }
        }

        public ActionResult Requests_And_Decisions(long entityId)
        {
            return PartialView("_Requests_And_Decisions", entityId);
        }

        public ActionResult Requests(long entityId)
        {
            using (var entityManager = new EntityManager())
            using (var entityPermissionManager = new EntityPermissionManager())
            using (var requestManager = new RequestManager())
            {
                var entityStore = (IEntityStore)Activator.CreateInstance(entityManager.FindById(entityId).EntityStoreType);

                // Source + Transformation - Data
                var requests = requestManager.Requests.Where(r => r.Entity.Id == entityId && r.Applicant.Name == HttpContext.User.Identity.Name);

                List<RequestGridRowModel> model = new List<RequestGridRowModel>();

                foreach (var m in requests)
                {
                    // add the descicion to the model if the entity exist in the database
                    // exclude when enity was deleted
                    if (entityStore.Exist(m.Key))
                    {
                        model.Add(
                            new RequestGridRowModel()
                            {
                                Id = m.Id,
                                InstanceId = m.Key,
                                Title = entityStore.GetTitleById(m.Key),
                                Rights = string.Join(", ", entityPermissionManager.GetRights(m.Rights)), //string.Join(",", Enum.GetNames(typeof(RightType)).Select(n => n).Where(n => (m.Request.Rights & (short)Enum.Parse(typeof(RightType), n)) > 0)),
                                RequestStatus = Enum.GetName(typeof(RequestStatus), m.Status),
                                Intention = m.Intention,
                                RequestDate = m.RequestDate
                            }
                            );
                    }
                }

                return PartialView("_Requests", model);
            }
        }

        private string getPartyName(User user)
        {
            using (var uow = this.GetUnitOfWork())
            {
                if (user != null)
                {
                    return user.DisplayName;
                }
            }
            return user.Name;
        }
    }
}