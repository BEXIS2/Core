using BExIS.Dlm.Entities.Party;
using BExIS.Security.Entities.Objects;
using BExIS.Security.Entities.Requests;
using BExIS.Security.Entities.Subjects;
using System;
using System.Linq;
using Vaiona.Persistence.Api;
using Vaiona.Web.Mvc.Modularity;

namespace BExIS.Security.Services.Requests
{
    public class RequestManager : IDisposable
    {
        private readonly IUnitOfWork _guow;
        private bool _isDisposed;

        public RequestManager()
        {
            _guow = this.GetIsolatedUnitOfWork();
            RequestRepository = _guow.GetReadOnlyRepository<Request>();
            PartyRepository = _guow.GetReadOnlyRepository<Party>();
        }

        ~RequestManager()
        {
            Dispose(true);
        }

        public IReadOnlyRepository<Party> PartyRepository { get; }
        public IReadOnlyRepository<Request> RequestRepository { get; }
        public IQueryable<Request> Requests => RequestRepository.Query();

        public void Accept(long requestId)
        {
            using (var uow = this.GetUnitOfWork())
            {
                var entityRequestRepository = uow.GetRepository<Request>();
                var entityDecisionRepository = uow.GetRepository<Decision>();

                var entityRequest = entityRequestRepository.Get(requestId);
                var entityDecisions = entityDecisionRepository.Query(m => m.Request.Id == requestId).ToList();

                if (entityRequest != null)
                {
                    entityRequest.Status = RequestStatus.Accepted;
                    Update(entityRequest);
                }
            }
        }

        public Request Create(long applicantId, long entityId, long key, short rights = 1, string intention = "")
        {
            using (var uow = this.GetUnitOfWork())
            {
                var requestRepository = uow.GetRepository<Request>();
                var decisionRepository = uow.GetRepository<Decision>();
                var partyRelationshipRepository = uow.GetReadOnlyRepository<PartyRelationship>();
                var partyTypeRepository = uow.GetReadOnlyRepository<PartyType>();
                var partyUserRepository = uow.GetReadOnlyRepository<PartyUser>();
                var userRepository = uow.GetReadOnlyRepository<User>();
                var entityRepository = uow.GetReadOnlyRepository<Entity>();

                var dataset_partyType = partyTypeRepository.Query(m => m.Title == "Dataset").FirstOrDefault();

                if (dataset_partyType != null)
                {
                    var dataset_party =
                        PartyRepository.Query(m => m.Name == key.ToString() && m.PartyType.Id == dataset_partyType.Id)
                            .FirstOrDefault();

                    var ownerPartyRelationshipType = ModuleManager.GetModuleSettings("bam").GetValueByKey("OwnerPartyRelationshipType");

                    if (dataset_party != null && ownerPartyRelationshipType != null)
                    {
                        var partyRelationship =
                            partyRelationshipRepository.Query(
                                    m => m.PartyRelationshipType.Title == ownerPartyRelationshipType && m.TargetParty.Id == dataset_party.Id)
                                .FirstOrDefault();

                        if (partyRelationship != null)
                        {
                            var partyUser =
                                partyUserRepository.Query(m => m.Party.Id == partyRelationship.SourceParty.Id)
                                    .FirstOrDefault();

                            if (partyUser != null)
                            {
                                var request = new Request()
                                {
                                    Applicant = userRepository.Get(applicantId),
                                    Entity = entityRepository.Get(entityId),
                                    Key = key,
                                    RequestDate = DateTime.Now,
                                    Status = RequestStatus.Open,
                                    Rights = rights,
                                    Intention = intention
                                };

                                requestRepository.Put(request);

                                var decision = new Decision()
                                {
                                    Status = DecisionStatus.Open,
                                    Request = request,
                                    DecisionMaker = userRepository.Get(partyUser.UserId)
                                };

                                decisionRepository.Put(decision);
                                uow.Commit();

                                return request;
                            }
                        }
                    }
                }
            }

            return null;
        }

        public void Create(Request request)
        {
            using (var uow = this.GetUnitOfWork())
            {
                var entityRequestRepository = uow.GetRepository<Request>();
                entityRequestRepository.Put(request);
                uow.Commit();
            }
        }

        public void Delete(Request request)
        {
            using (var uow = this.GetUnitOfWork())
            {
                var entityRequestRepository = uow.GetRepository<Request>();
                entityRequestRepository.Delete(request);
                uow.Commit();
            }
        }

        public void Dispose()
        {
            Dispose(true);
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

        public Request FindById(long id)
        {
            return RequestRepository.Get(id);
        }

        public void Reject(long requestId)
        {
        }

        public void Update(Request entity)
        {
            using (var uow = this.GetUnitOfWork())
            {
                var repo = uow.GetRepository<Request>();
                repo.Merge(entity);
                var merged = repo.Get(entity.Id);
                repo.Put(merged);
                uow.Commit();
            }
        }

        protected void Dispose(bool disposing)
        {
            if (!_isDisposed)
            {
                if (disposing)
                {
                    if (_guow != null)
                        _guow.Dispose();
                    _isDisposed = true;
                }
            }
        }
    }
}