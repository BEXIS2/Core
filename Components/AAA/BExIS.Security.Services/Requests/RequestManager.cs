using BExIS.Dlm.Entities.Party;
using BExIS.Security.Entities.Objects;
using BExIS.Security.Entities.Requests;
using BExIS.Security.Entities.Subjects;
using System;
using System.Linq;
using Vaiona.Persistence.Api;

namespace BExIS.Security.Services.Requests
{
    public sealed class RequestManager
    {
        public RequestManager()
        {
            var uow = this.GetUnitOfWork();

            RequestRepository = uow.GetReadOnlyRepository<Request>();
        }

        public IReadOnlyRepository<Request> RequestRepository { get; }
        public IReadOnlyRepository<Party> PartyRepository { get; }
        public IQueryable<Request> Requests => RequestRepository.Query();

        public void Create(long applicantId, long entityId, long key, int rights = 1)
        {
            using (var uow = this.GetUnitOfWork())
            {
                var requestRepository = uow.GetRepository<Request>();
                var decisionRepository = uow.GetRepository<Decision>();
                var partyRelationshipRepository = uow.GetReadOnlyRepository<PartyRelationship>();
                var partyUserRepository = uow.GetReadOnlyRepository<PartyUser>();
                var userRepository = uow.GetReadOnlyRepository<User>();
                var entityRepository = uow.GetReadOnlyRepository<Entity>();

                var firstOrDefault = partyRelationshipRepository.Query(m => m.PartyRelationshipType.Title == "Owner" && m.SecondParty.Id == key).FirstOrDefault();

                if (firstOrDefault == null) return;

                var partyUser = partyUserRepository.Get(firstOrDefault.FirstParty.Id);

                var request = new Request()
                {
                    Applicant = userRepository.Get(applicantId),
                    Entity = entityRepository.Get(entityId),
                    Key = key,
                    RequestDate = DateTime.Now,
                    Status = RequestStatus.Open
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
            }
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

        public Request FindById(long id)
        {
            return RequestRepository.Get(id);
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
    }
}