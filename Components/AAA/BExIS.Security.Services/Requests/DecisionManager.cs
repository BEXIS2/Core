using BExIS.Security.Entities.Authorization;
using BExIS.Security.Entities.Objects;
using BExIS.Security.Entities.Requests;
using BExIS.Security.Entities.Subjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Vaiona.Persistence.Api;

namespace BExIS.Security.Services.Requests
{
    public class DecisionManager
    {
        public void Accept(long decisionId, string reason)
        {
            updateDecision(decisionId, DecisionStatus.Accepted, reason, RequestStatus.Accepted, grantEntityPermissions: true);
        }

        public void Reject(long decisionId, string reason)
        {
            updateDecision(decisionId, DecisionStatus.Rejected, reason, RequestStatus.Rejected, grantEntityPermissions: false);
        }

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

        public IList<Decision> Find()
        {
            using (var uow = this.GetUnitOfWork())
            {
                var decisionRepository = uow.GetReadOnlyRepository<Decision>();
                return decisionRepository.Query().ToList();
            }
        }

        public IList<Decision> Find(Expression<Func<Decision, bool>> predicate)
        {
            using (var uow = this.GetUnitOfWork())
            {
                var decisionRepository = uow.GetReadOnlyRepository<Decision>();
                return decisionRepository.Query(predicate).ToList();
            }
        }

        public bool Exists(Expression<Func<Decision, bool>> predicate)
        {
            using (var uow = this.GetUnitOfWork())
            {
                var decisionRepository = uow.GetReadOnlyRepository<Decision>();
                return decisionRepository.Query(predicate).Any();
            }
        }

        public Decision Get(long decisionId)
        {
            using (var uow = this.GetUnitOfWork())
            {
                var decisionRepository = uow.GetReadOnlyRepository<Decision>();
                return decisionRepository.Get(decisionId);
            }
        }

        public void Withdraw(long requestId)
        {
            using (var uow = this.GetUnitOfWork())
            {
                var decisionRepository = uow.GetRepository<Decision>();
                var requestRepository = uow.GetRepository<Request>();

                var request = requestRepository.Get(requestId);
                if (request == null) return;

                request.Status = RequestStatus.Withdrawn;
                requestRepository.Put(request);

                var decision = decisionRepository.Query(m => m.Request.Id == requestId).FirstOrDefault();
                if (decision == null) return;

                decision.Status = DecisionStatus.Withdrawn;
                decisionRepository.Put(decision);

                uow.Commit();
            }
        }

        public void Update(Decision decision)
        {
            using (var uow = this.GetUnitOfWork())
            {
                var decisionRepository = uow.GetRepository<Decision>();
                decisionRepository.Merge(decision);
                uow.Commit();
            }
        }

        private void updateDecision(long decisionId, DecisionStatus decisionStatus, string reason, RequestStatus requestStatus, bool grantEntityPermissions)
        {
            using (var uow = this.GetUnitOfWork())
            {
                var decisionRepository = uow.GetRepository<Decision>();
                var requestRepository = uow.GetRepository<Request>();
                var entityPermissionRepository = uow.GetRepository<EntityPermission>();
                var entityRepository = uow.GetRepository<Entity>();
                var userRepository = uow.GetRepository<User>();

                var decision = decisionRepository.Get(decisionId);
                if (decision == null) return;

                decision.Status = decisionStatus;
                decision.DecisionDate = DateTime.UtcNow;
                decision.Reason = reason;
                decisionRepository.Put(decision);

                // Prüfen, ob alle Decisions für Request geschlossen sind
                var allClosed = !decisionRepository.Query(d => d.Request.Id == decision.Request.Id).Any(d => d.Status == DecisionStatus.Open);

                if (!allClosed)
                {
                    uow.Commit();
                    return;
                }

                var request = requestRepository.Get(decision.Request.Id);
                if (request != null)
                {
                    request.Status = requestStatus;
                    requestRepository.Put(request);

                    if (grantEntityPermissions)
                    {
                        var entityPermission = entityPermissionRepository
                            .Query(ep => ep.Subject.Id == request.Applicant.Id &&
                                         ep.Entity.Id == request.Entity.Id &&
                                         ep.Key == request.Key)
                            .FirstOrDefault();

                        if (entityPermission != null)
                        {
                            entityPermission.Rights |= request.Rights;
                            entityPermissionRepository.Put(entityPermission);
                        }
                        else
                        {
                            entityPermission = new EntityPermission
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

                uow.Commit();
            }
        }
    }
}