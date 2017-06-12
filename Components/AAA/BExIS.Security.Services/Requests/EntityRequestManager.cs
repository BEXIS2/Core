using BExIS.Security.Entities.Requests;
using System.Linq;
using Vaiona.Persistence.Api;

namespace BExIS.Security.Services.Requests
{
    public class EntityRequestManager
    {
        public EntityRequestManager()
        {
            var uow = this.GetUnitOfWork();

            EntityRequestRepository = uow.GetReadOnlyRepository<EntityRequest>();
        }

        public IQueryable<EntityRequest> EntityRequests => EntityRequestRepository.Query();
        private IReadOnlyRepository<EntityRequest> EntityRequestRepository { get; }

        public void Create(EntityRequest entityRequest)
        {
            using (var uow = this.GetUnitOfWork())
            {
                var entityRequestRepository = uow.GetRepository<Request>();
                entityRequestRepository.Put(entityRequest);
                uow.Commit();
            }
        }

        public void Delete(EntityRequest entityRequest)
        {
            using (var uow = this.GetUnitOfWork())
            {
                var entityRequestRepository = uow.GetRepository<Request>();
                entityRequestRepository.Delete(entityRequest);
                uow.Commit();
            }
        }

        public EntityRequest FindById(long id)
        {
            return EntityRequestRepository.Get(id);
        }

        public void Update(EntityRequest entityRequest)
        {
            using (var uow = this.GetUnitOfWork())
            {
                var entityRequestRepository = uow.GetRepository<Request>();
                entityRequestRepository.Put(entityRequest);
                uow.Commit();
            }
        }
    }
}