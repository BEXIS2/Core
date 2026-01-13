using BExIS.Security.Entities.Authorization;
using BExIS.Security.Entities.Objects;
using BExIS.Security.Entities.Requests;
using BExIS.Security.Entities.Subjects;
using System;
using System.Collections.Generic;
using System.Linq;
using Vaiona.Persistence.Api;

namespace BExIS.Security.Services.Requests
{
    public class DecisionManager
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="decisionId"></param>
        /// <param name="reason"></param>
        public void Accept(long decisionId, string reason)
        {
            using (var uow = this.GetUnitOfWork())
            {
                var decisionRepository = uow.GetRepository<Decision>();
                var requestRepository = uow.GetRepository<Request>();
                var entityPermissionRepository = uow.GetRepository<EntityPermission>();
                var entityRepository = uow.GetRepository<Entity>();
                var userRepository = uow.GetRepository<User>();

                var decision = decisionRepository.Get(decisionId);

                if (decision != null)
                {
                    decision.Status = DecisionStatus.Accepted;
                    decision.DecisionDate = DateTime.Now;
                    decision.Reason = reason;

                    decisionRepository.Merge(decision);
                    var mergedDecision = decisionRepository.Get(decision.Id);
                    decisionRepository.Put(mergedDecision);

                    if (decisionRepository.Query(m => m.Request.Id == decision.Request.Id).ToList()
                        .All(m => m.Status != DecisionStatus.Open))
                    {
                        var request = requestRepository.Get(decision.Request.Id);

                        if (request != null)
                        {
                            request.Status = RequestStatus.Accepted;

                            requestRepository.Merge(request);
                            var mergedRequest = requestRepository.Get(request.Id);
                            requestRepository.Put(mergedRequest);

                            var entityPermission =
                            entityPermissionRepository.Query(
                                m =>
                                    m.Subject.Id == request.Applicant.Id && m.Entity.Id == request.Entity.Id &&
                                    m.Key == request.Key).FirstOrDefault();

                            if (entityPermission != null)
                            {
                                if ((entityPermission.Rights & 1) == 0) entityPermission.Rights += 1;
                                if ((entityPermission.Rights & 2) == 0) entityPermission.Rights += 2;

                                entityPermissionRepository.Merge(entityPermission);
                                var mergedEntityPermission = entityPermissionRepository.Get(request.Id);
                                entityPermissionRepository.Put(mergedEntityPermission);
                            }
                            else
                            {
                                entityPermission = new EntityPermission()
                                {
                                    Entity = entityRepository.Get(request.Entity.Id),
                                    Key = request.Key,
                                    Rights = request.Rights,
                                    Subject = userRepository.Get(request.Applicant.Id)
                                };

                                entityPermissionRepository.Put(entityPermission);
                            }
                        }
                    }
                }

                uow.Commit();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="decisionId"></param>
        /// <returns></returns>
        public bool Delete(long decisionId)
        {
            using (var uow = this.GetUnitOfWork())
            {
                var decisionRepository = uow.GetRepository<Decision>();
                var decision = decisionRepository.Get(decisionId);
                if (decision == null)
                    return false;

                var deleted = decisionRepository.Delete(decision);
                uow.Commit();

                return deleted;
            }
        }

        public List<Decision> Get()
        {
            using (var uow = this.GetUnitOfWork())
            {
                var decisionRepository = uow.GetReadOnlyRepository<Decision>();
                return decisionRepository.Query().ToList();
            }
        }

        public List<Decision> Get(Func<Decision, bool> predicate)
        {
            using (var uow = this.GetUnitOfWork())
            {
                var decisionRepository = uow.GetReadOnlyRepository<Decision>();
                return decisionRepository.Query(predicate).ToList();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="predicate"></param>
        /// <returns></returns>
        public bool Exists(Func<Decision, bool> predicate)
        {
            using (var uow = this.GetUnitOfWork())
            {
                var decisionRepository = uow.GetReadOnlyRepository<Decision>();
                return decisionRepository.Query(predicate).Any();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="decisionId"></param>
        /// <returns></returns>
        public Decision Get(long decisionId)
        {
            using (var uow = this.GetUnitOfWork())
            {
                var decisionRepository = uow.GetReadOnlyRepository<Decision>();
                return decisionRepository.Get(decisionId);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="decisionId"></param>
        /// <param name="reason"></param>
        public void Reject(long decisionId, string reason)
        {
            using (var uow = this.GetUnitOfWork())
            {
                var decisionRepository = uow.GetRepository<Decision>();
                var requestRepository = uow.GetRepository<Request>();

                var decision = decisionRepository.Get(decisionId);

                if (decision != null)
                {
                    decision.Status = DecisionStatus.Rejected;
                    decision.DecisionDate = DateTime.Now;
                    decision.Reason = reason;

                    decisionRepository.Merge(decision);
                    var mergedDecision = decisionRepository.Get(decision.Id);
                    decisionRepository.Put(mergedDecision);

                    if (decisionRepository.Query(m => m.Request.Id == decision.Request.Id).ToList()
                        .All(m => m.Status != DecisionStatus.Open))
                    {
                        var request = requestRepository.Get(decision.Request.Id);

                        if (request != null)
                        {
                            request.Status = RequestStatus.Rejected;

                            requestRepository.Merge(request);
                            var mergedRequest = requestRepository.Get(request.Id);
                            requestRepository.Put(mergedRequest);
                        }
                    }

                    uow.Commit();
                }
            }
        }

        public void Withdraw(long requestId)
        {
            using (var uow = this.GetUnitOfWork())
            {
                var decisionRepository = uow.GetRepository<Decision>();
                var requestRepository = uow.GetRepository<Request>();

                var request = requestRepository.Get(requestId);

                if (request != null)
                {
                    request.Status = RequestStatus.Withdrawn;

                    requestRepository.Merge(request);
                    var mergedRequest = requestRepository.Get(request.Id);
                    requestRepository.Put(mergedRequest);

                    var decision = decisionRepository.Query(m => m.Request.Id == requestId).FirstOrDefault();
                    decision.Status = DecisionStatus.Withdrawn;

                    decisionRepository.Merge(decision);
                    var mergedDecision = decisionRepository.Get(decision.Id);
                    decisionRepository.Put(mergedDecision);
                }

                uow.Commit();
            }
        }

        public void Update(Decision decision)
        {
            using (var uow = this.GetUnitOfWork())
            {
                var decisionRepository = uow.GetRepository<Decision>();
                decisionRepository.Merge(decision);
                var merged = decisionRepository.Get(decision.Id);
                decisionRepository.Put(merged);
                uow.Commit();
            }
        }
    }
}