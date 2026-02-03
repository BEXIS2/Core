using BExIS.Dlm.Entities.Party;
using BExIS.Security.Entities.Objects;
using BExIS.Security.Entities.Requests;
using BExIS.Security.Entities.Subjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Vaiona.Persistence.Api;
using Vaiona.Web.Mvc.Modularity;

namespace BExIS.Security.Services.Requests
{
    public class RequestManager
    {
        public void Accept(long requestId)
        {
            using (var uow = this.GetUnitOfWork())
            {
                var requestRepository = uow.GetRepository<Request>();

                var request = requestRepository.Get(requestId);
                if (request == null)
                    throw new InvalidOperationException($"Request {requestId} not found.");

                if(request.Status != RequestStatus.Open)
                    throw new InvalidOperationException($"Request {requestId} is not open and cannot be accepted.");

                request.Status = RequestStatus.Accepted;
                requestRepository.Put(request);
                uow.Commit();
            }
        }

        public Request Create(long applicantId, long entityId, long key, short rights = 1, string intention = "")
        {
            using (var uow = this.GetUnitOfWork())
            {
                var decisionRepository = uow.GetRepository<Decision>();
                var entityRepository = uow.GetReadOnlyRepository<Entity>();
                var partyRepository = uow.GetReadOnlyRepository<Party>();
                var partyRelationshipRepository = uow.GetReadOnlyRepository<PartyRelationship>();
                var partyTypeRepository = uow.GetReadOnlyRepository<PartyType>();
                var partyUserRepository = uow.GetReadOnlyRepository<PartyUser>();
                var requestRepository = uow.GetRepository<Request>();
                var userRepository = uow.GetReadOnlyRepository<User>();

                if (requestRepository.Query(r => r.Applicant.Id == applicantId && r.Entity.Id == entityId && r.Key == key).Any())
                    return null;

                var datasetPartyType = partyTypeRepository.Query(pt => pt.Title == "Dataset").FirstOrDefault();

                if (datasetPartyType == null)
                    return null;

                var datasetParty = partyRepository.Query(p => p.Name == key.ToString() && p.PartyType.Id == datasetPartyType.Id).FirstOrDefault();

                if (datasetParty == null)
                    return null;

                var ownerPartyRelationshipType = ModuleManager.GetModuleSettings("bam").GetValueByKey("OwnerPartyRelationshipType");
                var ownerRelationship = partyRelationshipRepository.Query(r => r.PartyRelationshipType.Title == ownerPartyRelationshipType && r.TargetParty.Id == datasetParty.Id).FirstOrDefault();

                if (ownerRelationship == null)
                    return null;

                var partyUser = partyUserRepository.Query(pu => pu.Party.Id == ownerRelationship.SourceParty.Id).FirstOrDefault();

                if (partyUser == null)
                    return null;

                var request = new Request
                {
                    Applicant = userRepository.Get(applicantId),
                    Entity = entityRepository.Get(entityId),
                    Key = key,
                    Rights = rights,
                    Intention = intention,
                    RequestDate = DateTime.UtcNow,
                    Status = RequestStatus.Open
                };

                requestRepository.Put(request);

                var decision = new Decision
                {
                    Request = request,
                    DecisionMaker = userRepository.Get(partyUser.UserId),
                    Status = DecisionStatus.Open
                };

                decisionRepository.Put(decision);
                uow.Commit();

                return request;
            }
        }

        public bool Delete(long requestId)
        {
            using (var uow = this.GetUnitOfWork())
            {
                var requestRepository = uow.GetRepository<Request>();
                var request = requestRepository.Get(requestId);
                if (request == null)
                    return false;

                var deleted = requestRepository.Delete(request);
                uow.Commit();

                return deleted;
            }
        }

        public bool Exists(Expression<Func<Request, bool>> predicate)
        {
            using (var uow = this.GetUnitOfWork())
            {
                var requestRepository = uow.GetReadOnlyRepository<Request>();
                return requestRepository.Query(predicate).Any();
            }
        }

        public bool Exists(long applicantId, long entityId, long key)
        {
            using (var uow = this.GetUnitOfWork())
            {
                var requestRepository = uow.GetReadOnlyRepository<Request>();

                var request =
                    requestRepository.Query(
                        m => m.Applicant.Id == applicantId && m.Entity.Id == entityId && m.Key == key).FirstOrDefault();

                return request != null;
            }
        }

        public bool Exists(long applicantId, long entityId, long key, RequestStatus status)
        {
            using (var uow = this.GetUnitOfWork())
            {
                var requestRepository = uow.GetReadOnlyRepository<Request>();

                var request =
                    requestRepository.Query(
                        m => m.Applicant.Id == applicantId && m.Entity.Id == entityId && m.Key == key && m.Status == status).FirstOrDefault();

                return request != null;
            }
        }

        public Request Get(long id)
        {
            using (var uow = this.GetUnitOfWork())
            {
                var requestRepository = uow.GetReadOnlyRepository<Request>();
                return requestRepository.Get(id);
            }
        }

        public List<Request> Find()
        {
            using (var uow = this.GetUnitOfWork())
            {
                var requestRepository = uow.GetReadOnlyRepository<Request>();
                return requestRepository.Query().ToList();
            }
        }

        public List<Request> Find(Expression<Func<Request, bool>> predicate)
        {
            using (var uow = this.GetUnitOfWork())
            {
                var requestRepository = uow.GetReadOnlyRepository<Request>();
                return requestRepository.Query(predicate).ToList();
            }
        }

        public void Reject(long requestId)
        {
            using (var uow = this.GetUnitOfWork())
            {
                var requestRepository = uow.GetRepository<Request>();

                var request = requestRepository.Get(requestId);
                
                if (request == null)
                    throw new InvalidOperationException($"Request {requestId} not found.");

                if(request.Status != RequestStatus.Open)
                    throw new InvalidOperationException($"Request {requestId} is not open and cannot be rejected.");

                request.Status = RequestStatus.Rejected;
                requestRepository.Put(request);
                uow.Commit();
            }
        }

        public void Update(Request request)
        {
            using (var uow = this.GetUnitOfWork())
            {
                var requestRepository = uow.GetRepository<Request>();
                requestRepository.Merge(request);
                uow.Commit();
            }
        }
    }
}