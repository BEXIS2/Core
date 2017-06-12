using BExIS.Security.Entities.Objects;
using BExIS.Security.Entities.Requests;
using BExIS.Security.Entities.Subjects;
using Vaiona.Persistence.Api;

namespace BExIS.Security.Services.Requests
{
    public sealed class RequestManager
    {
        public RequestManager()
        {
            var uow = this.GetUnitOfWork();

            EntityRepository = uow.GetReadOnlyRepository<Entity>();
            UserRepository = uow.GetReadOnlyRepository<User>();
        }

        #region Data Readers

        public IReadOnlyRepository<Entity> EntityRepository { get; private set; }
        public IReadOnlyRepository<Request> RequestRepository { get; private set; }
        public IReadOnlyRepository<User> UserRepository { get; private set; }

        #endregion Data Readers

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

        public void Update(Request request)
        {
            using (var uow = this.GetUnitOfWork())
            {
                var entityRequestRepository = uow.GetRepository<Request>();
                entityRequestRepository.Put(request);
                uow.Commit();
            }
        }
    }
}