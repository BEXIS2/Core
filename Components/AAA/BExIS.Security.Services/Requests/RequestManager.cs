using BExIS.Security.Entities.Requests;
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
        public IQueryable<Request> Requests => RequestRepository.Query();

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