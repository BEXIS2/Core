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

        public IQueryable<EntityRequest> Entities => EntityRequestRepository.Query();
        public IReadOnlyRepository<EntityRequest> EntityRequestRepository { get; }

        public void Create(EntityRequest entityRequest)
        {
            using (var uow = this.GetUnitOfWork())
            {
                var entityRequestRepository = uow.GetRepository<EntityRequest>();
                entityRequestRepository.Put(entityRequest);
                uow.Commit();
            }
        }

        public void Delete(EntityRequest entityRequest)
        {
            using (var uow = this.GetUnitOfWork())
            {
                var entityRequestRepository = uow.GetRepository<EntityRequest>();
                entityRequestRepository.Delete(entityRequest);
                uow.Commit();
            }
        }

        public EntityRequest FindByIdAsync(long entityRequestId)
        {
            return EntityRequestRepository.Get(entityRequestId);
        }

        public void Update(EntityRequest entityRequest)
        {
            using (var uow = this.GetUnitOfWork())
            {
                var entityRequestRepository = uow.GetRepository<EntityRequest>();
                entityRequestRepository.Put(entityRequest);
                uow.Commit();
            }
        }
    }
}